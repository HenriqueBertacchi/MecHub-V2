using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.Models
{
    [Table("mecanico")]
    public class Mecanico
    {
        [Column("id")] // nome real no banco
        public int Id { get; set; }
        
        
        [Column("usuario_id")] // nome real no banco
        public int UsuarioId { get; set; }

        [Column("telefone")] // nome real no banco
        public string? Telefone { get; set; }

        // Relacionamentos
        public Usuario? Usuario { get; set; }
        public List<OrdemServico> OrdensServico { get; set; } = new();
    }
}