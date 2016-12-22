using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Alyio.Extensions.Tests
{
    public class BooleanExtensionsTests
    {
        [Fact]
        public void Object_Tests()
        {
            Assert.True(bool.TrueString.ToBoolean());
            Assert.False(bool.FalseString.ToBoolean());

            Assert.True(1.0D.ToBoolean());
            Assert.False(0.0D.ToBoolean());

            Assert.True(true.ToBoolean());
            Assert.False(false.ToBoolean());

            object t = true;
            Assert.True(t.ToBoolean());
            object f = false;
            Assert.False(f.ToBoolean());

            object t1 = "1";
            Assert.True(t1.ToBoolean());
            object t2 = "0";
            Assert.False(t2.ToBoolean());

            var mdnz = new MyDoubleNotZero();
            Assert.True(mdnz.ToBoolean());
            var mdz = new MyDoubleZero();
            Assert.False(mdz.ToBoolean());
        }

        class MyDoubleZero
        {
            public override string ToString()
            {
                return "0.0";
            }
        }

        class MyDoubleNotZero
        {
            public override string ToString()
            {
                return "1.0";
            }
        }
    }
}
