using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wes.Customer.Avnet.Model
{
    public class SowModel
    {
        public SowModel()
        {
            LoadingNo = string.Empty;
            CartonNo = string.Empty;
            PartNo = string.Empty;
            FactoryPN = string.Empty;
            CustomerPN = string.Empty;
            OrderNo = string.Empty;
            NCartonNo = string.Empty;
            Qty = 0;
            TotalQty = 0;
            rowId = 0;
            PageIndex = 1;
            CheckFlag = "N";
            LotNos = string.Empty;
            DateCode = string.Empty;
            BinNo = string.Empty;
            MPQ = 0;
            Pans = 0;
            Consignee = string.Empty;
            Shipper = string.Empty;
            Opn = string.Empty;//PartNo
            Mpn = string.Empty;//FactoryPN
        }
        public int rowId { get; set; }
        public string OperationNo { get; set; }
        public string LoadingNo { get; set; }
        public string CartonNo { get; set; }
        public string PartNo { get; set; }
        /// <summary>
        /// 原廠料號
        /// </summary>
        public string FactoryPN { get; set; }
        public string CustomerPN { get; set; }
        public string OrderNo { get; set; }

        public string BinNo { get; set; }

        public string NCartonNo { get; set; }

        /// <summary>
        /// 最小包規
        /// </summary>
        public int MPQ { get; set; }
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

        public string LotNos { get; set; }

        public string DateCode { get; set; }
        public int Pans { get; set; }

        public string QtyDesc => Qty + "/" + TotalQty;

        public string Consignee { get; set; }

        public string Shipper { get; set; }
        public string Opn { get; set; }
        public string Mpn { get; set; }
    }
}
