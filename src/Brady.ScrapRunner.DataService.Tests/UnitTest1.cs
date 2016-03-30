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
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

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

        /// <summary>
        /// Another silly example to illustrate a unit test.  And get a better handle on the 
        /// behavior of the query builder.
        /// </summary>
        [TestMethod]
        public void IllustrateQueryBuilder()
        {

            // Simple AND of two clauses (nested within the AND)
            var qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And(em2 => em2.Property(x => x.AllowMessaging).EqualTo(Constants.Yes)));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and (AllowMessaging='Y')", qb.GetQuery());

            // Simple AND of two clauses (appended to AND)
            qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(x => x.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and AllowMessaging='Y'", qb.GetQuery());

            // Simple AND of three clauses (nested within the ANDs)
            var terminalid = "FOO";
            qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(emp => emp.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And(em2 => em2.Property(x => x.AllowMessaging).EqualTo(Constants.Yes))
                .And(em3 => em3.Property(x2 => x2.TerminalId).EqualTo(terminalid)));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and (AllowMessaging='Y') and (TerminalId='FOO')", qb.GetQuery());

            // Simple AND of three clauses (appended to ANDs)
            qb = new QueryBuilder<EmployeeMaster>();
            qb.Filter(em => em.Property(emp => emp.SecurityLevel).NotEqualTo(SecurityLevelConstants.Driver)
                .And().Property(x => x.AllowMessaging).EqualTo(Constants.Yes)
                .And().Property(x2 => x2.TerminalId).EqualTo(terminalid));
            Assert.AreEqual("EmployeeMasters?$filter=SecurityLevel!='DR' and AllowMessaging='Y' and TerminalId='FOO'", qb.GetQuery());

            var qb2 = new QueryBuilder<CodeTable>();
            qb2.Filter(ct => ct.Property(p => p.CodeName).EqualTo(CodeTableNameConstants.DelayCodes)
                    .And().Parenthesis(paren => paren.Property(p => p.CodeDisp5)
                        .EqualTo("SDF").Or(x => x.CodeDisp5).IsNull()));
            Assert.AreEqual("CodeTables?$filter=CodeName='DELAYCODES' and (CodeDisp5='SDF' or CodeDisp5 isNull)", qb2.GetQuery());

        }

    }
}
