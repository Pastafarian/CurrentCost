namespace CurrentCost.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToUkDateTime(this DateTime utcDateTime)
        {
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, britishZone);
        }
    }
}
