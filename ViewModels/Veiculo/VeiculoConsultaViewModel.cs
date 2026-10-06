using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MecHub.ViewModel
{
    public class VeiculoConsultaViewModel
    {
        [Required(ErrorMessage = "Informe a placa do veículo")]
        [Display(Name = "Placa do veículo")]
        [Column("placa")]
        public string Placa { get; set; }
    }
}