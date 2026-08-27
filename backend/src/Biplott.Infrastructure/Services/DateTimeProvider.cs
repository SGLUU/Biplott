using Biplott.Core.Interfaces;

namespace Biplott.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public string GetCurrentLocalDate(string timezoneId = "Asia/Ho_Chi_Minh")
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback for Windows/macOS/Linux compatibility
            if (timezoneId == "Asia/Ho_Chi_Minh")
            {
                try
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }
                catch
                {
                    tz = TimeZoneInfo.Utc;
                }
            }
            else
            {
                tz = TimeZoneInfo.Utc;
            }
        }

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(UtcNow, tz);
        return localTime.ToString("yyyy-MM-dd");
    }
}
