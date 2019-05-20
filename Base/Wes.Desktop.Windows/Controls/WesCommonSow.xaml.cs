using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wes.Desktop.Windows.Model;
using Wes.Desktop.Windows.ViewModel;
using Wes.Print;
using Wes.Wrapper;

namespace Wes.Desktop.Windows.Controls
{
    /// <summary>
    /// WesCommonSow.xaml 的交互逻辑
    /// </summary>
    public partial class WesCommonSow
    {
        readonly TranslateTransform _translate = new TranslateTransform();
        private Point _startPoint;//开始位置
        private Point _endPoint;//结束位置
        private int pageIndex = 1;
        private int pageSize = 9;
        private string selectedCartonId = "";
        private IEnumerable<CommonSowModel> _sowDataSource = new List<CommonSowModel>();//总数据源
        private IEnumerable<CommonSowModel> _currDataSource = new List<CommonSowModel>();//当前九宫格数据源
        private readonly List<Tuple<string, int>> _lstLoadingNoColor = new List<Tuple<string, int>>(); //loadingNo,除3餘數

        public WesCommonSow()
        {
            InitializeComponent();
            this.RenderTransform = _translate;
            InitGrid();
        }

        private void InitGrid()
        {
            //页面初始化
            for (int i = 0; i < 9; i++)
            {
                var viewModel = new GridSowViewModel();
                viewModel.Num = i + 1;
                viewModel.NCartonNo = string.Empty;
                viewModel.QtyDesc = string.Empty;
                viewModel.NumFontSize = 84;
                viewModel.NumForeground = Brushes.Black;
                viewModel.Background = Brushes.DarkSeaGreen;
                var gridSow = new WesGridSow();
                gridSow.DataContext = viewModel;
                gridSow.BorderThickness = new Thickness(1, 1, 1, 1);
                gridSow.BorderBrush = new SolidColorBrush(Colors.Black);
                this.gridSow.Children.Add(gridSow);

                var x = i / 3;
                var y = i % 3;
                Grid.SetRow(gridSow, x);
                Grid.SetColumn(gridSow, y);
            }
            _sowDataSource = null;
            _currDataSource = null;
            SelectedCartonId = "";
        }

        private void GridSow_Cell_DlbClick(object sender, EventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            if (sp.DataContext == null)
                return;
            var model = sp.DataContext as GridSowViewModel;
            var pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>
            {
                { "NCartonNo", model.NCartonNo },
                { "RowID", model.Num.ToString() }
            };
            pm.TemplateFileName = "Sinopower_NewCartonLabel.btw";
            LabelPrintBase lpb = new LabelPrintBase(new List<PrintTemplateModel>() { pm });
            var res = lpb.Print();

            if (WesDesktop.Instance.AddIn.EndCustomer == "C03694")
            {
                //针对信邦客户需要多打一张标签
                DataTable result = RestApi.NewInstance(Method.GET)
                    .AddUri("/dispatching/print-customer")
                    .AddQueryParameter("newCartonNo", model.NCartonNo)
                    .Execute()
                    .To<DataTable>();
                if (result != null && result.Rows.Count > 0)
                {
                    string[] needPrintConsignee = new string[] { "C03829","C03830","C03831","C03832","C03833","C03834","C03835","C03836","C03837",
                        "C03838","C03839","C03840","C03841","C03842","C03843","C03844","C03845","C03846","C03847","C03848","C03920","C03939","C03940","C03941" };
                    foreach (DataRow row in result.Rows)
                    {
                        string consignee = row["consignee"].ToString();
                        string customer = row["customer"].ToString();
                        string pn = row["partNo"].ToString();
                        int qty = Convert.ToInt32(row["qty"]);
                        if (needPrintConsignee.Contains(consignee))
                        {
                            PrintToCustomers(customer, qty, pn);
                        }
                    }
                }
            }
        }


