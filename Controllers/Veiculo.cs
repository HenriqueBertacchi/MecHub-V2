using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MecHub.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class VeiculosController : Controller
    {
        private readonly AppDbContext _context;

        private async Task CarregarCombosVeiculoEdit(VeiculoEditViewModel model)
        {
            var mecanicoId = ObterMecanicoId();

            model.Clientes = await _context.cliente
                .Where(c => c.MecanicoId == mecanicoId)
                .OrderBy(c => c.Nome)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Nome} - {c.Cpf}"
                })
                .ToListAsync();

            model.StatusOptions = Enum.GetValues(typeof(StatusVeiculoEnum))
                .Cast<StatusVeiculoEnum>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString()
                })
                .ToList();
        }

        private async Task CarregarCombosVeiculo(VeiculoCreateViewModel model)
        {
            var mecanicoId = ObterMecanicoId();

            model.Clientes = await _context.cliente
                .Where(c => c.MecanicoId == mecanicoId)
                .OrderBy(c => c.Nome)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Nome} - {c.Cpf}"
                })
                .ToListAsync();

            model.StatusOptions = Enum.GetValues(typeof(StatusVeiculoEnum))
                .Cast<StatusVeiculoEnum>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString()
                })
                .ToList();
        }

        private int ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                throw new UnauthorizedAccessException("MecanicoId não encontrado na sessão.");

            return int.Parse(mecanicoId);
        }

        public VeiculosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mecanicoId = ObterMecanicoId();

            var veiculos = await _context.veiculo
                .Include(v => v.Cliente)
                .Where(v => v.MecanicoId == mecanicoId)
                .OrderBy(v => v.Placa)
                .Select(v => new VeiculoListViewModel
                {
                    Id = v.Id,
                    Placa = v.Placa,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Cor = v.Cor,
                    AnoFabricacao = v.AnoFabricacao,
                    DataCriacao = v.DataCriacao,
                    StatusAtual = v.StatusAtual,
                    ClienteNome = v.Cliente != null ? v.Cliente.Nome : "Sem cliente"
                })
                .ToListAsync();

            return View(veiculos);
        }

        [HttpGet]
        public async Task<IActionResult> Criar()
        {
            var model = new VeiculoCreateViewModel
            {
                StatusAtual = StatusVeiculoEnum.PreAvaliacao
            };

            await CarregarCombosVeiculo(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(VeiculoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CarregarCombosVeiculo(model);
                return View(model);
            }

            var mecanicoId = ObterMecanicoId();

            var placaNormalizada = model.Placa.Trim().ToUpper();

            var placaExiste = await _context.veiculo
                .AnyAsync(v => v.Placa == placaNormalizada);

            if (placaExiste)
            {
                ModelState.AddModelError("Placa", "Já existe um veículo cadastrado com esta placa.");
                await CarregarCombosVeiculo(model);
                return View(model);
            }

            var clienteExiste = await _context.cliente
                .AnyAsync(c => c.Id == model.ClienteId && c.MecanicoId == mecanicoId);

            if (!clienteExiste)
            {
                ModelState.AddModelError("ClienteId", "Cliente informado não existe.");
                await CarregarCombosVeiculo(model);
                return View(model);
            }

            var veiculo = new Veiculo
            {
                Placa = placaNormalizada,
                Modelo = model.Modelo,
                Marca = model.Marca,
                ClienteId = model.ClienteId,
                Cor = model.Cor,
                AnoFabricacao = model.AnoFabricacao,
                StatusAtual = model.StatusAtual,
                ObservacaoStatus = model.ObservacaoStatus,
                DataCriacao = DateTime.Now,
                DataAtualizacaoStatus = DateTime.Now,
                MecanicoId = mecanicoId
            };

            _context.veiculo.Add(veiculo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var veiculo = await _context.veiculo
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound();

            return View(veiculo);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var veiculo = await _context.veiculo
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound();

            var model = new VeiculoEditViewModel
            {
                Id = veiculo.Id,
                Placa = veiculo.Placa,
                Cor = veiculo.Cor,
                AnoFabricacao = veiculo.AnoFabricacao,
                Modelo = veiculo.Modelo,
                Marca = veiculo.Marca,
                ClienteId = veiculo.ClienteId,
                StatusAtual = veiculo.StatusAtual,
                ObservacaoStatus = veiculo.ObservacaoStatus
            };

            await CarregarCombosVeiculoEdit(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, VeiculoEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await CarregarCombosVeiculoEdit(model);
                return View(model);
            }

            var mecanicoId = ObterMecanicoId();

            var veiculo = await _context.veiculo
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound();

            var placaNormalizada = model.Placa.Trim().ToUpper();

            var placaExiste = await _context.veiculo
                .AnyAsync(v => v.Placa == placaNormalizada && v.Id != id);

            if (placaExiste)
            {
                ModelState.AddModelError("Placa", "Já existe outro veículo cadastrado com esta placa.");
                await CarregarCombosVeiculoEdit(model);
                return View(model);
            }

            var clienteExiste = await _context.cliente
                .AnyAsync(c => c.Id == model.ClienteId && c.MecanicoId == mecanicoId);

            if (!clienteExiste)
            {
                ModelState.AddModelError("ClienteId", "Cliente informado não existe.");
                await CarregarCombosVeiculoEdit(model);
                return View(model);
            }

            veiculo.Placa = placaNormalizada;
            veiculo.Cor = model.Cor;
            veiculo.AnoFabricacao = model.AnoFabricacao;
            veiculo.Modelo = model.Modelo;
            veiculo.Marca = model.Marca;
            veiculo.ClienteId = model.ClienteId;
            veiculo.StatusAtual = model.StatusAtual;
            veiculo.ObservacaoStatus = model.ObservacaoStatus;
            veiculo.DataAtualizacaoStatus = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var veiculo = await _context.veiculo
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound();

            try
            {
                _context.veiculo.Remove(veiculo);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Veículo excluído com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Erro"] = "Erro ao tentar excluir o veículo.";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet]
        public async Task<IActionResult> AtualizarStatus(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var veiculo = await _context.veiculo
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound();

            var model = new VeiculoAtualizarStatusViewModel
            {
                Id = veiculo.Id,
                Placa = veiculo.Placa,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                StatusAtual = veiculo.StatusAtual,
                ObservacaoStatus = veiculo.ObservacaoStatus
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatus(VeiculoAtualizarStatusViewModel model, int id)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mecanicoId = ObterMecanicoId();

            var veiculo = await _context.veiculo
                .FirstOrDefaultAsync(v => v.Id == id && v.MecanicoId == mecanicoId);

            if (veiculo == null)
                return NotFound();

            veiculo.StatusAtual = model.StatusAtual;
            veiculo.ObservacaoStatus = model.ObservacaoStatus;
            veiculo.DataAtualizacaoStatus = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Status do veículo atualizado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
    }
}