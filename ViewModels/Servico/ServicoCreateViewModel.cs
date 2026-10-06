using System.ComponentModel.DataAnnotations;

namespace MecHub.ViewModel
{
    public class ServicoCreateViewModel
    {
        [Required(ErrorMessage = "Informe a descrição.")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o valor.")]
        [Range(0.01, 999999.99, ErrorMessage = "Informe um valor válido.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Informe o tipo.")]
        public string Tipo { get; set; } = string.Empty;
    }
}