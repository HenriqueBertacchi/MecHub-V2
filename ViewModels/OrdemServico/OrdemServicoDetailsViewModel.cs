public class OrdemServicoDetailsViewModel
{
    public int Id { get; set; }

    public required string ClienteNome { get; set; }

    public required string VeiculoDescricao { get; set; }

    public required string Observacoes { get; set; }

    public required List<ItemOrdemServicoViewModel> Itens { get; set; }

    public decimal ValorTotal { get; set; }
}