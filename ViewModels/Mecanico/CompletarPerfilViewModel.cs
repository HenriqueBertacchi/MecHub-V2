using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class MecanicoCompletarPerfilViewModel
    {
        [Required(ErrorMessage = "Informe seu telefone.")]
        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;
    }
}