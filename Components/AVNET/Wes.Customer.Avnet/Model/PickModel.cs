namespace Wes.Customer.Avnet.Model
{
    public class PickModel
    {
        public PickModel()
        {
            BinNo = string.Empty;
            PackageID = string.Empty;
            PartNo = string.Empty;
            FactoryPN = string.Empty;
            Reels = 0;
            Qty = 0;
            Shipper = string.Empty;
            DataCode = string.Empty;
            LotNos = string.Empty;
        }
        public string BinNo { get; set; }
        public string PackageID { get; set; }
        public string PartNo { get; set; }
        public string LotNos { get; set; }

        /// <summary>
        /// 原廠料號
        /// </summary>
        public string FactoryPN { get; set; }
        public int Reels { get; set; }
        public int Qty { get; set; }
        public string Shipper { get; set; }
        public string DataCode { get; set; }

    }
}
