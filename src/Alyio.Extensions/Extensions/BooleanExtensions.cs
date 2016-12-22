using System;
using System.Globalization;

namespace Alyio.Extensions
{
    public static class BooleanExtensions
    {
        /// <summary>
        /// Converts the value of a specified object to an equivalent Boolean value.
        /// </summary>
        /// <param name="value">The object to convert. </param>
        /// <returns>
        /// <see cref="System.Object"/>: true or false, which reflects the value returned by invoking the <see cref="IConvertible.ToBoolean"/> method for the underlying type of value. If value is null, the method returns false. 
        /// <see cref="System.String"/>: true if value equals TrueString, or false if value equals FalseString or null.
        /// <see cref="System.Double"/>: true if value is not zero; otherwise, false.
        /// </returns>
        public static bool ToBoolean(this object value)
        {
            if (value == null) { return false; }
            if (typeof(bool).Equals(value.GetType())) { return (bool)value; }
            var converter = value as IConvertible;
            if (converter != null)
            {
                try
                {
                    return converter.ToBoolean(CultureInfo.InvariantCulture);
                }
                catch
                {
                    var d = value.ToDouble();
                    if (d != null)
                    {
                        return d != 0D;
                    }
                    else
                    {
                        var s = value.ToString();
                        return s.ToBoolean();
                    }
                }
            }
            else
            {
                var d = value.ToDouble();
                if (d != null)
                {
                    return d != 0D;
                }
                else
                {
                    var s = value.ToString();
                    return s.ToBoolean();
                }
            }
        }

        /// <summary>
        /// Converts the specified string representation of a logical value to its <see cref="System.Boolean"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing the value to convert.</param>
        /// <returns>true if value is equal to <see cref="System.Boolean.TrueString"/> or false if value is equal to <see cref="System.Boolean.FalseString"/>, otherwise false.</returns>
        public static bool ToBoolean(this string s)
        {
            bool result;
            if (bool.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return false;
            }
        }
    }
}
