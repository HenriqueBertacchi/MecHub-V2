using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MecHub.Models;

namespace MecHub.ViewModel
{
    public class OrdemServicoCreateViewModel
    {
        [Required]
        public int MecanicoId { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        [Required]
        public StatusOrdemEnum StatusOrdem { get; set; } = StatusOrdemEnum.Aberto;

        public List<ItemOrdemServicoCreateViewModel> Itens { get; set; } = new();

        public List<SelectListItem> Mecanicos { get; set; } = new();
        public List<SelectListItem> Clientes { get; set; } = new();
        public List<SelectListItem> Veiculos { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> Servicos { get; set; } = new();

        public string? PlacaBusca { get; set; }
        public string? ClienteNome { get; set; }
        public string? VeiculoDescricao { get; set; }
    }
}