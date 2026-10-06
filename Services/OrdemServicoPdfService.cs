using MecHub.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace MecHub.Services
{
    public class OrdemServicoPdfService
    {
        public byte[] GerarPdf(OrdemServico ordem)
        {
            var cultura = new CultureInfo("pt-BR");

            var total = ordem.Itens?
                .Sum(i => i.Quantidade * i.Servico.Valor) ?? 0;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.PageColor("#FFFFFF");

                    page.DefaultTextStyle(x => x
                        .FontSize(10)
                        .FontColor("#111827"));

                    page.Header().Element(c => Header(c, ordem));

                    page.Content().Column(column =>
                    {
                        column.Spacing(18);

                        column.Item().Element(c => SectionTitle(c, "Dados da emissão"));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => InfoBox(c, "Data/Hora da emissão", DateTime.Now.ToString("dd/MM/yyyy HH:mm")));
                            row.RelativeItem().Element(c => InfoBox(c, "Nº da OS", $"#{ordem.Id}"));
                            row.RelativeItem().Element(c => InfoBox(c, "Status", ordem.StatusOrdem.ToString()));
                        });

                        column.Item().Element(c => SectionTitle(c, "Mecânico emitente"));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => InfoBox(c, "Nome", ordem.Mecanico?.Usuario?.Nome ?? "Não informado"));
                            row.RelativeItem().Element(c => InfoBox(c, "Telefone", ordem.Mecanico?.Telefone ?? "Não informado"));
                            row.RelativeItem().Element(c => InfoBox(c, "E-mail", ordem.Mecanico?.Usuario?.Email ?? "Não informado"));
                        });

                        column.Item().Element(c => SectionTitle(c, "Cliente responsável"));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => InfoBox(c, "Nome", ordem.Cliente?.Nome ?? "Não informado"));
                            row.RelativeItem().Element(c => InfoBox(c, "Telefone", ordem.Cliente?.Telefone ?? "Não informado"));
                            row.RelativeItem().Element(c => InfoBox(c, "E-mail", ordem.Cliente?.Email ?? "Não informado"));
                        });

                        column.Item().Element(c => SectionTitle(c, "Informações do veículo"));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => InfoBox(c, "Placa", ordem.Veiculo?.Placa ?? "Não informado"));
                            row.RelativeItem().Element(c => InfoBox(c, "Marca/Modelo", $"{ordem.Veiculo?.Marca} {ordem.Veiculo?.Modelo}"));
                            row.RelativeItem().Element(c => InfoBox(c, "Cor/Ano", $"{ordem.Veiculo?.Cor} - {ordem.Veiculo?.AnoFabricacao}"));
                        });

                        column.Item().Element(c => SectionTitle(c, "Serviços sugeridos pelo mecânico"));

                        column.Item().Element(c => ServicesTable(c, ordem, cultura));

                        column.Item().AlignRight().PaddingTop(10).Text($"Total estimado: {total.ToString("C", cultura)}")
                            .FontSize(15)
                            .Bold()
                            .FontColor("#FF022E");
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Documento gerado automaticamente pelo ");
                        text.Span("MEC").Bold();
                        text.Span("HUB").Bold().FontColor("#FF022E");
                        text.Span(".");
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        private void Header(IContainer container, OrdemServico ordem)
        {
            container
                .PaddingBottom(18)
                .BorderBottom(1)
                .BorderColor("#E5E7EB")
                .Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("MEC").FontSize(28).Bold().FontColor("#111827");
                            text.Span("HUB").FontSize(28).Bold().FontColor("#FF022E");
                        });

                        col.Item().Text("Ordem de Serviço")
                            .FontSize(13)
                            .FontColor("#6B7280");
                    });

                    row.ConstantItem(160).AlignRight().Column(col =>
                    {
                        col.Item().Text($"OS #{ordem.Id}")
                            .FontSize(16)
                            .Bold()
                            .FontColor("#FF022E");

                        col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                            .FontSize(9)
                            .FontColor("#6B7280");
                    });
                });
        }

        private void SectionTitle(IContainer container, string title)
        {
            container
                .PaddingTop(4)
                .PaddingBottom(6)
                .Text(title)
                .FontSize(13)
                .Bold()
                .FontColor("#FF022E");
        }

        private void InfoBox(IContainer container, string label, string value)
        {
            container
                .Padding(10)
                .Border(1)
                .BorderColor("#E5E7EB")
                .Background("#F9FAFB")
                .Column(col =>
                {
                    col.Item().Text(label)
                        .FontSize(8)
                        .Bold()
                        .FontColor("#6B7280");

                    col.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(value) ? "Não informado" : value)
                        .FontSize(10)
                        .FontColor("#111827");
                });
        }

        private void ServicesTable(IContainer container, OrdemServico ordem, CultureInfo cultura)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "Serviço");
                    HeaderCell(header.Cell(), "Tipo");
                    HeaderCell(header.Cell(), "Qtd.");
                    HeaderCell(header.Cell(), "Valor");
                });

                foreach (var item in ordem.Itens)
                {
                    var subtotal = item.Quantidade * item.Servico.Valor;

                    BodyCell(table.Cell(), item.Servico.Descricao);
                    BodyCell(table.Cell(), item.Servico.Tipo);
                    BodyCell(table.Cell(), item.Quantidade.ToString());
                    BodyCell(table.Cell(), subtotal.ToString("C", cultura));
                }
            });
        }

        private void HeaderCell(IContainer container, string text)
        {
            container
                .Background("#111827")
                .Padding(8)
                .Text(text)
                .FontColor("#FFFFFF")
                .Bold()
                .FontSize(9);
        }

        private void BodyCell(IContainer container, string text)
        {
            container
                .BorderBottom(1)
                .BorderColor("#E5E7EB")
                .Padding(8)
                .Text(text)
                .FontSize(9);
        }
    }
}