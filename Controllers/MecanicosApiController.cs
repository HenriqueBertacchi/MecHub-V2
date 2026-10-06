using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using System.Security.Claims;

namespace MecHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class MecanicosApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MecanicosApiController(AppDbContext context)
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

        // GET: api/mecanicos/me
        [HttpGet("me")]
        public async Task<IActionResult> ObterMeuPerfil()
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var mecanico = await _context.mecanico
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == mecanicoId);

            if (mecanico == null)
                return NotFound(new { mensagem = "Mecânico não encontrado." });

            return Ok(new
            {
                id = mecanico.Id,
                telefone = mecanico.Telefone,
                usuario = new
                {
                    id = mecanico.Usuario?.Id,
                    nome = mecanico.Usuario?.Nome,
                    email = mecanico.Usuario?.Email,
                    tipoLogin = mecanico.Usuario?.TipoLogin.ToString()
                }
            });
        }

        // PUT: api/mecanicos/me
        [HttpPut("me")]
        public async Task<IActionResult> AtualizarMeuPerfil([FromBody] AtualizarPerfilRequest request)
        {
            var mecanicoId = ObterMecanicoIdDoToken();

            var mecanico = await _context.mecanico
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == mecanicoId);

            if (mecanico == null)
                return NotFound(new { mensagem = "Mecânico não encontrado." });

            // Atualiza o telefone do mecânico
            mecanico.Telefone = request.Telefone;

            // Atualiza o nome do usuário (se veio)
            if (!string.IsNullOrWhiteSpace(request.Nome) && mecanico.Usuario != null)
            {
                mecanico.Usuario.Nome = request.Nome;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Perfil atualizado com sucesso.",
                id = mecanico.Id,
                telefone = mecanico.Telefone,
                usuario = new
                {
                    id = mecanico.Usuario?.Id,
                    nome = mecanico.Usuario?.Nome,
                    email = mecanico.Usuario?.Email,
                    tipoLogin = mecanico.Usuario?.TipoLogin.ToString()
                }
            });
        }

        public class AtualizarPerfilRequest
        {
            public string? Nome { get; set; }
            public string? Telefone { get; set; }
        }
    }
}