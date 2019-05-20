namespace Wes.Customer.Sinbon.Model
{
    public class PickModel
    {
        public PickModel()
        {
            BinNo = string.Empty;
            PackageID = string.Empty;
            PartNo = string.Empty;
            CLCode = string.Empty;
            Reels = 0;
            Qty = 0;
            Shipper = string.Empty;
            DateCode = string.Empty;
            IsOriginalCarton = string.Empty;
        }
        public string BinNo { get; set; }
        public string PackageID { get; set; }
        public string PartNo { get; set; }
        public string CLCode { get; set; }
        public int Reels { get; set; }
        public int Qty { get; set; }
        public string Shipper { get; set; }
        public string DateCode { get; set; }
        //整箱揀貨，但是整箱數量大於客戶能接受的數量，會整箱變為分箱
        public string IsOriginalCarton { get; set; }

    }
}
