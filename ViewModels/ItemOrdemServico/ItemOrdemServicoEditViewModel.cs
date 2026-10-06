using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class ItemOrdemServicoEditViewModel
    {

        public int Id {get; set; }

        [Required(ErrorMessageResourceName = "CampoObrigatorio",
            ErrorMessageResourceType = typeof(SharedResource))]
        public int ServicoId { get; set; }

        [Required(ErrorMessageResourceName = "CampoObrigatorio",
            ErrorMessageResourceType = typeof(SharedResource))]
        public int Quantidade { get; set; }

        public List<ItemOrdemServicoViewModel> Itens { get; set; }
    }
}
