// <copyright file="ProgramTest.cs">Copyright ©  2015</copyright>
using System;
using CustomerCodeCoverage;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomerCodeCoverage.Tests
{
    /// <summary>This class contains parameterized unit tests for Program</summary>
    [PexClass(typeof(Program))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class ProgramTest
    {
        /// <summary>Test stub for add(Int32, Int32)</summary>
        [PexMethod]
        internal int addTest(
            [PexAssumeUnderTest]Program target,
            int a,
            int b
        )
        {
            int result = target.add(a, b);
            return result;
            // TODO: add assertions to method ProgramTest.addTest(Program, Int32, Int32)
        }
    }
}
