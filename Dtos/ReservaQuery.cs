namespace api_aggregations.Dtos;

public sealed class ReservaQuery : PaginationQuery
{
    public string? numero { get; set; }
    public byte? tipo { get; set; }
    public short? estado { get; set; }
    public string? id_externo { get; set; }
}

