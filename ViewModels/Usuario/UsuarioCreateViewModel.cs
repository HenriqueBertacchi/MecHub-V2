using System.ComponentModel.DataAnnotations;
using MecHub.Models;

namespace MecHub.ViewModel
{
    public class UsuarioCreateViewModel
    {
        [Required(ErrorMessage = "Informe seu nome.")]
        [StringLength(250)]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "Informe seu email.")]
        [StringLength(250)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Informe a senha.")]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        [RegularExpression(@"^(?=.*[!@#$%^&*(),.?""{}|<>_\-+=/\\[\]:;']).+$",
        ErrorMessage = "A senha deve conter pelo menos um caractere especial.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Confirme sua senha.")]
        [MinLength(10)]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
        public required string ConfirmarSenha { get; set; }

        [Required(ErrorMessage = "Informe o telefone")]
        [Phone(ErrorMessage = "Informe um telefone válido")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }
    }
}

