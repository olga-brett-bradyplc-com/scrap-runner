using System;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Domain.Models;
using BWF.DataServices.PortableClients;
using BWF.DataServices.PortableClients.Builder;
using BWF.DataServices.PortableClients.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brady.ScrapRunner.DataService.Tests
{

    /// <summary>
    /// Rudimentary unit tests.
    /// </summary>
    [TestClass]
    public class UnitTest1
    {

        /// <summary>
        /// A silly example (why make trivial tests of trivial property sets and gets) to 
        /// illustrate a unit test.
        /// </summary>
        [TestMethod]
        public void TestCodeTable()
        {
            CodeTable codeTable = new CodeTable();
            codeTable.CodeName = "1";
            codeTable.CodeSeq = 2;
            codeTable.CodeValue = "3";
            codeTable.CodeDisp1 = "Disp1";
            codeTable.CodeDisp2 = "Disp2";
            codeTable.CodeDisp3 = "Disp3";
            codeTable.CodeDisp4 = "Disp4";
            codeTable.CodeDisp5 = "Disp5";
            codeTable.CodeDisp6 = "Disp6";

            Assert.AreEqual("1", codeTable.CodeName, "Wow is this trivial test actually failing?");
            Assert.AreEqual(2, codeTable.CodeSeq);
            Assert.AreEqual("3", codeTable.CodeValue);
            Assert.AreEqual("Disp1", codeTable.CodeDisp1);
            Assert.AreEqual("Disp2", codeTable.CodeDisp2);
            Assert.AreEqual("Disp3", codeTable.CodeDisp3);
            Assert.AreEqual("Disp4", codeTable.CodeDisp4);
            Assert.AreEqual("Disp5", codeTable.CodeDisp5);
            Assert.AreEqual("Disp6", codeTable.CodeDisp6);
        }

    }
}
