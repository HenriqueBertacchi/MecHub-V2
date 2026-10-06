using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.Models
{
    [Table("ordem_servico")]
    public class OrdemServico
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("mecanico_id")]
        public int MecanicoId { get; set; }

        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [Column("veiculo_id")]
        public int VeiculoId { get; set; }  

        [Column("status_ordem")]
        public StatusOrdemEnum StatusOrdem{ get; set; }

        // [Column("status_descricao")]
        // public required string statusDescricao {get; set; }

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; }

        [Column("data_fechamento")]
        public DateTime? DataFechamento { get; set; }

        // Relacionamentos
        public Mecanico? Mecanico { get; set; }
        public Cliente? Cliente { get; set; }
        public Veiculo? Veiculo { get; set; }

        public List<ItemOrdemServico> Itens { get; set; } = new();
    }

    public enum StatusOrdemEnum
    {
        Aberto = 1,
        Em_andamento = 2,
        AguardandoAprovacao = 3,
        Fechada = 4
    }
}