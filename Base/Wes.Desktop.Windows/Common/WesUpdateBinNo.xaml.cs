using System;
using System.Windows.Input;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Desktop.Windows.Common
{
    /// <summary>
    /// WesUpdateBinNo.xaml 的交互逻辑
    /// </summary>
    public partial class WesUpdateBinNo : BaseWindow
    {
        private readonly CommonUpdateBinNoModel _binNoModel;
        private BinAction _binAction = BinAction.Update;
        public string BinNo { get; set; }

        public WesUpdateBinNo(CommonUpdateBinNoModel model, BinAction binAction = BinAction.Update)
        {
            InitializeComponent();
            _binNoModel = model;
            _binAction = binAction;
            txtBinNo.Focus();
            txtTip.Content = string.Format("請掃描 {0} 新儲位 {1}", _binNoModel.PackageId, _binNoModel.YushuQty);
        }

        public WesUpdateBinNo(string toolTip)
        {
            InitializeComponent();
            _binAction = BinAction.Add;
            txtBinNo.Focus();
            txtTip.Content = $"請掃描 {toolTip} 新儲位";
        }

        private void TxtBinNo_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            string binNo = txtBinNo.Text;
            if (string.IsNullOrWhiteSpace(binNo))
            {
                WesModernDialog.ShowWesMessage("儲位不能為空，请扫描储位");
                return;
            }

            if (binNo.Equals("Close", StringComparison.OrdinalIgnoreCase))
            {
                Close();
                return;
            }

            if (_binAction == BinAction.Update)
            {
                dynamic result = RestApi.NewInstance(Method.POST)
                    .SetUrl(RestUrlType.WmsServer, "/1/{si}")
                    .AddScriptId(ScriptID.UPDATE_BIN_NO)
                    .AddJsonBody("pxt", _binNoModel.OperationNo)
                    .AddJsonBody("actionType", _binNoModel.ActionType)
                    .AddJsonBody("pid", _binNoModel.PackageId)
                    .AddJsonBody("binNo", binNo)
                    .AddJsonBody("user", _binNoModel.UpdateUser)
                    .Execute()
                    .To<object>();
            }
            else
            {
                if (_binNoModel != null)
                {
                    _binNoModel.BinNo = binNo;
                }

                BinNo = binNo;
            }

            Close();
        }
    }

    /// <summary>
    /// 储位动作
    /// </summary>
    public enum BinAction
    {
        /// <summary>
        /// 设置储位
        /// </summary>
        Add,

        /// <summary>
        /// 更新储位
        /// </summary>
        Update,
    }
}