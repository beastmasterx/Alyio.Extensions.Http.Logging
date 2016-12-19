using System;
using System.Globalization;

namespace Alyio.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int To8DigitsDate(this DateTime dateTime)
        {
            return int.Parse(dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        }

        public static string ToyyyyMMddHHmmss(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static long ToUnixTimestamp(this DateTime datetime)
        {
            var timeSpan = datetime.ToUniversalTime().Subtract(UnixEpochTime);
            var timestamp = (long)timeSpan.TotalSeconds;
            return timestamp;
        }
    }
}
