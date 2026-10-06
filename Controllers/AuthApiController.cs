using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using MecHub.Models;
using MecHub.Services;

namespace MecHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AuthApiController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
                return BadRequest(new { mensagem = "E-mail e senha são obrigatórios." });

            var usuario = await _context.usuario
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
                return Unauthorized(new { mensagem = "Usuário não encontrado." });

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.Senha,
                request.Senha
            );

            if (resultado == PasswordVerificationResult.Failed)
                return Unauthorized(new { mensagem = "Senha inválida." });

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.Senha = _passwordHasher.HashPassword(usuario, request.Senha);
                await _context.SaveChangesAsync();
            }

            // 🚨 BUSCA O MECÂNICO VINCULADO AO USUÁRIO
            var mecanico = await _context.mecanico
                .FirstOrDefaultAsync(m => m.UsuarioId == usuario.Id);

            if (mecanico == null)
            {
                // Se o usuário ainda não tem mecânico, cria um (igual o site faz)
                mecanico = new Mecanico { UsuarioId = usuario.Id };
                _context.mecanico.Add(mecanico);
                await _context.SaveChangesAsync();
            }

            var tipoLogin = usuario.TipoLogin.ToString();

            var token = _tokenService.GerarToken(
                usuario.Email,
                usuario.Nome,
                tipoLogin,
                mecanico.Id   // 🚨 PASSA O MECANICOID PRO TOKEN
            );

            return Ok(new
            {
                token,
                expiraEm = DateTime.UtcNow.AddHours(8),
                usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    TipoLogin = tipoLogin,
                    usuario.DataCriacao,
                    mecanicoId = mecanico.Id,
                    mecanicoTelefone = mecanico.Telefone
                }
            });
        }
    }
}