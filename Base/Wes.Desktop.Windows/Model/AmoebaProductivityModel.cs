using System;

namespace Wes.Desktop.Windows.Model
{
    public class AmoebaProductivityModel
    {
        public string OperationNo { get; set; }
        public DateTime OperationDate { get; set; }
        /// <summary>
        /// Receiving,Shipping,Others
        /// </summary>
        public string OperationType { get; set; }
        public string ActionType { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public string DifficultyGrade { get; set; }
        /// <summary>
        /// 权重
        /// </summary>
        public decimal DifficultyWeight { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        public decimal DifficultyTimes { get; set; }
        public string PackageId { get; set; }
        public decimal Qty { get; set; }
        /// <summary>
        /// 产量
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// 产值
        /// </summary>
        public decimal Productivity { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public string DeviceNo { get; set; }
        /// <summary>
        /// Mac地址
        /// </summary>
        public string MacAddress { get; set; }

        public string EndCustomer { get; set; }
        public string TargetType { get; set; }
        public string Target { get; set; }
        public string VersionNo { get; set; }

        public int IdleTime { get; set; }


    }
}
