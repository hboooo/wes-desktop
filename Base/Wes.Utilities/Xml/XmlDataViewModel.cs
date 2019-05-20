namespace Wes.Desktop.Windows.ViewModel
{
    public class XmlDataViewModel
    {
        private string _time;
        /// <summary>
        /// 时间
        /// </summary>
        public string Time
        {
            get { return _time; }
            set
            {
                _time = value;

            }
        }

        private string _value;
        /// <summary>
        /// 历史记录
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
            }
        }

        private string _flowID;
        /// <summary>
        /// 类型
        /// </summary>
        public string FlowID
        {
            get { return _flowID; }
            set
            {
                _flowID = value;
            }
        }
    }
}
