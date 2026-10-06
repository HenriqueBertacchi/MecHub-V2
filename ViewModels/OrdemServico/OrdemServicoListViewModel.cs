namespace MecHub.ViewModel
{
    public class OrdemServicoListViewModel
    {
        public int Id { get; set; }

        public string MecanicoNome { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;

        public string VeiculoPlaca { get; set; } = string.Empty;
        public string VeiculoMarca { get; set; } = string.Empty;
        public string VeiculoModelo { get; set; } = string.Empty;

        public string StatusTexto { get; set; } = string.Empty;
        public string StatusClasse { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }
        public DateTime? DataFechamento { get; set; }

        public int QuantidadeItens { get; set; }
        public decimal ValorTotal { get; set; }
    }
}