namespace Akade.IndexedSet.Tests.Samples.Appointments;

internal static class DateOnlyExtensions
{
    public static DateTime WithDayTime(this DateOnly dateOnly, int hours, int minutes)
    {
        return new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, hours, minutes, 0);
    }
}
