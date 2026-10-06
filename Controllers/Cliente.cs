using Microsoft.AspNetCore.Mvc;
using MecHub.Data;
using MecHub.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MecHub.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MecHub.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;

        private int ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                throw new UnauthorizedAccessException("MecanicoId não encontrado na sessão.");

            return int.Parse(mecanicoId);
        }

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mecanicoId = ObterMecanicoId();

            var clientes = await _context.cliente
                .Where(c => c.MecanicoId == mecanicoId)
                .Select(c => new ClienteListViewModel
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    Cpf = c.Cpf
                })
                .ToListAsync();

            return View(clientes);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhe(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var cliente = await _context.cliente
                .FirstOrDefaultAsync(c => c.Id == id && c.MecanicoId == mecanicoId);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }


        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(ClienteCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mecanicoId = ObterMecanicoId();

            var cliente = new Cliente
            {
                Nome = model.Nome,
                Telefone = model.Telefone,
                Cpf = model.Cpf,
                Email = model.Email,
                MecanicoId = mecanicoId
            };

            try
            {
                _context.cliente.Add(cliente);
                _context.SaveChanges();

                TempData["Sucesso"] = "Cliente cadastrado com sucesso!";
                return RedirectToAction("Index", "Cliente");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Não foi possível salvar o cliente: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var cliente = _context.cliente
                .FirstOrDefault(c => c.Id == id && c.MecanicoId == mecanicoId);

            if (cliente == null)
                return NotFound();

            var model = new ClienteEditViewModel
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Cpf = cliente.Cpf,
                Email = cliente.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, ClienteEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var mecanicoId = ObterMecanicoId();

            var cliente = _context.cliente
                .FirstOrDefault(c => c.Id == id && c.MecanicoId == mecanicoId);

            if (cliente == null)
                return NotFound();

            cliente.Nome = model.Nome;
            cliente.Telefone = model.Telefone;
            cliente.Cpf = model.Cpf;
            cliente.Email = model.Email;

            try
            {
                _context.SaveChanges();

                TempData["Sucesso"] = "Cliente atualizado com sucesso.";
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Não foi possível atualizar o cliente.");
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletarConfirmado(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var cliente = await _context.cliente
                .FirstOrDefaultAsync(c => c.Id == id && c.MecanicoId == mecanicoId);

            if (cliente == null)
                return NotFound();

            try
            {
                _context.cliente.Remove(cliente);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Cliente excluído com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Erro"] = "Erro ao tentar excluir o cliente.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}

