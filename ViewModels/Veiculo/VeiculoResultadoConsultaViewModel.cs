using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.ViewModel
{
    public class VeiculoResultadoConsultaViewModel
    {
        
        public string Placa { get; set; }
        
        public string Marca { get; set; }
        
        public string Modelo { get; set; }
        
        public string Cor { get; set; }

        public string NomeCliente { get; set; }

        public string StatusAtual { get; set; }
        public int EtapaAtual { get; set; }

        public string? ObservacaoStatus { get; set; }
        public DateTime DataAtualizacaoStatus { get; set; }

        public string NomeMecanico { get; set; }
        public string? TelefoneMecanico { get; set; }

        public bool PossuiOrdemServico { get; set; }
        public int? OrdemServicoId { get; set; }

        public List<string> Etapas { get; set; } = new()
        {
            "Pré-avaliação",
            "Aguardando aprovação da OS",
            "Em Reparo",
            "Pronto para retirada"
        };
    }
}