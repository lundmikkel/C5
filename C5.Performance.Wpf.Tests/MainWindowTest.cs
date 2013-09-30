// <copyright file="MainWindowTest.cs">Copyright ©  2013</copyright>
using System;
using System.Diagnostics.Contracts;
using C5.Performance.Wpf;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace C5.Performance.Wpf
{
    /// <summary>This class contains parameterized unit tests for MainWindow</summary>
    [PexClass(typeof(MainWindow))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class MainWindowTest
    {
        [PexMethod]

    }
}
