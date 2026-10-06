using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;
using Microsoft.AspNetCore.Authorization;
using MecHub.Services;

namespace MecHub.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class OrdemServicoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly OrdemServicoPdfService _pdfService;

        private int ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                throw new UnauthorizedAccessException("MecanicoId não encontrado na sessão.");

            return int.Parse(mecanicoId);
        }

        public OrdemServicoController(AppDbContext context, OrdemServicoPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;

        }

        // FUNÇÃO/MÉTODO RESPONSÁVEL POR EXIBIR DE FORMA ORDENADA AS ORDENS DE SERVIÇOS EXISTENTES NO BANCO DE DADOS
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mecanicoId = ObterMecanicoId();

            var ordens = await _context.ordem_servico
                .Include(o => o.Mecanico).ThenInclude(m => m.Usuario)
                .Include(o => o.Cliente)
                .Include(o => o.Veiculo)
                .Include(o => o.Itens).ThenInclude(i => i.Servico)
                .Where(o => o.MecanicoId == mecanicoId)
                .OrderByDescending(o => o.DataCriacao)
                .Select(o => new OrdemServicoListViewModel
                {
                    Id = o.Id,

                    MecanicoNome = o.Mecanico != null && o.Mecanico.Usuario != null
                        ? o.Mecanico.Usuario.Nome
                        : "Não informado",

                    ClienteNome = o.Cliente != null ? o.Cliente.Nome : "Não informado",

                    VeiculoPlaca = o.Veiculo != null ? o.Veiculo.Placa : "Não informado",
                    VeiculoMarca = o.Veiculo != null ? o.Veiculo.Marca : "Não informado",
                    VeiculoModelo = o.Veiculo != null ? o.Veiculo.Modelo : "Não informado",

                    StatusTexto = o.StatusOrdem == StatusOrdemEnum.Aberto ? "Aberta"
                        : o.StatusOrdem == StatusOrdemEnum.Em_andamento ? "Em andamento"
                        : o.StatusOrdem == StatusOrdemEnum.AguardandoAprovacao ? "Aguardando aprovação"
                        : o.StatusOrdem == StatusOrdemEnum.Fechada ? "Fechada"
                        : "Não informado",

                    StatusClasse = o.StatusOrdem == StatusOrdemEnum.Aberto ? "aberta"
                        : o.StatusOrdem == StatusOrdemEnum.Em_andamento ? "emandamento"
                        : o.StatusOrdem == StatusOrdemEnum.AguardandoAprovacao ? "aguardandoaprovacao"
                        : o.StatusOrdem == StatusOrdemEnum.Fechada ? "fechada"
                        : "naoinformado",

                    DataCriacao = o.DataCriacao,
                    DataFechamento = o.DataFechamento,
                    QuantidadeItens = o.Itens.Count,

                    ValorTotal = o.Itens.Sum(i =>
                        i.Servico != null ? i.Servico.Valor * i.Quantidade : 0)
                })
                .ToListAsync();

            return View(ordens);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var ordem = await _context.ordem_servico
                .Include(o => o.Mecanico).ThenInclude(m => m.Usuario)
                .Include(o => o.Cliente)
                .Include(o => o.Veiculo)
                .Include(o => o.Itens).ThenInclude(i => i.Servico)
                .FirstOrDefaultAsync(o => o.Id == id && o.MecanicoId == mecanicoId);

            if (ordem == null)
                return NotFound();

            return View(ordem);
        }

        [HttpGet]
        public async Task<IActionResult> Criar()
        {
            var model = new OrdemServicoCreateViewModel
            {
                StatusOrdem = StatusOrdemEnum.Aberto,
                MecanicoId = await ObterMecanicoLogadoId()
            };

            await CarregarCombos(model);

            return View(model);
        }

        // FUNÇÃO/MÉTODO RESPONSÁVEL POR CRIAR NOVAS ORDENS DE SERVIÇOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(OrdemServicoCreateViewModel model)
        {

            var mecanicoId = ObterMecanicoId();

            if (!ModelState.IsValid)
            {
                await CarregarCombos(model);
                return View(model);
            }

            if (model.Itens == null || !model.Itens.Any())
            {
                ModelState.AddModelError("", "Adicione pelo menos um serviço à ordem.");
                await CarregarCombos(model);
                return View(model);
            }

            var clienteExiste = await _context.cliente
                 .AnyAsync(c => c.Id == model.ClienteId && c.MecanicoId == mecanicoId);
            var veiculoExiste = await _context.veiculo
                 .AnyAsync(v => v.Id == model.VeiculoId && v.MecanicoId == mecanicoId);
            var mecanicoExiste = await _context.mecanico.AnyAsync(m => m.Id == model.MecanicoId);

            if (!clienteExiste)
                ModelState.AddModelError("ClienteId", "Cliente inválido.");

            if (!veiculoExiste)
                ModelState.AddModelError("VeiculoId", "Veículo inválido.");

            if (!mecanicoExiste)
                ModelState.AddModelError("MecanicoId", "Mecânico inválido.");

            if (!ModelState.IsValid)
            {
                await CarregarCombos(model);
                return View(model);
            }

            var ordem = new OrdemServico
            {
                MecanicoId = model.MecanicoId,
                ClienteId = model.ClienteId,
                VeiculoId = model.VeiculoId,
                StatusOrdem = model.StatusOrdem,
                DataCriacao = DateTime.Now,

                Itens = model.Itens
                    .Where(i => i.ServicoId > 0 && i.Quantidade > 0)
                    .Select(i => new ItemOrdemServico
                    {
                        ServicoId = i.ServicoId,
                        Quantidade = i.Quantidade
                    })
                    .ToList()
            };

            _context.ordem_servico.Add(ordem);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Ordem de serviço criada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var ordem = await _context.ordem_servico
                .Include(o => o.Itens)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound();

            var model = new OrdemServicoCreateViewModel
            {
                MecanicoId = mecanicoId,
                ClienteId = ordem.ClienteId,
                VeiculoId = ordem.VeiculoId,
                StatusOrdem = ordem.StatusOrdem,
                Itens = ordem.Itens.Select(i => new ItemOrdemServicoCreateViewModel
                {
                    ServicoId = i.ServicoId,
                    Quantidade = i.Quantidade
                }).ToList()
            };

            await CarregarCombos(model);

            ViewBag.OrdemId = ordem.Id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, OrdemServicoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CarregarCombos(model);
                ViewBag.OrdemId = id;
                return View(model);
            }

            var mecanicoId = ObterMecanicoId();

            var ordem = await _context.ordem_servico
                .Include(o => o.Itens)
                .FirstOrDefaultAsync(o => o.Id == id && o.MecanicoId == mecanicoId);

            if (ordem == null)
                return NotFound();

            ordem.MecanicoId = model.MecanicoId;
            ordem.ClienteId = model.ClienteId;
            ordem.VeiculoId = model.VeiculoId;
            ordem.StatusOrdem = model.StatusOrdem;

            if (model.StatusOrdem == StatusOrdemEnum.Fechada && ordem.DataFechamento == null)
                ordem.DataFechamento = DateTime.Now;

            if (model.StatusOrdem != StatusOrdemEnum.Fechada)
                ordem.DataFechamento = null;

            _context.RemoveRange(ordem.Itens);

            ordem.Itens = model.Itens
                .Where(i => i.ServicoId > 0 && i.Quantidade > 0)
                .Select(i => new ItemOrdemServico
                {
                    OrdemServicoId = ordem.Id,
                    ServicoId = i.ServicoId,
                    Quantidade = i.Quantidade
                })
                .ToList();

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Ordem de serviço atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConfirmado(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var ordem = await _context.ordem_servico
                .Include(o => o.Itens)
                .FirstOrDefaultAsync(o => o.Id == id && o.MecanicoId == mecanicoId);

            if (ordem == null)
                return NotFound();

            try
            {
                _context.item_ordem_servico.RemoveRange(ordem.Itens);
                _context.ordem_servico.Remove(ordem);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Ordem de serviço excluída com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Erro"] = "Erro ao tentar excluir a ordem de serviço.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarVeiculoPorPlaca(string placa)
        {

            var mecanicoId = ObterMecanicoId();

            if (string.IsNullOrWhiteSpace(placa))
                return BadRequest();

            var veiculo = await _context.veiculo
                .Include(v => v.Cliente)
                .Where(v => v.Placa == placa && v.MecanicoId == mecanicoId)
                .Select(v => new
                {
                    id = v.Id,
                    placa = v.Placa,
                    marca = v.Marca,
                    modelo = v.Modelo,
                    cor = v.Cor,
                    anoFabricacao = v.AnoFabricacao,
                    clienteId = v.ClienteId,
                    clienteNome = v.Cliente != null ? v.Cliente.Nome : ""
                })
                .FirstOrDefaultAsync();

            if (veiculo == null)
                return NotFound();

            return Json(veiculo);
        }

        private async Task CarregarCombos(OrdemServicoCreateViewModel model)
        {
            var mecanicoId = ObterMecanicoId();


            model.Mecanicos = await _context.mecanico
                .Include(m => m.Usuario)
                .OrderBy(m => m.Usuario!.Nome)
                .Where(m => m.Id == mecanicoId)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Usuario != null ? m.Usuario.Nome : $"Mecânico #{m.Id}"
                })
                .ToListAsync();

            model.Clientes = await _context.cliente
                .OrderBy(c => c.Nome)
                .Where(c => c.MecanicoId == mecanicoId)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nome
                })
                .ToListAsync();

            model.Veiculos = await _context.veiculo
                .Include(v => v.Cliente)
                .OrderBy(v => v.Placa)
                .Where(c => c.MecanicoId == mecanicoId)
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.Placa + " - " + v.Marca + " " + v.Modelo
                })
                .ToListAsync();

            model.Servicos = await _context.servico
                .OrderBy(s => s.Descricao)
                .Where(c => c.MecanicoId == mecanicoId)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Descricao + " - " + s.Valor.ToString("C")
                })
                .ToListAsync();

            model.StatusOptions = Enum.GetValues(typeof(StatusOrdemEnum))
                .Cast<StatusOrdemEnum>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s == StatusOrdemEnum.Aberto ? "Aberta"
                        : s == StatusOrdemEnum.Em_andamento ? "Em andamento"
                        : s == StatusOrdemEnum.AguardandoAprovacao ? "Aguardando aprovação"
                        : s == StatusOrdemEnum.Fechada ? "Fechada"
                        : s.ToString()
                })
                .ToList();
        }

        private async Task<int> ObterMecanicoLogadoId()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
                return 0;

            var mecanico = await _context.mecanico
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Usuario != null && m.Usuario.Email == email);

            return mecanico?.Id ?? 0;
        }

        [HttpGet]
        public async Task<IActionResult> GerarPdf(int id)
        {
            var mecanicoId = ObterMecanicoId();

            var ordem = await _context.ordem_servico
                .Include(o => o.Mecanico)
                    .ThenInclude(m => m.Usuario)
                .Include(o => o.Cliente)
                .Include(o => o.Veiculo)
                .Include(o => o.Itens)
                    .ThenInclude(i => i.Servico)
                .FirstOrDefaultAsync(o => o.Id == id && o.MecanicoId == mecanicoId);

            if (ordem == null)
                return NotFound();

            var pdfBytes = _pdfService.GerarPdf(ordem);

            var fileName = $"OS-{ordem.Id}-{ordem.Veiculo.Placa}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var ordem = await _context.ordem_servico
                .Include(o => o.Mecanico)
                    .ThenInclude(m => m.Usuario)
                .Include(o => o.Cliente)
                .Include(o => o.Veiculo)
                .Include(o => o.Itens)
                    .ThenInclude(i => i.Servico)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound();

            var pdfBytes = _pdfService.GerarPdf(ordem);

            var fileName = $"OS-{ordem.Id}-{ordem.Veiculo.Placa}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}