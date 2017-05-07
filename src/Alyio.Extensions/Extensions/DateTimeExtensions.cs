using System;
using System.Globalization;

namespace Alyio.Extensions
{
    /// <summary>
    /// Extension methods for converting a <see cref="string"/> type to another base data type.
    /// </summary>
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts the <see cref="DateTime"/> object as its equivalent string representation to an equivalent 32-bit signed integer using the specified 'yyyyMMdd' format.
        /// </summary>
        /// <param name="datetime">The date and time value to convert.</param>
        /// <returns>A 32-bit signed integer.</returns>
        public static int ToDateInt32(this DateTime datetime)
        {
            return int.Parse(datetime.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> object to its equivalent string representation using the specified 'yyyy-MM-dd HH:mm:ss' format.
        /// </summary>
        /// <param name="datetime">The date and time value to convert.</param>
        /// <returns>The string representation of value.</returns>
        public static string ToyyyyMMddHHmmss(this DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> object to its equivalent unix time stamp.
        /// </summary>
        /// <param name="datetime">The date and time value to convert.</param>
        /// <returns>A 64-bit signed integer representation of unix time stamp.</returns>
        public static long ToUnixTimestamp(this DateTime datetime)
        {
            var timeSpan = datetime.ToUniversalTime().Subtract(UnixEpochTime);
            var timestamp = (long)timeSpan.TotalSeconds;
            return timestamp;
        }
    }
}
