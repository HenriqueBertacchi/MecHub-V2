using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecHub.Data;
using MecHub.Models;
using MecHub.ViewModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;



namespace MecHub.Controllers
{
    // [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ConsultaVeiculo : Controller
    {
        private readonly AppDbContext _context;

        private int ObterMecanicoId()
        {
            var mecanicoId = User.FindFirstValue("MecanicoId");

            if (string.IsNullOrWhiteSpace(mecanicoId))
                throw new UnauthorizedAccessException("MecanicoId não encontrado na sessão.");

            return int.Parse(mecanicoId);
        }

        public ConsultaVeiculo(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new VeiculoConsultaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(VeiculoConsultaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var placa = model.Placa.Trim().ToUpper();

            return RedirectToAction("Resultado", new { placa });
        }

        // [Authorize]
        [HttpGet]
        public IActionResult Resultado(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                return RedirectToAction("Index");

            placa = placa.Trim().ToUpper();

            var veiculo = _context.veiculo
                .Include(v => v.Cliente)
                .FirstOrDefault(v => v.Placa.ToUpper() == placa);

            if (veiculo == null)
            {
                TempData["Erro"] = "Nenhum veículo encontrado com essa placa.";
                return RedirectToAction("Index");
            }

            var ordemServico = _context.ordem_servico
                .Include(o => o.Mecanico)
                    .ThenInclude(m => m.Usuario)
                .Where(o => o.VeiculoId == veiculo.Id)
                .OrderByDescending(o => o.DataCriacao)
                .FirstOrDefault();

            var etapaAtual = ObterEtapaAtual(veiculo.StatusAtual);

            var model = new VeiculoResultadoConsultaViewModel
            {
                Placa = veiculo.Placa,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Cor = veiculo.Cor,

                NomeCliente = veiculo.Cliente?.Nome ?? "Não informado",

                StatusAtual = ObterDescricaoStatus(veiculo.StatusAtual),
                EtapaAtual = etapaAtual,

                ObservacaoStatus = veiculo.ObservacaoStatus,
                DataAtualizacaoStatus = veiculo.DataAtualizacaoStatus,

                NomeMecanico = ordemServico?.Mecanico?.Usuario?.Nome ?? "Ainda não definido",
                TelefoneMecanico = ordemServico?.Mecanico?.Telefone,

                PossuiOrdemServico = ordemServico != null,
                OrdemServicoId = ordemServico?.Id
            };

            return View(model);
        }

        private int ObterEtapaAtual(StatusVeiculoEnum status)
        {
            return status switch
            {
                StatusVeiculoEnum.PreAvaliacao => 1,
                StatusVeiculoEnum.AguardandoAprovacaoOS => 2,
                StatusVeiculoEnum.EmReparo => 3,
                StatusVeiculoEnum.ProntoParaRetirada => 4,
                _ => 1
            };
        }

        private string ObterDescricaoStatus(StatusVeiculoEnum status)
        {
            return status switch
            {
                StatusVeiculoEnum.PreAvaliacao => "Pré-avaliação",
                StatusVeiculoEnum.AguardandoAprovacaoOS => "Aguardando aprovação da OS",
                StatusVeiculoEnum.EmReparo => "Em Reparo",
                StatusVeiculoEnum.ProntoParaRetirada => "Pronto para retirada",
                _ => "Status não informado"
            };
        }
    }
}