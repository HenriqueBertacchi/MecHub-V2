using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class ExternalLoginViewModel
    {
        [EmailAddress]
        public required string Email { get; set; }

        public required string Nome { get; set; }

        public required string IdGoogle { get; set; } // ID único do Google
    }
}
