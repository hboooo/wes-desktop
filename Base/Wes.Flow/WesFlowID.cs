namespace Wes.Flow
{
    /// <summary>
    /// 物流倉儲流程
    /// </summary>
    public enum WesFlowID
    {
        UNKNOW = -1,

        #region 倉庫操作

        /// <summary>
        /// 掃描PXT
        /// </summary>
        FLOW_ACTION_SCAN_PICKING_NO = 0x0000001,

        /// <summary>
        /// 掃描TXT
        /// </summary>
        FLOW_ACTION_SCAN_TRUCK_NO = 0x0000002,

        /// <summary>
        /// 掃描MPN
        /// </summary>
        FLOW_ACTION_SCAN_MPN = 0x0000003,

        /// <summary>
        /// 掃描QRCode
        /// </summary>
        FLOW_ACTION_SCAN_QRCODE = 0x0000004,

        /// <summary>
        /// 掃描箱號
        /// </summary>
        FLOW_ACTION_SCAN_PACKAGE_ID = 0x0000005,

        /// <summary>
        /// 掃描儲位
        /// </summary>
        FLOW_ACTION_SCAN_STORAGE_SPAVES = 0x0000006,

        /// <summary>
        /// 錄入體積
        /// </summary>
        FLOW_ACTION_ENTRY_DIM = 0x0000007,

        /// <summary>
        /// 錄入大小
        /// </summary>
        FLOW_ACTION_ENTRY_GW = 0x0000008,

        /// <summary>
        /// 掃描QTY
        /// </summary>
        FLOW_ACTION_SCAN_QTY = 0x0000009,

        /// <summary>
        /// 掃描Nippon
        /// </summary>
        FLOW_ACTION_SCAN_NIPPON = 0x000000A,

        /// <summary>
        /// 掃描Receiving NO
        /// </summary>
        FLOW_ACTION_SCAN_RECEIVING_NO = 0x000000B,

        /// <summary>
        /// 输入total
        /// </summary>
        FLOW_ACTION_SCAN_TOTAL_RECEIVE = 0x000000C,

        /// <summary>
        /// 板号
        /// </summary>
        FLOW_ACTION_SCAN_PALLET_ID = 0x000000D,

        /// <summary>
        /// 料号.
        /// </summary>
        FLOW_ACTION_SCAN_PART_NO = 0x00000E,

        /// <summary>
        /// 原箱号
        /// </summary>
        FLOW_ACTION_SCAN_CARTON_NO = 0x00000F,

        /// <summary>
        /// 储位
        /// </summary>
        FLOW_ACTION_SCAN_BIN_NO = 0x000010,

        /// <summary>
        /// 新箱号
        /// </summary>
        FLOW_ACTION_SCAN_NEW_CARTON_NO = 0x0000011,

        /// <summary>
        /// 新箱号
        /// </summary>
        FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO = 0x0000012,

        /// <summary>
        /// Lot
        /// </summary>
        FLOW_ACTION_SCAN_LOT_NO = 0x0000013,

        /// <summary>
        /// DC
        /// </summary>
        FLOW_ACTION_SCAN_DC_NO = 0x0000014,

        /// <summary>
        /// 掃描LoadingNo
        /// </summary>
        FLOW_ACTION_SCAN_LOADING_NO = 0x0000015,

        /// <summary>
        /// 掃描PN/QRCode
        /// </summary>
        FLOW_ACTION_SCAN_PN_OR_QRCODE = 0x0000016,

        /// <summary>
        /// 掃描或录入COO
        /// </summary>
        FLOW_ACTION_SCAN_COO = 0x0000017,


        /// <summary>
        /// 掃描PN/CLCode/QRCode
        /// </summary>
        FLOW_ACTION_SCAN_PID_OR_PN_OR_CLCODE_OR_QRCODE = 0x0000018,

        /// <summary>
        /// 掃描PN/CLCode/QRCode
        /// </summary>
        FLOW_ACTION_SCAN_DC_AFTERQRCODE = 0x0000019,

        /// <summary>
        /// 扫描CLCODE
        /// </summary>
        FLOW_ACTION_SCAN_ClCode_NO = 0x000001A,

        /// <summary>
        /// 扫描SPN
        /// </summary>
        FLOW_ACTION_SCAN_SPN = 0x000001B,

        /// <summary>
        /// 扫描BRANCH
        /// </summary>
        FLOW_ACTION_SCAN_BRANCH = 0x000001D,

        /// <summary>
        /// 板号或箱号
        /// </summary>
        FLOW_ACTION_SCAN_PALLET_OR_CARTON = 0x000001E,

        /// <summary>
        /// 請輸入最小包規
        /// </summary>
        FLOW_ACTION_SCAN_MIN_QTY = 0x000001F,

        /// <summary>
        /// REEL START
        /// </summary>
        FLOW_ACTION_SCAN_REEL_START = 0x0000020,

        /// <summary>
        /// REEL END
        /// </summary>
        FLOW_ACTION_SCAN_REEL_END = 0x0000021,

        /// <summary>
        /// DATE 针对C03984
        /// </summary>
        FLOW_ACTION_SCAN_DATE = 0x0000022,

        /// <summary>
        /// 本地SPN, 不调用解析服务
        /// </summary>
        FLOW_ACTION_SCAN_LOCAL_SPN = 0x0000023,

       
        /// <summary>
        /// FW
        /// </summary>
        FLOW_ACTION_SCAN_FW = 0x0000025,
        /// <summary>
        /// 新的FW
        /// </summary>
        FLOW_ACTION_SCAN_FW_NEW = 0x0000027,


        /// <summary>
        /// 指令
        /// </summary>
        FLOW_ACTION_SCAN_COMMAND = 0x0000026,

        /// <summary>
        /// 扫描指令或者PN
        /// </summary>
        FLOW_ACTION_SCAN_COMMAND_OR_PN = 0x0000028,
        #endregion

        #region mask

        /// <summary>
        /// 擴展流程
        /// </summary>
        FLOW_EXTENDER_MUDULE = 0x0100,

        /// <summary>
        /// 空運
        /// </summary>
        FLOW_TRANSPORTATION = 0x0200,

        #endregion

        #region 入庫流程

        /// <summary>
        /// 入庫
        /// </summary>
        FLOW_IN = 0x0010000,

        /// <summary>
        /// 入庫擴展
        /// </summary>
        FLOW_IN_EXTENDER_MUDULE = FLOW_IN | FLOW_EXTENDER_MUDULE,

        /// <summary>
        /// 板轉箱
        /// </summary>
        FLOW_IN_PALLET_TO_CARTON = FLOW_IN | 0x0001,

        /// <summary>
        /// 採集
        /// </summary>
        FLOW_IN_GATHER = FLOW_IN | 0x0002,

        /// <summary>
        /// 上架
        /// </summary>
        FLOW_IN_SHELF = FLOW_IN | 0x0003,

        /// <summary>
        /// 采集异常
        /// </summary>
        FLOW_GATHER_ABNORMAL = FLOW_IN_EXTENDER_MUDULE | 0x01,

        /// <summary>
        /// 移倉採集
        /// </summary>
        FLOW_IN_STORAGE_MOVE_COLLECTING = FLOW_IN_EXTENDER_MUDULE | 0x02,

        #endregion

        #region 出庫流程

        /// <summary>
        /// 出库
        /// </summary>
        FLOW_OUT = 0x0020000,

        /// <summary>
        /// 出庫擴展
        /// </summary>
        FLOW_OUT_EXTENDER_MUDULE = FLOW_OUT | FLOW_EXTENDER_MUDULE,

        /// <summary>
        /// 空運出庫
        /// </summary>
        FLOW_OUT_TRANSPORTATION = FLOW_OUT | FLOW_TRANSPORTATION,

        /// <summary>
        /// 拣货
        /// </summary>
        FLOW_OUT_PICKING = FLOW_OUT | 0x0001,

        /// <summary>
        /// 分撥
        /// </summary>
        FLOW_OUT_SOW = FLOW_OUT | 0x0002,

        /// <summary>
        /// 貼標
        /// </summary>
        FLOW_OUT_LABELING = FLOW_OUT | 0x0003,

        /// <summary>
        /// 貼箱標
        /// </summary>
        FLOW_OUT_CARTON_LABELING = FLOW_OUT | 0x0004,

        /// <summary>
        /// 打板
        /// </summary>
        FLOW_OUT_BOARDING = FLOW_OUT | 0x0005,

        /// <summary>
        /// 拣并合一
        /// </summary>
        FLOW_OUT_PICKING_AND_SOW = FLOW_OUT | 0x0006,

        /// <summary>
        /// 箱号查询
        /// </summary>
        FLOW_OUT_PACKAGEIDINFO_REPORT = FLOW_OUT_EXTENDER_MUDULE | 0x01,

        /// <summary>
        /// 空運貼標
        /// </summary>
        FLOW_OUT_TRANSPORTATION_LABELING = FLOW_OUT_TRANSPORTATION | 0x01,

        /// <summary>
        /// 拣货查询
        /// </summary>
        FLOW_OUT_PICKEDINFO_REPORT = FLOW_OUT_EXTENDER_MUDULE | 0x02,

        #endregion
    }
}