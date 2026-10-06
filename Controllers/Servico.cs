using Microsoft.AspNetCore.Mvc;
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MecHub.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ServicoController : Controller
    {
        private readonly AppDbContext _context;

        public ServicoController(AppDbContext context)
        {
            _context = context;
        }

        private int ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                throw new UnauthorizedAccessException("MecanicoId não encontrado na sessão.");

            return int.Parse(mecanicoId);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mecanicoId = ObterMecanicoId();

            var servicos = await _context.servico
                .Where(s => s.MecanicoId == mecanicoId)
                .Select(s => new ServicoListViewModel
                {
                    Id = s.Id,
                    Descricao = s.Descricao,
                    Valor = s.Valor,
                    Tipo = s.Tipo
                })
                .ToListAsync();

            return View(servicos);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhe(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var servico = await _context.servico
                .FirstOrDefaultAsync(s => s.Id == id && s.MecanicoId == mecanicoId);

            if (servico == null)
                return NotFound();

            return View(servico);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View(new ServicoCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(ServicoCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mecanicoId = ObterMecanicoId();

            var servico = new Servico
            {
                Descricao = model.Descricao,
                Valor = model.Valor,
                Tipo = model.Tipo,
                MecanicoId = mecanicoId
            };

            try
            {
                _context.servico.Add(servico);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Serviço cadastrado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Não foi possível salvar o serviço. Tente novamente.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var servico = await _context.servico
                .FirstOrDefaultAsync(s => s.Id == id && s.MecanicoId == mecanicoId);

            if (servico == null)
                return NotFound();

            var model = new ServicoEditViewModel
            {
                Id = servico.Id,
                Descricao = servico.Descricao,
                Valor = servico.Valor,
                Tipo = servico.Tipo
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ServicoEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var mecanicoId = ObterMecanicoId();

            var servico = await _context.servico
                .FirstOrDefaultAsync(s => s.Id == id && s.MecanicoId == mecanicoId);

            if (servico == null)
                return NotFound();

            servico.Descricao = model.Descricao;
            servico.Valor = model.Valor;
            servico.Tipo = model.Tipo;

            try
            {
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Serviço atualizado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Erro ao tentar editar dados do serviço.");
                return View(model);
            }
        }

        // [HttpGet]
        // public async Task<IActionResult> Deletar(int id)
        // {
        //     var mecanicoId = ObterMecanicoId();

        //     var servico = await _context.servico
        //         .FirstOrDefaultAsync(s => s.Id == id && s.MecanicoId == mecanicoId);

        //     if (servico == null)
        //         return NotFound();

        //     return View(servico);
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletarConfirmado(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var servico = await _context.servico
                .FirstOrDefaultAsync(s => s.Id == id && s.MecanicoId == mecanicoId);

            if (servico == null)
                return NotFound();

            try
            {
                _context.servico.Remove(servico);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Serviço excluído com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Erro"] = "Erro ao tentar deletar o serviço.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}