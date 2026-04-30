namespace api_aggregations.Dtos;

public sealed class ProdutoReservadoQuery : PaginationQuery
{
    /// <summary>optional</summary>
    public int? id_reserva { get; set; }
    /// <summary>optional</summary>
    public int? id_produto { get; set; }
    /// <summary>optional</summary>
    public short? estado { get; set; }
    /// <summary>optional</summary>
    public string? referencia { get; set; }
    /// <summary>optional</summary>
    public byte? agregado { get; set; }
}

