using System;
using System.Globalization;

namespace Alyio.Extensions
{
    /// <summary>
    /// Extension methods for converting a <see cref="object"/> type to another base data type.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts the value of the specified object to its equivalent string representation.
        /// </summary>
        /// <param name="value">An object that supplies the value to convert, or null.</param>
        /// <returns>The string representation of value, or System.String.Empty if value is null.</returns>
        public static string ToStringExt(this object value) => value?.ToString() ?? string.Empty;

        /// <summary>
        /// Converts the value of the specified object to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">An object that supplies the value to convert, or null.</param>
        /// <returns>A 32-bit signed integer equivalent to value, or null if value is null or was not converted successfully.</returns>
        public static int? ToInt32(this object value)
        {
            if (value == null) { return null; }
            if (typeof(int).Equals(value.GetType())) { return (int)value; }
            var converter = value as IConvertible;
            if (converter != null)
            {
                try
                {
                    return converter.ToInt32(CultureInfo.InvariantCulture);
                }
                catch
                {
                    var s = value.ToString();
                    return s.ToInt32();
                }
            }
            else
            {
                var s = value.ToString();
                return s.ToInt32();
            }
        }

        /// <summary>
        /// Converts the value of the specified object to a 64-bit signed integer.
        /// </summary>
        /// <param name="value">An object that supplies the value to convert, or null.</param>
        /// <returns>A 64-bit signed integer equivalent to value, or null if value is null or was not converted successfully.</returns>
        public static long? ToInt64(this object value)
        {
            if (value == null) { return null; }
            if (typeof(long).Equals(value.GetType())) { return (long)value; }
            var converter = value as IConvertible;
            if (converter != null)
            {
                try
                {
                    return converter.ToInt64(CultureInfo.InvariantCulture);
                }
                catch
                {
                    var s = value.ToString();
                    return s.ToInt64();
                }
            }
            else
            {
                var s = value.ToString();
                return s.ToInt64();
            }
        }

        /// <summary>
        /// Converts the value of the specified object to a double-precision floating-point number.
        /// </summary>
        /// <param name="value">An object that supplies the value to convert, or null.</param>
        /// <returns>A double-precision floating-point number equivalent to value, or null if value is null or was not converted successfully.</returns>
        public static double? ToDouble(this object value)
        {
            if (value == null) { return null; }
            if (typeof(double).Equals(value.GetType())) { return (double)value; }
            var converter = value as IConvertible;
            if (converter != null)
            {
                try
                {
                    return converter.ToDouble(CultureInfo.InvariantCulture);
                }
                catch
                {
                    var s = value.ToString();
                    return s.ToDouble();
                }
            }
            else
            {
                var s = value.ToString();
                return s.ToDouble();
            }
        }

        /// <summary>
        /// Converts the value of the specified object to a decimal number.
        /// </summary>
        /// <param name="value">An object that supplies the value to convert, or null.</param>
        /// <returns>A decimal number equivalent to value, or null if value is null or was not converted successfully.</returns>
        public static decimal? ToDecimal(this object value)
        {
            if (value == null) { return null; }
            if (typeof(decimal).Equals(value.GetType())) { return (decimal)value; }
            var converter = value as IConvertible;
            if (converter != null)
            {
                try
                {
                    return converter.ToDecimal(CultureInfo.InvariantCulture);
                }
                catch
                {
                    var s = value.ToString();
                    return s.ToDecimal();
                }
            }
            else
            {
                var s = value.ToString();
                return s.ToDecimal();
            }
        }

        /// <summary>
        /// Converts the value of the specified object to a date and time.
        /// </summary>
        /// <param name="value">An object that supplies the value to convert, or null.</param>
        /// <returns>The date and time equivalent of the value of value, or null if value is null or was not converted successfully.</returns>
        public static DateTime? ToDateTime(this object value)
        {
            if (value == null) { return null; }
            if (typeof(DateTime).Equals(value.GetType())) { return (DateTime)value; }
            var converter = value as IConvertible;
            if (converter != null)
            {
                try
                {
                    return converter.ToDateTime(CultureInfo.InvariantCulture);
                }
                catch
                {
                    var str = value.ToString();
                    return str.ToDateTime();
                }
            }
            else
            {
                var str = value.ToString();
                return str.ToDateTime();
            }
        }

        /// <summary>
        /// Converts the value of the specified object to a date without time.
        /// </summary>
        /// <param name="value">An object that supplies the value to convert, or null.</param>
        /// <returns>The date without time equivalent of the value of value, or null if value is null or was not converted successfully.</returns>
        public static DateTime? ToDate(this object value)
        {
            return value.ToDateTime()?.Date;
        }
    }
}
