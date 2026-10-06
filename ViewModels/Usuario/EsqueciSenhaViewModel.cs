using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel;

public class EsqueciSenhaViewModel
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; }
}






// using System.ComponentModel.DataAnnotations;

// namespace MecHub.ViewModel
// {
//     public class UsuarioTrocarSenhaViewModel
//     {
//         public int Id { get; set; }

//         [Required(ErrorMessage = "Informe a nova senha.")]
        
//         public required string NovaSenha { get; set; }

//         [Required(ErrorMessage = "Confirme sua nova senha.")]
//         public required string ConfirmarSenha { get; set; }

//         // [Required(ErrorMessageResourceName = "CampoObrigatorio",
//         //     ErrorMessageResourceType = typeof(SharedResource))]
//         // public required string Senha { get; set; }
//     }
// }
