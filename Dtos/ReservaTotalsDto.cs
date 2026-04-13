namespace api_aggregations.Dtos;

public sealed class ReservaTotalsDto
{
    public required int ano { get; init; }
    public int? mes { get; init; }
    public int? dia { get; init; }

    public required int id_vendedor { get; init; }

    // "quantidade" here means how many reservas exist in the group.
    public required int quantidade { get; init; }
}

