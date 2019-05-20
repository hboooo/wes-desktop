namespace Wes.Desktop.Windows.Model
{
    public class CommonUpdateBinNoModel
    {
        public CommonUpdateBinNoModel()
        {
            ActionType = string.Empty;
            OperationNo = string.Empty;
            LoadingNo = string.Empty;
            PackageId = string.Empty;
            BinNo = string.Empty;
            YushuQty = string.Empty;
            UpdateUser = string.Empty;
        }

        public string ActionType { get; set; }
        public string OperationNo { get; set; }
        public string LoadingNo { get; set; }
        public string PackageId { get; set; }
        public string BinNo { get; set; }
        public string YushuQty { get; set; }
        public string UpdateUser { get; set; }
    }
}
