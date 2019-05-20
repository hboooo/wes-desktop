using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Print;

namespace Wes.Core.Tests.Print
{
    /// <summary>
    /// PrintTest 的摘要说明
    /// </summary>
    [TestClass]
    public class PrintTest
    {
        public PrintTest()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void DownLoadTemplate()
        {
            string filename = string.Empty;
            FtpDownload ftpDownload = new FtpDownload();
            bool res = ftpDownload.Download("AmazonDuke_Carton.btw", ref filename);
            Assert.AreEqual(true, res);
        }

        [TestMethod]
        public void DownLoadTemplateFalse()
        {
            string filename = string.Empty;
            FtpDownload ftpDownload = new FtpDownload();
            bool res = ftpDownload.Download("111.btw", ref filename);
            Assert.AreEqual(false, res);


        }

        [TestMethod]
        public void ErrorPrint1()
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("PartNo", "111111111");
            pm.TemplateFileName = "AmazonDuke_Carton.btw";

            LabelPrintBase lpb = new LabelPrintBase(pm);
            var res = lpb.Print();
            Assert.AreEqual(ErrorCode.Unrealized, res);
        }

        [TestMethod]
        public void ErrorPrint2()
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.TemplateFileName = "AmazonDuke_Carton.btw";

            LabelPrintBase lpb = new LabelPrintBase(pm);
            var res = lpb.Print();
            Assert.AreEqual(ErrorCode.DataError, res);
        }

        [TestMethod]
        public void ErrorPrint3()
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("PartNo", "111111111");
            pm.PrintDataValues.Add("Po", "333333333");
            pm.TemplateFileName = "test.btw";

            LabelPrintBase lpb = new LabelPrintBase(pm);
            var res = lpb.Print();
            Assert.AreEqual(ErrorCode.DownloadTemplateFailure, res);
        }

        [TestMethod]
        public void ErrorPrint4()
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("PartNo", "abc");
            pm.PrintDataValues.Add("Po", "abc");
            pm.TemplateFileName = "AmazonDuke_Carton.btw";

            LabelPrintBase lpb = new LabelPrintBase(pm);
            var res = lpb.Print();
            Assert.AreEqual(ErrorCode.InternalError, res);
        }

        [TestMethod]
        public void Print()
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("PartNo", "111111111");
            pm.PrintDataValues.Add("Po", "333333333");
            pm.PrintDataValues.Add("Qty", "444444444");
            pm.PrintDataValues.Add("Upc", "0812751008507");
            pm.PrintDataValues.Add("LotNo", "666666666");
            pm.PrintDataValues.Add("ImeiList", "777777777");
            pm.PrintDataValues.Add("PO", "888888888");
            pm.TemplateFileName = "AmazonDuke_Carton.btw";

            LabelPrintBase lpb = new LabelPrintBase(pm);
            var res = lpb.Print();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var res1 = lpb.Print();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds.ToString());
            Assert.AreEqual(ErrorCode.Success, res);
        }
        
    }
}
