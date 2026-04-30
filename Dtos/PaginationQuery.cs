namespace api_aggregations.Dtos;

public class PaginationQuery
{
    /// <summary>optional</summary>
    public int PageNumber { get; set; } = 1;
    /// <summary>optional</summary>
    public int PageSize { get; set; } = 20;
}

