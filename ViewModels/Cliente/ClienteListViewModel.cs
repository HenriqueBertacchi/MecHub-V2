using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.ViewModel
{
    public class ClienteListViewModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("nome")]
        public required string Nome { get; set; }

        [Column("telefone")]
        public required string Telefone { get; set; }

        [Column("email")]
        public required string Email { get; set; }

        [Column("cpf")]
        public required string Cpf { get; set; }


    }
}
