using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class OrdemServicoEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "CampoObrigatorio",
            ErrorMessageResourceType = typeof(SharedResource))]
        public int ClienteId { get; set; }

        [Required(ErrorMessageResourceName = "CampoObrigatorio",
            ErrorMessageResourceType = typeof(SharedResource))]
        public int VeiculoId { get; set; }

        public required string Observacoes { get; set; }

        public required List<ItemOrdemServicoViewModel> Itens { get; set; }
    }
}
