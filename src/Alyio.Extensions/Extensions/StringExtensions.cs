using System;

namespace Alyio.Extensions
{
    public static class StringExtensions
    {
        public static int? ToInt32(this string s)
        {
            int result;
            if (int.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static long? ToInt64(this string s)
        {
            long result;
            if (long.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static double? ToDouble(this string s)
        {
            double result;
            if (double.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static decimal? ToDecimal(this string s)
        {
            decimal result;
            if (decimal.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static DateTime? ToDateTime(this string s)
        {
            DateTime dateTime;
            if (DateTime.TryParse(s, out dateTime))
            {
                return dateTime;
            }
            {
                return null;
            }
        }
    }
}
