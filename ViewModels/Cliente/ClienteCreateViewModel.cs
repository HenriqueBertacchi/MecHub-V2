using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
/// ViewModel responsável por intermediar a comunicação entre a View (front-end)
/// e o Controller (back-end), transportando apenas os dados necessários para a interface,
/// sem expor diretamente as entidades do banco de dados.
    public class ClienteCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(250)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o telefone.")]
        public string Telefone { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Informe o CPF.")]
        [MinLength(11, ErrorMessage = "O CPF deve conter 11 números.")]
        [MaxLength(11, ErrorMessage = "O CPF deve conter 11 números.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o email.")]
        [StringLength(250)]
        [EmailAddress(ErrorMessage = "Informe um email válido.")]
        public string Email { get; set; } = string.Empty;
    }
}