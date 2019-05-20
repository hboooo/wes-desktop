
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Wes.Print;

namespace Wes.Customer.Promaster.View
{
    /// <summary>
    /// HandPrint.xaml 的交互逻辑
    /// </summary>
    public partial class HandPrint : UserControl
    {
        public HandPrint()
        {
            InitializeComponent();
        }
        private void BtnMainPrint_Click(object sender, RoutedEventArgs e)
        {
            int printNum = 0;
            if (!Int32.TryParse(TxtNum1.Text.Trim(), out printNum))
            {
                MessageBox.Show("張數格式不正確，應為數字！");
                return;
            }
            if (printNum < 1)
            {
                MessageBox.Show("張數必須大於0！");
                return;
            }

            for (int i = 0; i < printNum; i++)
            {
                var pm = new PrintTemplateModel();
                pm.PrintDataValues = new Dictionary<string, object>
                {
                    { "PN", TxtPN.Text },
                    { "LOT", TxtLotNo.Text },
                    { "DC", TxtDataCode.Text },
                    { "QTY", TxtQty.Text },
                };
                pm.TemplateFileName = "MCC_DISPARTLABEL.btw";
                LabelPrintBase lpb = new LabelPrintBase(pm, false);
                lpb.PrintParam.IsCheck = false;
                var res = lpb.Print();
            }
            ClearMainLabel();
        }
        private void ClearMainLabel()
        {
            TxtPN.Text = string.Empty;
            TxtPN.Focus();
            TxtQty.Text = string.Empty;
            TxtLotNo.Text = string.Empty;
            TxtDataCode.Text = string.Empty;
            TxtNum1.Text = "1";
        }
    }
}
