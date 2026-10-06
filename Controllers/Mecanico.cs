using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
// Recursos de autenticação
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace MecHub.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class MecanicoController : Controller
    {
        private readonly AppDbContext _context;


        private int? ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                return null;

            return int.Parse(mecanicoId);
        }
        public MecanicoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CompletarPerfil()
        {
            var mecanicoId = int.Parse(User.FindFirstValue("MecanicoId")!);

            var mecanico = await _context.mecanico
                .FirstOrDefaultAsync(m => m.Id == mecanicoId);

            if (mecanico == null)
                return NotFound();

            var model = new MecanicoCompletarPerfilViewModel
            {
                Telefone = mecanico.Telefone ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarPerfil(MecanicoCompletarPerfilViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mecanicoId = int.Parse(User.FindFirstValue("MecanicoId")!);

            var mecanico = await _context.mecanico
                .FirstOrDefaultAsync(m => m.Id == mecanicoId);

            if (mecanico == null)
                return NotFound();

            mecanico.Telefone = model.Telefone;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Mecanico");
        }


        // Dashboard do mecânico
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var mecanicoId = ObterMecanicoId();

            if (mecanicoId == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("LoginLocal", "Auth");
            }

            ViewBag.TotalClientes = await _context.cliente
                .CountAsync(c => c.MecanicoId == mecanicoId);

            ViewBag.TotalVeiculos = await _context.veiculo
                .CountAsync(v => v.MecanicoId == mecanicoId);

            ViewBag.TotalServicos = await _context.servico
                .CountAsync(s => s.MecanicoId == mecanicoId);

            ViewBag.TotalOrdens = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId);

            ViewBag.OrdensAbertas = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.Aberto);

            ViewBag.OrdensAndamento = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.Em_andamento);

            ViewBag.OrdensAguardando = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.AguardandoAprovacao);

            ViewBag.OrdensFechadas = await _context.ordem_servico
                .CountAsync(o => o.MecanicoId == mecanicoId && o.StatusOrdem == StatusOrdemEnum.Fechada);

            ViewBag.UltimasOrdens = await _context.ordem_servico
                .Include(o => o.Cliente)
                .Include(o => o.Veiculo)
                .Where(o => o.MecanicoId == mecanicoId)
                .OrderByDescending(o => o.DataCriacao)
                .Take(5)
                .ToListAsync();

            return View();
        }
        // Lista de clientes para o mecânico
        [HttpGet]
        public async Task<IActionResult> ListaClientes()
        {
            var mecanicoId = ObterMecanicoId();

            var clientes = await _context.cliente
                .Where(c => c.MecanicoId == mecanicoId)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return View(clientes);
        }

        // Detalhes do mecânico
        [HttpGet]
        public async Task<IActionResult> Detalhe(int id)
        {
            var mecanico = await _context.mecanico
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mecanico == null)
                return NotFound();

            return View(mecanico);
        }

        // Tela de criação de mecânico
        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        // Processar criação de mecânico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(MecanicoCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mecanico = new Mecanico
            {
                UsuarioId = model.UsuarioId,
                Telefone = model.Telefone
            };

            _context.mecanico.Add(mecanico);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Tela de edição de mecânico
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var mecanico = await _context.mecanico.FindAsync(id);

            if (mecanico == null)
                return NotFound();

            var model = new MecanicoEditViewModel
            {
                Id = mecanico.Id,
                UsuarioId = mecanico.UsuarioId,
                Telefone = mecanico.Telefone
            };

            return View(model);
        }

        // Processar edição de mecânico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(MecanicoEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mecanico = await _context.mecanico.FindAsync(model.Id);

            if (mecanico == null)
                return NotFound();

            mecanico.UsuarioId = model.UsuarioId;
            mecanico.Telefone = model.Telefone;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Tela de confirmação de exclusão
        [HttpGet]
        public async Task<IActionResult> Excluir(int id)
        {
            var mecanico = await _context.mecanico
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mecanico == null)
                return NotFound();

            return View(mecanico);
        }

        // Confirmar exclusão
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            var mecanico = await _context.mecanico.FindAsync(id);

            if (mecanico == null)
                return NotFound();

            _context.mecanico.Remove(mecanico);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}