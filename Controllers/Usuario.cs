using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace MecHub.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 LISTAR
        [HttpGet]
        public IActionResult Index()
        {
            var usuarios = _context.usuario
                .Select(u => new UsuarioListViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    TipoLogin = u.TipoLogin
                })
                .ToList();

            return View(usuarios);
        }
        private int ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                throw new UnauthorizedAccessException("MecanicoId não encontrado na sessão.");

            return int.Parse(mecanicoId);
        }

        // 🔹 DETALHE
        [HttpGet]
        public IActionResult Detalhe(int id)
        {
            var usuario = _context.usuario.Find(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // 🔹 CRIAR (GET)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        // 🔹 CRIAR (POST)
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(UsuarioCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var emailExiste = _context.usuario.Any(u => u.Email == model.Email);

            if (emailExiste)
            {
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
                return View(model);
            }

            var hasher = new PasswordHasher<Usuario>();

            var usuario = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email,
                DataCriacao = DateTime.Now,
                TipoLogin = TipoLoginEnum.Local
            };

            usuario.Senha = hasher.HashPassword(usuario, model.Senha);

            try
            {
                _context.usuario.Add(usuario);
                _context.SaveChanges();

                var mecanico = new Mecanico
                {
                    UsuarioId = usuario.Id,
                    Telefone = model.Telefone
                };

                _context.mecanico.Add(mecanico);
                _context.SaveChanges();

                return RedirectToAction("LoginLocal", "Auth");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao criar usuário.");
                return View(model);
            }
        }

        // 🔹 EDITAR (GET)
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var usuario = _context.usuario.Find(id);

            if (usuario == null)
                return NotFound();

            var model = new UsuarioEditViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
                // Senha normalmente NÃO volta para edição direta
            };

            return View(model);
        }

        // 🔹 EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, UsuarioEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var usuario = _context.usuario.Find(id);

            if (usuario == null)
                return NotFound();

            usuario.Nome = model.Nome;
            usuario.Email = model.Email;
            // usuario.Senha = model.Senha;

            try
            {
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao atualizar usuário.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TrocarSenha(int id, UsuarioEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var usuario = _context.usuario.Find(id);

            if (usuario == null)
                return NotFound();

            usuario.Nome = model.Nome;
            usuario.Email = model.Email;
            // usuario.Senha = model.Senha;

            try
            {
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao atualizar usuário.");
                return View(model);
            }
        }


        // 🔹 DELETAR (GET - confirmação)
        [HttpGet]
        public IActionResult Deletar(int id)
        {
            var usuario = _context.usuario.Find(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // 🔹 DELETAR (POST)
        [HttpPost]
        [ActionName("Deletar")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletarConfirmado(int id)
        {
            var usuario = _context.usuario.Find(id);

            if (usuario == null)
                return NotFound();

            try
            {
                _context.usuario.Remove(usuario);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao deletar usuário.");
                return View(usuario);
            }
        }
    }
}
