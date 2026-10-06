using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class MecanicoCreateViewModel
    {
        [Required(ErrorMessage = "Selecione um usuário")]
        [Display(Name = "Usuário")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Informe o telefone")]
        [Phone(ErrorMessage = "Informe um telefone válido")]
        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }
    }
}