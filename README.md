# Alyio.Extensions

*ObjectExtensions.cs*

```csharp
using System;
using System.Globalization;

namespace Alyio.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A string that represents the current object or <see cref="string.Empty"/> if <paramref name="value"/> is null.</returns>
        public static string ToStringExt(this object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else
            {
                return value.ToString();
            }
        }

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

        public static DateTime? ToDate(this object value)
        {
            return value.ToDateTime()?.Date;
        }
    }
}
```
