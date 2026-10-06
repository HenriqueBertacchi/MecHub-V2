using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using MecHub.Models;

namespace MecHub.ViewModel
{
    public class VeiculoAtualizarStatusViewModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("placa")]
        public string? Placa { get; set; }

        [Column("modelo")]
        public string? Modelo { get; set; }

        [Column("marca")]
        public string? Marca { get; set; }

        [Required(ErrorMessage = "Selecione um status")]
        [Display(Name = "Status atual")]
        [Column("status_atual")]
        public StatusVeiculoEnum StatusAtual { get; set; }

        [Column("observacao_status")]
        [Display(Name = "Observação")]
        [StringLength(300, ErrorMessage = "A observação deve ter no máximo 300 caracteres")]
        public string? ObservacaoStatus { get; set; }
    }
}