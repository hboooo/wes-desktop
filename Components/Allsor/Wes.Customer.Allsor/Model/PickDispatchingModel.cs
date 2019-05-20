namespace Wes.Customer.Allsor.Model
{
    public class PickDispatchingModel
    {
        public PickDispatchingModel()
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
            DateCode = string.Empty;
            BinNo = string.Empty;
            Reels = 0;
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

        public string DateCode { get; set; }
        public int Reels { get; set; }
    }
}
