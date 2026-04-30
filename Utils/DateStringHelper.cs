namespace api_aggregations.Utils;

using System.Globalization;

public static class DateStringHelper
{
    public static string ToDateString(DateTime value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public static DateTime ParseDate(string value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public static DateTime? ParseDateOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ParseDate(value);
    }
}
