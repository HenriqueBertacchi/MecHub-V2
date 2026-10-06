using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using MecHub.Models;
using System.Security.Claims;

namespace MecHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VeiculosApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VeiculosApiController(AppDbContext context)
        {
            _context = context;
        }

        private int ObterMecanicoIdDoToken()
        {
            var claim = User.FindFirstValue("MecanicoId");
            if (string.IsNullOrWhiteSpace(claim))
                throw new UnauthorizedAccessException("MecanicoId não encontrado no token.");
            return int.Parse(claim);
        }

        // GET: api/veiculos
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var veiculos = await _context.veiculo
                .Where(v => v.MecanicoId == mecanicoId)
                .OrderBy(v => v.Placa)
                .Select(v => new
                {
                    v.Id,
                    v.Placa,
                    v.Marca,
                    v.Modelo,
                    v.Cor,
                    v.AnoFabricacao,
                    v.ClienteId,
                    ClienteNome = v.Cliente != null ? v.Cliente.Nome : null,
                    StatusAtual = v.StatusAtual.ToString()
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        // GET: api/veiculos/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var veiculo = await _context.veiculo
                .Where(v => v.Id == id && v.MecanicoId == mecanicoId)
                .Select(v => new
                {
                    v.Id,
                    v.Placa,
                    v.Marca,
                    v.Modelo,
                    v.Cor,
                    v.AnoFabricacao,
                    v.ClienteId,
                    ClienteNome = v.Cliente != null ? v.Cliente.Nome : null,
                    StatusAtual = (int)v.StatusAtual,
                    StatusAtualTexto = v.StatusAtual.ToString(),
                    v.ObservacaoStatus
                })
                .FirstOrDefaultAsync();

            if (veiculo == null)
                return NotFound(new { mensagem = "Veículo não encontrado." });

            return Ok(veiculo);
        }

        // POST: api/veiculos
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] VeiculoRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            if (string.IsNullOrWhiteSpace(request.Placa))
                return BadRequest(new { mensagem = "Placa é obrigatória." });

            if (string.IsNullOrWhiteSpace(request.Marca))
                return BadRequest(new { mensagem = "Marca é obrigatória." });

            if (string.IsNullOrWhiteSpace(request.Modelo))
                return BadRequest(new { mensagem = "Modelo é obrigatório." });

            var placaNormalizada = request.Placa.Trim().ToUpper();

            // Valida placa duplicada
            var placaExiste = await _context.veiculo
                .AnyAsync(v => v.Placa == placaNormalizada && v.MecanicoId == mecanicoId);

            if (placaExiste)
                return BadRequest(new { mensagem = "Já existe um veículo com essa placa." });

            // Valida se o cliente pertence ao mecânico
            var clienteExiste = await _context.cliente
                .AnyAsync(c => c.Id == request.ClienteId && c.MecanicoId == mecanicoId);

            if (!clienteExiste)
                return BadRequest(new { mensagem = "Cliente inválido." });

            var veiculo = new Veiculo
            {
                Placa = placaNormalizada,
                Marca = request.Marca.Trim(),
                Modelo = request.Modelo.Trim(),
                Cor = request.Cor?.Trim() ?? "",
                AnoFabricacao = request.AnoFabricacao,
                ClienteId = request.ClienteId,
                MecanicoId = mecanicoId,
                StatusAtual = (StatusVeiculoEnum)request.StatusAtual,
                ObservacaoStatus = request.ObservacaoStatus,
                DataCriacao = DateTime.Now,
                DataAtualizacaoStatus = DateTime.Now
            };

            _context.veiculo.Add(veiculo);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Veículo criado com sucesso.",
                id = veiculo.Id
            });
        }

        // PUT: api/veiculos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] VeiculoRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var veiculo = await _context.veiculo
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound(new { mensagem = "Veículo não encontrado." });

            if (string.IsNullOrWhiteSpace(request.Placa))
                return BadRequest(new { mensagem = "Placa é obrigatória." });

            var placaNormalizada = request.Placa.Trim().ToUpper();

            // Valida placa duplicada (outro veículo)
            var placaExiste = await _context.veiculo
                .AnyAsync(v => v.Placa == placaNormalizada && v.MecanicoId == mecanicoId && v.Id != id);

            if (placaExiste)
                return BadRequest(new { mensagem = "Já existe outro veículo com essa placa." });

            // Valida cliente
            var clienteExiste = await _context.cliente
                .AnyAsync(c => c.Id == request.ClienteId && c.MecanicoId == mecanicoId);

            if (!clienteExiste)
                return BadRequest(new { mensagem = "Cliente inválido." });

            veiculo.Placa = placaNormalizada;
            veiculo.Marca = request.Marca.Trim();
            veiculo.Modelo = request.Modelo.Trim();
            veiculo.Cor = request.Cor?.Trim() ?? "";
            veiculo.AnoFabricacao = request.AnoFabricacao;
            veiculo.ClienteId = request.ClienteId;
            veiculo.StatusAtual = (StatusVeiculoEnum)request.StatusAtual;
            veiculo.ObservacaoStatus = request.ObservacaoStatus;
            veiculo.DataAtualizacaoStatus = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Veículo atualizado com sucesso.",
                id = veiculo.Id
            });
        }

        // DELETE: api/veiculos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var veiculo = await _context.veiculo
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound(new { mensagem = "Veículo não encontrado." });

            // Verifica se tem ordens vinculadas
            var temOrdens = await _context.ordem_servico.AnyAsync(o => o.VeiculoId == id);

            if (temOrdens)
                return BadRequest(new
                {
                    mensagem = "Não é possível excluir. Este veículo possui ordens de serviço vinculadas."
                });

            _context.veiculo.Remove(veiculo);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Veículo excluído com sucesso." });
        }

        public class VeiculoRequest
        {
            public string Placa { get; set; } = string.Empty;
            public string Marca { get; set; } = string.Empty;
            public string Modelo { get; set; } = string.Empty;
            public string? Cor { get; set; }
            public int AnoFabricacao { get; set; }
            public int ClienteId { get; set; }
            public int StatusAtual { get; set; } = 1; // PreAvaliacao
            public string? ObservacaoStatus { get; set; }
        }
    }
}