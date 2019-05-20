namespace Wes.Customer.Sinbon.Model
{
    public class SowModel
    {
        public SowModel()
        {
            LoadingNo = string.Empty;
            CartonNo = string.Empty;
            PartNo = string.Empty;
            CLCode = string.Empty;
            NCartonNo = string.Empty;
            Qty = 0;
            RowID = 0;
            DateCode = string.Empty;
            OrderNo = string.Empty;
            Reels = 0;
            OperationNo = string.Empty;
            Shipper = string.Empty;
            MPQ = 1;
            IsAutoCombineCarton = "N";
        }
        public int RowID { get; set; }
        public string OperationNo { get; set; }

        public string LoadingNo { get; set; }
        public string CartonNo { get; set; }
        public string PartNo { get; set; }
        public string CLCode { get; set; }

        public string OrderNo { get; set; }

        public string NCartonNo { get; set; }

        public int Qty { get; set; }

        public string DateCode { get; set; }
        public int Reels { get; set; }

        public string Shipper { get; set; }

        public int MPQ { get; set; }

        /// <summary>
        /// 是否是自动并箱  Y：自动并箱  N：手动并箱
        /// </summary>
        public string IsAutoCombineCarton { get; set; }
    }
}
