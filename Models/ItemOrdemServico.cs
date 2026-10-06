using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MecHub.Models
{
    [Table("item_ordem_servico")]
    public class ItemOrdemServico
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ordem_servico_id")]
        public int OrdemServicoId { get; set; }

        [Column("servico_id")]
        public int ServicoId { get; set; }

        [Column("quantidade")]
        public int Quantidade { get; set; }

        // Relacionamentos
        [JsonIgnore]
        public OrdemServico? OrdemServico { get; set; } 
        public Servico? Servico { get; set; }
        
    }
}