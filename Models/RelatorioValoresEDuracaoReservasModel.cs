namespace api_aggregations.Models;

public class RelatorioValoresEDuracaoReservas
{
    public string DataInicio { get; set; }
    public string DataFim { get; set; }

    public int IdServico { get; set; }
    public int IdProduto { get; set; }

    public string? AbreviaturaProduto { get; set; }

    public int IdDispBase { get; set; }
    public string? RefDispBase { get; set; }

    public string? Lugar { get; set; }

    public int Quantidade { get; set; }
    public int Duracao { get; set; }
    public decimal Valor { get; set; }
}
