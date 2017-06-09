using Xunit;

namespace Alyio.Extensions.Tests
{
    public class DateTimeExtensionsTests
    {
        // "2017-06-09 10:00:00 +0000 UTC -- 1497002400";
        [Fact]
        public void ToUnix()
        {
            var secs = 1497002400L;
            var u = secs.ToDateTime().Value.ToUnix();
            Assert.Equal(1497002400L, u);
        }
    }
}
