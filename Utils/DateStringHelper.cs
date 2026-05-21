namespace api_aggregations.Utils;

using System.Globalization;

public static class DateStringHelper
{
    // Store dates in round-trip format so no timezone/detail is lost.
    public static string ToDateString(DateTime value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    // Always parse using the same culture. This avoids date bugs such as
    // confusing day/month order between machines.
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
