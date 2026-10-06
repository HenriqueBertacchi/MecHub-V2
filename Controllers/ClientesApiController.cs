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
    public class ClientesApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesApiController(AppDbContext context)
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

        // GET: api/clientes
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var clientes = await _context.cliente
                .Where(c => c.MecanicoId == mecanicoId)
                .OrderBy(c => c.Nome)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Email,
                    c.Telefone,
                    c.Cpf
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var cliente = await _context.cliente
                .Where(c => c.Id == id && c.MecanicoId == mecanicoId)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Email,
                    c.Telefone,
                    c.Cpf
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            return Ok(cliente);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] ClienteRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { mensagem = "Nome é obrigatório." });

            if (string.IsNullOrWhiteSpace(request.Cpf))
                return BadRequest(new { mensagem = "CPF é obrigatório." });

            // Valida se já existe cliente com esse CPF
            var cpfExiste = await _context.cliente
                .AnyAsync(c => c.Cpf == request.Cpf && c.MecanicoId == mecanicoId);

            if (cpfExiste)
                return BadRequest(new { mensagem = "Já existe um cliente com esse CPF." });

            // Valida se já existe cliente com esse email
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailExiste = await _context.cliente
                    .AnyAsync(c => c.Email == request.Email && c.MecanicoId == mecanicoId);

                if (emailExiste)
                    return BadRequest(new { mensagem = "Já existe um cliente com esse e-mail." });
            }

            var cliente = new Cliente
            {
                Nome = request.Nome.Trim(),
                Cpf = request.Cpf.Trim(),
                Telefone = request.Telefone?.Trim() ?? "",
                Email = request.Email?.Trim() ?? "",
                MecanicoId = mecanicoId
            };

            _context.cliente.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Cliente criado com sucesso.",
                id = cliente.Id
            });
        }

        // PUT: api/clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var cliente = await _context.cliente
                .FirstOrDefaultAsync(c => c.Id == id && c.MecanicoId == mecanicoId);

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { mensagem = "Nome é obrigatório." });

            if (string.IsNullOrWhiteSpace(request.Cpf))
                return BadRequest(new { mensagem = "CPF é obrigatório." });

            // Valida CPF duplicado (outro cliente)
            var cpfExiste = await _context.cliente
                .AnyAsync(c => c.Cpf == request.Cpf && c.MecanicoId == mecanicoId && c.Id != id);

            if (cpfExiste)
                return BadRequest(new { mensagem = "Já existe outro cliente com esse CPF." });

            // Valida email duplicado (outro cliente)
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailExiste = await _context.cliente
                    .AnyAsync(c => c.Email == request.Email && c.MecanicoId == mecanicoId && c.Id != id);

                if (emailExiste)
                    return BadRequest(new { mensagem = "Já existe outro cliente com esse e-mail." });
            }

            cliente.Nome = request.Nome.Trim();
            cliente.Cpf = request.Cpf.Trim();
            cliente.Telefone = request.Telefone?.Trim() ?? "";
            cliente.Email = request.Email?.Trim() ?? "";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Cliente atualizado com sucesso.",
                id = cliente.Id
            });
        }

        // DELETE: api/clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var cliente = await _context.cliente
                .FirstOrDefaultAsync(c => c.Id == id && c.MecanicoId == mecanicoId);

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            // Verifica se o cliente tem veículos ou ordens vinculadas
            var temVeiculos = await _context.veiculo.AnyAsync(v => v.ClienteId == id);
            var temOrdens = await _context.ordem_servico.AnyAsync(o => o.ClienteId == id);

            if (temVeiculos || temOrdens)
                return BadRequest(new
                {
                    mensagem = "Não é possível excluir. Este cliente possui veículos ou ordens vinculadas."
                });

            _context.cliente.Remove(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Cliente excluído com sucesso." });
        }

        public class ClienteRequest
        {
            public string Nome { get; set; } = string.Empty;
            public string Cpf { get; set; } = string.Empty;
            public string? Telefone { get; set; }
            public string? Email { get; set; }
        }
    }
}