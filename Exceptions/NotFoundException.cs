namespace api_aggregations.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(message, statusCode: 404)
    {
    }
}

