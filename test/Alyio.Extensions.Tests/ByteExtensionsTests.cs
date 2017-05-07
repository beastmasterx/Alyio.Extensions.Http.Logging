using System.Globalization;
using Xunit;

namespace Alyio.Extensions.Tests
{
    public class ByteExtensionsTests
    {
        private const string HEX = "19E9B9F3350B49189A2CC27D667541C5";

        [Fact]
        public void Test()
        {
            var bytes = new byte[HEX.Length / 2];

            for (int i = 0, j = 0; i < HEX.Length; i += 2, j++)
            {
                bytes[j] = byte.Parse(HEX.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            var hex = bytes.ToHex();

            Assert.Equal(HEX, hex);
        }
    }
}
