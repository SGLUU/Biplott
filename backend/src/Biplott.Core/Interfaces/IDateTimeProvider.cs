namespace Biplott.Core.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    string GetCurrentLocalDate(string timezoneId = "Asia/Ho_Chi_Minh");
}
