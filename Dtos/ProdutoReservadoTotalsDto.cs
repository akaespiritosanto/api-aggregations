namespace api_aggregations.Dtos;

public sealed class ProdutoReservadoTotalsDto
{
    public required int ano { get; init; }
    public int? mes { get; init; }
    public int? dia { get; init; }

    public int? id_entidade { get; init; }

    public required int quantidade { get; init; }
    public required double valor_tarifa { get; init; }
    public required double valor_comissao { get; init; }
    public required double valor_taxas { get; init; }
    public required double valor_descontos { get; init; }
}

