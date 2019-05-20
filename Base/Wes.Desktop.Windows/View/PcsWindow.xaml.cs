using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Wes.Desktop.Windows;
using Wes.Utilities;
using Wes.Utilities.Languages;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wes.Wrapper;
using Wes.Addins.Addin;
using Wes.Utilities.Exception;

namespace Wes.Desktop.Windows.View
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PcsWindow : BaseWindow
    {
        public PcsWindow()
        {
            InitializeComponent();
        }

        public int Total { get; set; }
        public double Gw { get; set; }

        private void UIElement_OnKeyUp_1(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtTotal.Text))
            {
                int result;
                if (int.TryParse(this.txtTotal.Text, out result))
                {
                    this.Total = result;
                    return;
                }
            }
            throw new WesException("請輸入正確的數量");
        }

        private void UIElement_OnKeyUp_2(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (string.IsNullOrEmpty(this.txtGw.Text))
            {
                double result;
                if (double.TryParse(this.txtGw.Text, out result))
                {
                    this.Gw = result;
                    return;
                }
            }
            throw new WesException("請輸入正確的重量");
        }
    }
}