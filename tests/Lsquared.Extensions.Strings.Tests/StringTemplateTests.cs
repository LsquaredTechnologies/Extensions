using System;
using Lsquared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lsquared.Extensions.Strings.Tests
{
    [TestClass]
    public class StringTemplateTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var expected ="Hello John Doe";

            var st = StringTemplate.Create("Hello {name}");
            var actual = st.Format("John Doe");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var expected ="Hello {0}";

            var st =  StringTemplate.Create("Hello {{0}}");
            var actual = st.Format("John Doe");

            Assert.AreEqual(expected, actual);
        }
    }
}
