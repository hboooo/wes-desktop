using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wes.Utilities.Extends;

namespace Wes.Core.Tests.Extend
{
    [TestClass()]
    public class ENameTests
    {
        [TestMethod()]
        public void Underline2HumpTest()
        {
            var result = "QR_CODE".Underline2Hump();
            Assert.AreEqual("QrCode", result); 
        }
    }
}