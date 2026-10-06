using System.ComponentModel.DataAnnotations;
using MecHub.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MecHub.ViewModel
{
    public class VeiculoCreateViewModel
    {
        [Required(ErrorMessage = "Informe a placa.")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o modelo.")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a marca.")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um cliente.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Informe a cor.")]
        public string Cor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o ano de fabricação.")]
        public int AnoFabricacao { get; set; }

        [Required(ErrorMessage = "Informe o status atual.")]
        public StatusVeiculoEnum StatusAtual { get; set; } = StatusVeiculoEnum.PreAvaliacao;

        public string? ObservacaoStatus { get; set; }

        public List<SelectListItem> Clientes { get; set; } = new();

        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}