using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class UsuarioEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "CampoObrigatorio",
            ErrorMessageResourceType = typeof(SharedResource))]
        public required string Nome { get; set; }

        [Required(ErrorMessageResourceName = "CampoObrigatorio",
            ErrorMessageResourceType = typeof(SharedResource))]
        public required string Email { get; set; }

        // [Required(ErrorMessageResourceName = "CampoObrigatorio",
        //     ErrorMessageResourceType = typeof(SharedResource))]
        // public required string Senha { get; set; }
    }
}
