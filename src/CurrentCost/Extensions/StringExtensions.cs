using System.Globalization;

namespace CurrentCost.Extensions
{
    public static class StringExtensions
    {
        public static string ConvertToUkDateTime(this string dateTime)
        {
            var dateTimeParsed = DateTime.ParseExact(dateTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            var newDate = TimeZoneInfo.ConvertTime(dateTimeParsed, TimeZoneInfo.Local, britishZone);
            return newDate.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}
