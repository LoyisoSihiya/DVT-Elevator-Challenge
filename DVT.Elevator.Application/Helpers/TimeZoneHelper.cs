namespace DVT.Elevator.Application.Helpers;

/// <summary>
/// Converts UTC datetimes to South African Standard Time (SAST = UTC+2).
/// </summary>
public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo Sast =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "South Africa Standard Time"
                : "Africa/Johannesburg");

    /// <summary>Converts a UTC DateTime to SAST.</summary>
    public static DateTime ToSast(DateTime utcDateTime)
    {
        var dt = utcDateTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)
            : utcDateTime;

        return TimeZoneInfo.ConvertTimeFromUtc(dt.ToUniversalTime(), Sast);
    }

    /// <summary>Formats a UTC DateTime as a SAST time string HH:mm:ss.</summary>
    public static string FormatTime(DateTime utcDateTime)
        => ToSast(utcDateTime).ToString("HH:mm:ss");

    /// <summary>Formats a UTC DateTime as a full SAST datetime string.</summary>
    public static string FormatDateTime(DateTime utcDateTime)
        => ToSast(utcDateTime).ToString("yyyy-MM-dd HH:mm:ss");
}
