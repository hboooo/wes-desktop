using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wes.Print;

namespace Wes.Core.Tests.Sort
{
    [TestClass]
    public class PrintTempSortTest
    {
        [TestMethod]
        public void Test()
        {
            List<PrintTemplateModel> list = new List<PrintTemplateModel>();
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("SplitPkgID", "C001");
            pm.SortKey = "SplitPkgID";

            list.Add(pm);

            pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("SplitPkgID", "C005");
            pm.SortKey = "SplitPkgID";
            list.Add(pm);

            pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("SplitPkgID", "C002");
            pm.SortKey = "SplitPkgID";
            list.Add(pm);

            pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("SplitPkgID", "C009");
            pm.SortKey = "SplitPkgID";
            list.Add(pm);

            list.Sort();
            //  list.Sort(pm);
            Assert.AreEqual("QrCode", list);
            var s = list;
        }
    }
}
