using System;

namespace Alyio.Extensions
{
    public static class ByteExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            var s = BitConverter.ToString(bytes).ToLowerInvariant().Replace("-", string.Empty);
            return s;
        }

        public static string ToUpperHexString(this byte[] bytes)
        {
            var s = BitConverter.ToString(bytes).Replace("-", string.Empty);
            return s;
        }
    }
}
