using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class ItemOrdemServicoCreateViewModel
    {
        [Required]
        public int OrdemServicoId { get; set; }

        [Required]
        public int ServicoId { get; set; }

        [Required]
         [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; } = 1;
    }
}