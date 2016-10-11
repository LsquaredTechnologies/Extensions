using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lsquared.Extensions.StringTemplate.Tests
{
    [TestClass]
    public class StringTemplateTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var expected ="Hello John Doe";

            var st = new StringTemplate("Hello {name}");
            var actual = st.Format("John Doe");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var expected ="Hello {0}";

            var st = new StringTemplate("Hello {{0}}");
            var actual = st.Format("John Doe");

            Assert.AreEqual(expected, actual);
        }
    }
}
