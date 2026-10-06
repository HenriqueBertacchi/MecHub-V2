using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MecHub.Models
{
    [Table("usuario")]
    public class Usuario
    {
        public int Id { get; set; }

        [StringLength(100)]
        public required string Nome { get; set; }

        [StringLength(355)]
        public string Senha { get; set; } = string.Empty;

        [Column("tipo_login")]
        public TipoLoginEnum TipoLogin { get; set; }

        [Column("id_google")]
        public string? IdGoogle { get; set; }

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public required string Email { get; set; }

        [Column("reset_senha_token")]
        public string? ResetSenhaToken { get; set; }

        [Column("reset_senha_expira_em")]
        public DateTime? ResetSenhaExpiraEm { get; set; }

        // Relacionamento
        public Mecanico? Mecanico { get; set; }
    }
    public enum TipoLoginEnum
    {
        Local = 1,
        Google = 2
    }
}