using System.ComponentModel.DataAnnotations;


namespace MecHub.ViewModel
{
    public class SharedResource
    {
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        public required string Senha { get; set; }
    
        public bool LembrarMe { get; set; }
    }
}
