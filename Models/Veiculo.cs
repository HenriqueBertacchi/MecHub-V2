using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.Models
{
    [Table("veiculo")]
    public class Veiculo
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("marca")]
        public required string Marca { get; set; }

        [Column("modelo")]
        public required string Modelo { get; set; }

        [Column("placa")]
        public required string Placa { get; set; }

        [Column("cor")]
        public required string Cor { get; set; }

        [Column("ano_fabricacao")]
        public required int AnoFabricacao { get; set; }

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; }

        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [Column("status_veiculo")]
        public StatusVeiculoEnum StatusAtual {get; set; }

        [Column("observacao_status")]
        public string? ObservacaoStatus {get; set; }

        [Column("data_atualizacao_status")]
        public DateTime DataAtualizacaoStatus { get; set; } = DateTime.Now;
      
        [Column("mecanico_id")]
        public int MecanicoId {get; set; }

        public Mecanico Mecanico {get; set; }

        // Relacionamentos
        public Cliente? Cliente { get; set; }
        public List<OrdemServico> OrdensServico { get; set; } = new();
    }

        public enum StatusVeiculoEnum
    {
        PreAvaliacao = 1,
        AguardandoAprovacaoOS = 2,
        EmReparo = 3,
        ProntoParaRetirada = 4
    }
}