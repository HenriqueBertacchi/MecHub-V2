using Microsoft.AspNetCore.Mvc;
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;

namespace MecHub.Controllers
{
    public class ItemOrdemServicoController : Controller
    {
        private readonly AppDbContext _context;

        public ItemOrdemServicoController(AppDbContext context)
        {
            _context = context;
        }

        // Listar todos os itens
        [HttpGet]
        public IActionResult Index()
        {
            var itens = _context.item_ordem_servico.ToList();
            return Json(itens);
        }

        // Buscar item por ID
        [HttpGet]
        public IActionResult Detalhes(int id)
        {
            var item = _context.item_ordem_servico.Find(id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        // Criar item (ligando a uma OS já existente)
        [HttpPost]
        public IActionResult Criar(ItemOrdemServicoCreateViewModel model)
        {
            var item = new ItemOrdemServico
            {
                OrdemServicoId = model.OrdemServicoId, // precisa existir
                ServicoId = model.ServicoId,      // precisa existir
                Quantidade = model.Quantidade
            };


            try
            {
                _context.item_ordem_servico.Add(item);
                _context.SaveChanges();

                return View(item);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao criar item.");
                return View(model);
            }

        }

        // Deletar item
        [HttpGet]
        public IActionResult Deletar(int id)
        {
            var item = _context.item_ordem_servico.Find(id);

            if (item == null)
                return NotFound();

            _context.item_ordem_servico.Remove(item);
            _context.SaveChanges();

            return View();
        }

        [HttpPost]
        public IActionResult DeletarConfirmado(int id)
        {
            var item = _context.item_ordem_servico.Find(id);

            try
            {
                _context.item_ordem_servico.Remove(item);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao deletar item.");
                return View(item);
            }
        }


        // Editar item
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var item = _context.item_ordem_servico.Find(id);

            if (item == null)
                return NotFound();

            var model = new ItemOrdemServicoEditViewModel
            {
                ServicoId = item.ServicoId,
                Quantidade = item.Quantidade
            };
            return View();
        }

        [HttpPost]
        public IActionResult Editar(int id, ItemOrdemServicoEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var item = _context.item_ordem_servico.Find(id);

            if (item == null)
                return NotFound();

            item.ServicoId = model.ServicoId;
            item.Quantidade = model.Quantidade;
            // usuario.Senha = model.Senha;

            try
            {
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao atualizar item ordem.");
                return View(model);
            }
        }

        
    }
}