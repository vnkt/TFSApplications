using CodeCoverage;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace CodeCoverage.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        [TestMethod()]
        public void getBuildTest()
        {
            CodeCoverage.Program source = new Program();

            int input1 = 10;
            int input2 = 20;
            Debug.WriteLine(source.getBuild(input1, input2));

            Assert.AreEqual(source.getBuild(input1, input2), input1 + input2);
        }
    }
}