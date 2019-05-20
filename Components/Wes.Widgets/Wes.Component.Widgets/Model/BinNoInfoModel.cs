namespace Wes.Component.Widgets.Model
{
    public class BinNoInfoModel
    {
        public BinNoInfoModel()
        {
            ScanBy = string.Empty;
            From = string.Empty;
            To = string.Empty;
            ScanOn = string.Empty;
        }
        public string ScanBy { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string ScanOn { get; set; }

    }
}
