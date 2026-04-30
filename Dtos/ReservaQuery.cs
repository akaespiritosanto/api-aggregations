namespace api_aggregations.Dtos;

public sealed class ReservaQuery : PaginationQuery
{
    /// <summary>optional</summary>
    public string? numero { get; set; }
    /// <summary>optional</summary>
    public byte? tipo { get; set; }
    /// <summary>optional</summary>
    public short? estado { get; set; }
    /// <summary>optional</summary>
    public string? id_externo { get; set; }
}

