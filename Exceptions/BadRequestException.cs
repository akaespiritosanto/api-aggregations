namespace api_aggregations.Exceptions;

public sealed class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(message, statusCode: 400)
    {
    }
}
