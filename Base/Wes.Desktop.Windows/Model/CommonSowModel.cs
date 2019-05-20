namespace Wes.Desktop.Windows.Model
{
    public class CommonSowModel
    {
        public CommonSowModel()
        {
            LoadingNo = string.Empty;
            CartonNo = string.Empty;
            PartNo = string.Empty;
            CustomerPN = string.Empty;
            OrderNo = string.Empty;
            NCartonNo = string.Empty;
            Qty = 0;
            TotalQty = 0;
            RowID = 0;
            PageIndex = 1;
            CheckFlag = "";
            LotNo = string.Empty;
            DateCode = string.Empty;
            Brand = string.Empty;
            PartNoName = string.Empty;
            BinNo = string.Empty;
            Pans = 0;
            OperationNo = string.Empty;
        }

        public int RowID { get; set; }
        public string OperationNo { get; set; }
        public string LoadingNo { get; set; }
        public string CartonNo { get; set; }
        public string PartNo { get; set; }
        public string CustomerPN { get; set; }
        public string OrderNo { get; set; }

        public string BinNo { get; set; }

        public string NCartonNo { get; set; }
        public int Qty { get; set; }

        public int TotalQty { get; set; }
        /// <summary>
        /// 九宮格編號
        /// </summary>
        public int GridNo { get; set; }

        /// <summary>
        /// 分頁索引
        /// </summary>
        public int PageIndex { get; set; }

        //是否裝滿 Y:是 N:否
        public string CheckFlag { get; set; }

        public string LotNo { get; set; }

        public string DateCode { get; set; }
        public string Brand { get; set; }//品牌名稱
        public string PartNoName { get; set; }//產品名稱
        public int Pans { get; set; }


        public string QtyDesc
        {
            get { return Qty + "/" + TotalQty; }
        }
       
    }
}
