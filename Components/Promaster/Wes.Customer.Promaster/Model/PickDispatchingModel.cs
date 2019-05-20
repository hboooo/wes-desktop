namespace Wes.Customer.Promaster.Model
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
            LotNo = string.Empty;
            BatchNo = string.Empty;
            DateCode = string.Empty;
            BinNo = string.Empty;
            OperationNo = string.Empty;
            Shipper = string.Empty;
            ShipperName = string.Empty;
            NeedScanFW = "N";
            CombineNo = string.Empty;
            OriginCountry = "";
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

        public string LotNo { get; set; }

        public string BatchNo { get; set; }

        public string DateCode { get; set; }

        public string Shipper { get; set; }

        public string ShipperName { get; set; }

        //Shipper 為C04460，根據基礎料號配置判斷是否需要掃FW
        public string NeedScanFW { get; set; }

        //出貨明細2 主鍵
        public int ItemNo { get; set; }

        //并箱序號
        public string CombineNo { get; set; }
        
        public string OriginCountry { get; set; }
    }
}