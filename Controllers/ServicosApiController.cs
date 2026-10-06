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
    public class ServicosApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicosApiController(AppDbContext context)
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

        // GET: api/servicos
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var servicos = await _context.servico
                .Where(s => s.MecanicoId == mecanicoId)
                .OrderBy(s => s.Descricao)
                .Select(s => new
                {
                    s.Id,
                    s.Descricao,
                    s.Valor,
                    s.Tipo
                })
                .ToListAsync();

            return Ok(servicos);
        }

        // GET: api/servicos/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var servico = await _context.servico
                .Where(s => s.Id == id && s.MecanicoId == mecanicoId)
                .Select(s => new
                {
                    s.Id,
                    s.Descricao,
                    s.Valor,
                    s.Tipo
                })
                .FirstOrDefaultAsync();

            if (servico == null)
                return NotFound(new { mensagem = "Serviço não encontrado." });

            return Ok(servico);
        }

        // POST: api/servicos
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] ServicoRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            if (string.IsNullOrWhiteSpace(request.Descricao))
                return BadRequest(new { mensagem = "Descrição é obrigatória." });

            if (request.Valor <= 0)
                return BadRequest(new { mensagem = "Valor deve ser maior que zero." });

            if (string.IsNullOrWhiteSpace(request.Tipo))
                return BadRequest(new { mensagem = "Tipo é obrigatório." });

            var servico = new Servico
            {
                Descricao = request.Descricao.Trim(),
                Valor = request.Valor,
                Tipo = request.Tipo.Trim(),
                MecanicoId = mecanicoId
            };

            _context.servico.Add(servico);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Serviço criado com sucesso.",
                id = servico.Id
            });
        }

        // PUT: api/servicos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ServicoRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var servico = await _context.servico
                .FirstOrDefaultAsync(s => s.Id == id && s.MecanicoId == mecanicoId);

            if (servico == null)
                return NotFound(new { mensagem = "Serviço não encontrado." });

            if (string.IsNullOrWhiteSpace(request.Descricao))
                return BadRequest(new { mensagem = "Descrição é obrigatória." });

            if (request.Valor <= 0)
                return BadRequest(new { mensagem = "Valor deve ser maior que zero." });

            if (string.IsNullOrWhiteSpace(request.Tipo))
                return BadRequest(new { mensagem = "Tipo é obrigatório." });

            servico.Descricao = request.Descricao.Trim();
            servico.Valor = request.Valor;
            servico.Tipo = request.Tipo.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Serviço atualizado com sucesso.",
                id = servico.Id
            });
        }

        // DELETE: api/servicos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var servico = await _context.servico
                .FirstOrDefaultAsync(s => s.Id == id && s.MecanicoId == mecanicoId);

            if (servico == null)
                return NotFound(new { mensagem = "Serviço não encontrado." });

            // Verifica se tem ordens usando esse serviço
            var temOrdens = await _context.item_ordem_servico.AnyAsync(i => i.ServicoId == id);

            if (temOrdens)
                return BadRequest(new
                {
                    mensagem = "Não é possível excluir. Este serviço está sendo usado em ordens de serviço."
                });

            _context.servico.Remove(servico);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Serviço excluído com sucesso." });
        }

        public class ServicoRequest
        {
            public string Descricao { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public string Tipo { get; set; } = string.Empty;
        }
    }
}