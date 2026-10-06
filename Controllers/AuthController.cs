// Recursos principais do ASP.NET Core MVC
using Microsoft.AspNetCore.Mvc;

// Recursos de autenticação
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

// Usado para validar senha com hash
using Microsoft.AspNetCore.Identity;

// Usado para consultas assíncronas com Entity Framework
using Microsoft.EntityFrameworkCore;

// Namespaces do seu projeto
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;
using MecHub.Services;

// Usado para trabalhar com Claims do usuário logado
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace MecHub.Controllers
{

    [AllowAnonymous]
    public class AuthController : Controller
    {
        // Contexto do banco de dados
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        private int ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                throw new UnauthorizedAccessException("MecanicoId não encontrado na sessão.");

            return int.Parse(mecanicoId);
        }

        // Injeção de dependência do AppDbContext
        public AuthController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Apenas uma rota simples para testar o login com Google
        [HttpGet]
        public IActionResult TestLogin()
        {
            return Content("<a href='/Auth/LoginGoogle'>Login com Google</a>", "text/html");
        }

        // Exibe a tela de login local
        [HttpGet]
        public async Task<IActionResult> LoginLocal()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var mecanicoId = User.FindFirstValue("MecanicoId");

                if (string.IsNullOrWhiteSpace(mecanicoId))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return View();
                }

                return RedirectToAction("Index", "Mecanico");
            }

            return View();
        }

        // Recebe os dados do formulário de login local
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginLocal(LoginViewModel model)
        {
            // Verifica se os campos do formulário são válidos
            if (!ModelState.IsValid)
                return View(model);

            // Busca o usuário pelo e-mail informado
            var usuario = await _context.usuario
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // Se não encontrar o usuário, retorna erro genérico
            if (usuario == null)
            {
                ModelState.AddModelError("", "Email inválido");
                return View(model);
            }

            // Instancia o verificador de senha com hash
            var hasher = new PasswordHasher<Usuario>();

            // Compara a senha digitada com a senha criptografada salva no banco
            var resultado = hasher.VerifyHashedPassword(
                usuario,
                usuario.Senha,
                model.Senha
            );

            // Se a senha estiver errada, retorna erro genérico
            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Senha inválida.");
                return View(model);
            }

            // Busca o mecânico vinculado ao usuário.
            // Se ainda não existir, cria automaticamente.
            var mecanico = await ObterOuCriarMecanico(usuario);

            // Marca esse usuário como login local
            usuario.TipoLogin = TipoLoginEnum.Local;

            // Salva possíveis alterações no banco
            await _context.SaveChangesAsync();

            // Cria a sessão do usuário com cookie e Claims
            await CriarSessaoUsuario(usuario, mecanico, model.LembrarMe);

            // Redireciona para a área principal do mecânico
            if (string.IsNullOrWhiteSpace(mecanico.Telefone))
                return RedirectToAction("CompletarPerfil", "Mecanico");

            return RedirectToAction("Index", "Mecanico");
        }

        // Inicia o fluxo de login com Google
        [HttpGet]
        public IActionResult LoginGoogle()
        {
            // Define para onde o Google deve redirecionar após autenticar
            var redirectUrl = Url.Action("GoogleResponse", "Auth", null, Request.Scheme);

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            // Redireciona o usuário para a tela de login do Google
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Recebe a resposta do Google após o login
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            // Lê os dados retornados pelo Google
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            // Se o Google não autenticou corretamente, volta para o login local
            if (!result.Succeeded || result.Principal == null)
            {
                return RedirectToAction(nameof(LoginLocal));
            }

            // Obtém os dados principais enviados pelo Google
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var nome = result.Principal.FindFirstValue(ClaimTypes.Name);

            // Se não vier e-mail ou ID do Google, não prossegue
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleId))
            {
                return RedirectToAction(nameof(LoginLocal));
            }

            // Tenta encontrar usuário pelo GoogleId ou pelo e-mail
            var usuario = await _context.usuario
                .FirstOrDefaultAsync(u => u.IdGoogle == googleId || u.Email == email);

            // Se não existir usuário, cria um novo
            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Nome = nome ?? email,
                    Email = email,
                    IdGoogle = googleId,
                    TipoLogin = TipoLoginEnum.Google
                };

                _context.usuario.Add(usuario);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Se já existir, atualiza o vínculo com o Google
                usuario.IdGoogle = googleId;
                usuario.TipoLogin = TipoLoginEnum.Google;

                // Se o nome estiver vazio, preenche com o nome vindo do Google
                if (string.IsNullOrWhiteSpace(usuario.Nome))
                    usuario.Nome = nome ?? email;

                await _context.SaveChangesAsync();
            }

            // Busca ou cria o mecânico vinculado a esse usuário
            var mecanico = await ObterOuCriarMecanico(usuario);

            // Cria a sessão final da aplicação com UserId e MecanicoId
            await CriarSessaoUsuario(usuario, mecanico, true);

            // Redireciona para a área principal do mecânico
            if (string.IsNullOrWhiteSpace(mecanico.Telefone))
                return RedirectToAction("CompletarPerfil", "Mecanico");

            return RedirectToAction("Index", "Mecanico");
        }

        // Finaliza a sessão do usuário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Remove o cookie de autenticação
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Impede o navegador de reaproveitar páginas protegidas em cache
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Volta para a tela de login
            return RedirectToAction("Index", "Home");
        }

        // Busca o registro de mecânico vinculado ao usuário.
        // Se não existir, cria automaticamente.
        private async Task<Mecanico> ObterOuCriarMecanico(Usuario usuario)
        {
            // Procura mecânico pelo UsuarioId
            var mecanico = await _context.mecanico
                .FirstOrDefaultAsync(m => m.UsuarioId == usuario.Id);

            // Se já existir, retorna
            if (mecanico != null)
                return mecanico;

            // Se não existir, cria um novo mecânico vinculado ao usuário
            mecanico = new Mecanico
            {
                UsuarioId = usuario.Id
            };

            _context.mecanico.Add(mecanico);
            await _context.SaveChangesAsync();

            return mecanico;
        }

        // Cria o cookie de autenticação da aplicação
        private async Task CriarSessaoUsuario(Usuario usuario, Mecanico mecanico, bool lembrarMe)
        {
            // Claims são informações gravadas no cookie do usuário logado.
            // Elas permitem recuperar rapidamente dados como UserId e MecanicoId nos controllers.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome ?? ""),
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                new Claim("UserId", usuario.Id.ToString()),
                new Claim("MecanicoId", mecanico.Id.ToString())
            };

            // Cria a identidade do usuário usando autenticação por cookie
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            // Cria o principal, que representa o usuário autenticado
            var principal = new ClaimsPrincipal(identity);


            // Grava o cookie de autenticação no navegador
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    // Se lembrarMe for true, o cookie persiste mesmo após fechar o navegador
                    IsPersistent = lembrarMe,

                    // Define o tempo de expiração do cookie
                    ExpiresUtc = lembrarMe
                        ? DateTimeOffset.UtcNow.AddDays(7)
                        : DateTimeOffset.UtcNow.AddHours(8)
                }
            );
        }

        [HttpGet]
        public IActionResult EsqueciSenha()
        {
            return View();
        }

        // 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueciSenha(EsqueciSenhaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _context.usuario
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null)
            {
                TempData["Mensagem"] = "Se o e-mail estiver cadastrado, enviaremos as instruções.";
                return RedirectToAction("LoginLocal", "Auth");
            }

            var tokenBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes);

            usuario.ResetSenhaToken = token;
            usuario.ResetSenhaExpiraEm = DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            var link = Url.Action(
                "ResetarSenha",
                "Auth",
                new { email = usuario.Email, token = token },
                protocol: "https",
                host: "mechubapp.com.br"
            );

            try
            {
                await _emailService.EnviarEmailAsync(
                    usuario.Email,
                    "Redefinição de senha - MecHub",
                    $"Olá! Clique no link abaixo para redefinir sua senha:\n\n{link}\n\nEsse link expira em 30 minutos."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO SMTP COMPLETO:");
                Console.WriteLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER EXCEPTION:");
                    Console.WriteLine(ex.InnerException.ToString());
                }

                ModelState.AddModelError("", "Não foi possível enviar o e-mail agora. Verifique as configurações SMTP.");
                return View(model);
            }

            TempData["Mensagem"] = "Se o e-mail estiver cadastrado, enviaremos as instruções.";
            return RedirectToAction("LoginLocal", "Auth");
        }

        [HttpGet]
        public IActionResult ResetarSenha(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction("LoginLocal", "Auth");

            var model = new ResetarSenhaViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetarSenha(ResetarSenhaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _context.usuario
                .FirstOrDefaultAsync(u =>
                    u.Email == model.Email &&
                    u.ResetSenhaToken == model.Token &&
                    u.ResetSenhaExpiraEm > DateTime.UtcNow
                );

            if (usuario == null)
            {
                ModelState.AddModelError("", "Link inválido ou expirado.");
                return View(model);
            }

            var hasher = new PasswordHasher<Usuario>();
            usuario.Senha = hasher.HashPassword(usuario, model.NovaSenha);

            usuario.ResetSenhaToken = null;
            usuario.ResetSenhaExpiraEm = null;

            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Senha redefinida com sucesso. Faça login novamente.";
            return RedirectToAction("LoginLocal", "Auth");
        }
    }
}
