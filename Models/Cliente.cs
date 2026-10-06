using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("nome")]
        public required string Nome { get; set; }

        [Column("telefone")]
        public required string Telefone { get; set; }

        [Column("cpf")]
        public required string Cpf { get; set; }

        [Column("email")]
        public required string Email {get; set; } = string.Empty;

        [Column("mecanico_id")]
        public int MecanicoId {get; set; }

        public Mecanico Mecanico {get; set; }
        public List<Veiculo> Veiculos { get; set; } = new();
    }
}