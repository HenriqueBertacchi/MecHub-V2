using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.ViewModel
{
    public class ClienteEditViewModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(250)]
        [Column("nome")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "Informe o telefone.")]
        [Column("telefone")]
        public required string Telefone { get; set; }

        [Required(ErrorMessage = "Informe o CPF.")]
        [MinLength(11, ErrorMessage = "O CPF deve conter 11 números.")]
        [MaxLength(11, ErrorMessage = "O CPF deve conter 11 números.")]
        [Column("cpf")]
        public required string Cpf { get; set; }

        [Required(ErrorMessage = "Informe o email.")]
        [StringLength(250)]
        [EmailAddress(ErrorMessage = "Informe um email válido.")]
        [Column("email")]
        public required string Email {get; set;}
    }
}
