using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel;

public class ResetarSenhaViewModel
{
    public string Email { get; set; }

    public string Token { get; set; }

    [Required(ErrorMessage = "Informe a nova senha.")]
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
    [RegularExpression(@"^(?=.*[!@#$%^&*(),.?""{}|<>_\-+=/\\[\]:;']).+$",
    ErrorMessage = "A senha deve conter pelo menos um caractere especial.")]
    [DataType(DataType.Password)]
    public string NovaSenha { get; set; }

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    [Compare("NovaSenha", ErrorMessage = "As senhas não conferem.")]
    public string ConfirmarSenha { get; set; }
}