        #region 针对某些报关客户，需多出一张标签
        private void PrintToCustomers(string customer, int qty, string pn)
        {
            var pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>
                {
                    { "PartNO", pn },
                    { "Qty", qty },
                    { "Brand", customer }
                };
            pm.TemplateFileName = "SB_NcartonLabel.btw";
            LabelPrintBase lpb = new LabelPrintBase(pm);
            var res = lpb.Print();
        }
        #endregion

        public IEnumerable<CommonSowModel> SowGrid
        {
            get { return (IEnumerable<CommonSowModel>)GetValue(SowDataSourceProperty); }
            set { SetValue(SowDataSourceProperty, value); }
        }

        public static readonly DependencyProperty SowDataSourceProperty =
            DependencyProperty.Register("SowGrid", typeof(IEnumerable<CommonSowModel>), typeof(WesCommonSow), new PropertyMetadata(null,
                (obj, e) =>
                {
                    WesCommonSow commonSow = obj as WesCommonSow;
                    commonSow.InitSowGrid();
                }));

        public string SelectedCartonId
        {
            get { return (string)GetValue(SelectedCartonIdProperty); }
            set { SetValue(SelectedCartonIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedCartonIdProperty =
            DependencyProperty.Register("SelectedCartonId", typeof(string), typeof(WesCommonSow), new PropertyMetadata(null,
                (obj, e) =>
                {
                    WesCommonSow wcs = obj as WesCommonSow;
                    wcs.selectedCartonId = wcs.SelectedCartonId;
                    if (wcs._sowDataSource == null)
                        return;
                    if (string.IsNullOrEmpty(wcs.SelectedCartonId)) //清空當前選中箱
                    {
                        wcs.BindNineData();//掃完一盤時，掃完箱號後，本箱應該從黃色變為綠色
                        return;
                    }
                    var selSource = wcs._sowDataSource.Where(a => a.NCartonNo == wcs.SelectedCartonId);
                    wcs.PageIndex = selSource.First().PageIndex;

                }));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(WesCommonSow), new PropertyMetadata(0));


        private void InitSowGrid()
        {
            if (this.SowGrid == null || !this.SowGrid.Any())
            {
                InitGrid();
                return;
            }
            _sowDataSource = this.SowGrid;
            PageIndex = _sowDataSource.First().PageIndex;
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        private int PageIndex
        {
            get { return pageIndex; }
            set
            {
                //循环切换
                if (value < 1)
                    pageIndex = TotalPage;
                else if (value > TotalPage)
                    pageIndex = 1;
                else
                    pageIndex = value;
                if (_sowDataSource != null && _sowDataSource.Any())
                {
                    _currDataSource = _sowDataSource.Where(a => a.PageIndex == pageIndex);
                    BindNineData(); //綁定九宮格數據
                }
            }
        }

        private void BindNineData()
        {
            SetNcartonColorByLoaingNo();//設置LoadingNo奇偶
            this.gridSow.Children.Clear();
            int index = 1;
            for (int i = 0; i < 9; i++)
            {
                int currGridNo = (pageIndex - 1) * pageSize + index;//單元格編號
                var lstGrid = _currDataSource.Where(a => a.GridNo == currGridNo);
                var viewModel = new GridSowViewModel();
                viewModel.Num = currGridNo;
                var gridSow = new WesGridSow();
                if (!lstGrid.Any() || lstGrid.First().NCartonNo == "")
                {
                    viewModel.Background = Brushes.Silver;
                    viewModel.NCartonNo = string.Empty;
                    viewModel.QtyDesc = string.Empty;
                    viewModel.NumFontSize = 64;
                    viewModel.NumForeground = Brushes.Black;
                    gridSow.DataContext = null;
                }
                else
                {
                    var model = lstGrid.First();
                    viewModel.NCartonNo = model.NCartonNo;
                    viewModel.QtyDesc = model.QtyDesc;
                    if (model.NCartonNo == selectedCartonId)
                    {
                        viewModel.Background = Brushes.Gold;
                        viewModel.NumForeground = Brushes.Blue;
                        viewModel.NumFontSize = 76;
                        SelectedIndex = currGridNo;
                    }
                    else
                    {
                        gridSow.Background = Brushes.DarkSeaGreen;
                        viewModel.NumFontSize = 64;
                        viewModel.NumForeground = Brushes.Black;
                        if (model.CheckFlag == "Y") //已满箱
                        {
                            viewModel.Background = Brushes.RoyalBlue;
                            viewModel.NumFontSize = 84;
                        }
                    }
                    if (model.Qty == model.TotalQty && model.NCartonNo != SelectedCartonId && model.CheckFlag != "N") //已满箱
                    {
                        viewModel.NumForeground = Brushes.Black;
                        viewModel.NumFontSize = 84;
                        viewModel.Background = Brushes.RoyalBlue;
                    }
                    var objLoading = _lstLoadingNoColor.Where(a => a.Item1.Equals(model.LoadingNo, StringComparison.OrdinalIgnoreCase));
                    if (objLoading.Any())
                    {
                        switch (objLoading.First().Item2)
                        {
                            case 0:
                                viewModel.NCartonForeground = Brushes.Red;//余0
                                break;
                            case 1:
                                viewModel.NCartonForeground = Brushes.Snow;//余1
                                break;
                            case 2:
                                viewModel.NCartonForeground = Brushes.SpringGreen;//余2
                                break;
                        }
                    }
                }
                gridSow.DataContext = viewModel;
                gridSow.BorderThickness = new Thickness(1, 1, 1, 1);
                gridSow.BorderBrush = new SolidColorBrush(Colors.Black);
                gridSow.Cell_DlbClick += GridSow_Cell_DlbClick;
                this.gridSow.Children.Add(gridSow);
                var x = i / 3;
                var y = i % 3;
                Grid.SetRow(gridSow, x);
                Grid.SetColumn(gridSow, y);
                index++;
            }
        }

        /// <summary>
        /// 設置LoadingNo奇偶
        /// </summary>
        private void SetNcartonColorByLoaingNo()
        {
            if (_sowDataSource == null || !_sowDataSource.Any())
                return;
            var lstLoadingNo = _sowDataSource.Select(a => a.LoadingNo).Distinct().ToList();
            for (int i = 0; i < lstLoadingNo.Count; i++)
            {
                _lstLoadingNoColor.Add(new Tuple<string, int>(lstLoadingNo[i], GetDividedNumYushu(i, 3)));
            }
        }

        private static int GetDividedNumYushu(int num, int n)
        {
            if (n == 0)
                return 0;
            return num % n;
        }

        /// <summary>
        /// 总页数
        /// </summary>
        private int TotalPage
        {
            get { return (TotalCount + pageSize - 1) / pageSize; }
        }

        /// <summary>
        /// 总条数
        /// </summary>
        private int TotalCount => _sowDataSource?.Count() ?? 1;

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            _startPoint = e.GetPosition(e.Source as FrameworkElement);
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _endPoint = e.GetPosition(e.Source as FrameworkElement);
            //X轴滑动的距离
            double offsetX = _startPoint.X - _endPoint.X;
            DoubleAnimation animation = null;
            if (offsetX > 6)
            {
                PageIndex++;
                animation = new DoubleAnimation(0, this.Width * 2, TimeSpan.FromMilliseconds(100));
            }
            else if (offsetX < -10)
            {
                PageIndex--;
                animation = new DoubleAnimation(0, -this.Width * 2, TimeSpan.FromMilliseconds(100));
            }
            ////动画
            if (animation != null)
            {
                animation.FillBehavior = FillBehavior.Stop;
                animation.DecelerationRatio = 0.2;
                animation.AccelerationRatio = 0.2;
                _translate.BeginAnimation(TranslateTransform.XProperty, animation);
            }
        }
    }
}
