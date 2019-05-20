using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wes.Flow
{
    /// <summary>
    /// KPI Action
    /// </summary>
    [Flags]
    public enum KPIActionType : ulong
    {
        Null,
        #region 收货
        /// <summary>
        /// 收货
        /// </summary>
        Receiving = 0xF000000000000000,
        /// <summary>
        /// 採集板轉箱
        /// </summary>
        LSReceivingLabelling = Receiving | 1,
        /// <summary>
        /// 採集板轉箱Plus
        /// </summary>
        LSReceivingLabellingPlus = Receiving | 2,
        /// <summary>
        /// 收貨採集
        /// </summary>
        LSDataCollection = Receiving | 4,
        /// <summary>
        /// 收貨採集--附加，以箱计算
        /// </summary>
        LSDataCollectionPlus = Receiving | 8,
        #endregion

        #region 出货
        /// <summary>
        /// 出货
        /// </summary>
        Shipping = 0xE000000000000000,
        /// <summary>
        /// 出貨工站一：揀貨分貨
        /// </summary>
        LSPickingSplitCarton = Shipping | 1,
        /// <summary>
        /// 出貨工站一：揀貨分貨附加，以箱計算
        /// </summary>
        LSPickingSplitCartonPlus = Shipping | 2,
        /// <summary>
        /// 出貨工站二：分貨并箱
        /// </summary>
        LSPacking = Shipping | 4,
        /// <summary>
        /// 出貨工站二：分貨并箱附加，以箱計算
        /// </summary>
        LSPackingPlus = Shipping | 8,
        /// <summary>
        /// 出貨工站三：出貨貼標
        /// </summary>
        LSLabeling = Shipping | 16,
        /// <summary>
        /// 出貨工站三：出貨貼標附加,以箱計算
        /// </summary>
        LSLabelingPlus = Shipping | 32,
        /// <summary>
        /// 貼標檢驗
        /// </summary>
        LSChecking = Shipping | 64,
        /// <summary>
        /// 裝箱貼標
        /// </summary>
        LSCartonLabeling = Shipping | 128,
        /// <summary>
        /// 裝箱貼標
        /// </summary>
        LSCartonLabelingPlus = Shipping | 256,
        /// <summary>
        /// 揀貨組板
        /// </summary>
        LSCombinePallet = Shipping | 512,
        /// <summary>
        /// 揀貨組板Plus
        /// </summary>
        LSCombinePalletPlus = Shipping | 1024,
        /// <summary>
        /// 揀貨組箱
        /// </summary>
        LSCombineCarton = Shipping | 2048,
        /// <summary>
        /// 揀貨組箱Plus
        /// </summary>
        LSCombineCartonPlus = Shipping | 4096,
        /// <summary>
        /// 空运贴标
        /// </summary>
        PDAAirLabel = Shipping | 8192,
        #endregion
    }

    /// <summary>
    /// KPI Operation
    /// </summary>
    public enum KPIOperationType
    {
        Receiving,
        Shipping,
        Others
    }
}
