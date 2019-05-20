namespace Wes.Customer.FitiPower.API
{
    public enum ScriptSid : long
    {
        /// <summary>
        /// name: QrCodeDispatch
        /// endCustomer: C00339
        /// fileName: QrcodeDispatch.groovy
        /// fileDir: avnet/resolver/supplier
        /// </summary>
        QR_CODE_DISPATCH = 6402348645168975872,

        /// <summary>
        /// name: DateCodeDispatch
        /// endCustomer: C00339
        /// fileName: DateCodeDispatch.groovy
        /// fileDir: avnet/resolver/supplier
        /// </summary>
        DATE_CODE_DISPATCH = 6402348645168979968,

        /// <summary>
        /// name: DecodeDispatch
        /// endCustomer: C00339
        /// fileName: DecodeDispatch.groovy
        /// fileDir: avnet/resolver/supplier
        /// </summary>
        DECODE_DISPATCH = 6402348645173174282,

        /// <summary>
        /// name: AvnetCheckDate
        /// endCustomer: C00339
        /// fileName: CheckDate.groovy
        /// fileDir: avnet/common
        /// </summary>
        CHECK_DATE = 6413568981646446592,

        /// <summary>
        /// name: AvnetSmallCargoShippingWorkProgress
        /// endCustomer: C00339
        /// fileName: SmallCargoShippingWorkProgress.groovy
        /// fileDir: avnet/common
        /// </summary>
        SMALL_CARGO_SHIPPING_WORK_PROGRESS = 6413610042498486272,

        /// <summary>
        /// name: AvnetSmallCargoShippingWorkingProgressbyTXT
        /// endCustomer: C00339
        /// fileName: SmallCargoShippingWorkingProgressbyTXT.groovy
        /// fileDir: avnet/common
        /// </summary>
        SMALL_CARGO_SHIPPING_WORKING_PROGRESSBY_TXT = 6414738370425004032,

        /// <summary>
        /// name: AvnetSmallCargoShippingWorkingProgressbyTXTList
        /// endCustomer: C00339
        /// fileName: SmallCargoShippingWorkingProgressbyTXTList.groovy
        /// fileDir: avnet/common
        /// </summary>
        SMALL_CARGO_SHIPPING_WORKING_PROGRESSBY_TXTLIST = 6415149574805000192,

        /// <summary>
        /// name: AvnetGetYushuQty
        /// endCustomer: C00339
        /// fileName: GetYushuQty.groovy
        /// fileDir: avnet/wes
        /// </summary>
        GET_YUSHU_QTY = 6417687321541349376,

        /// <summary>
        /// name: AvnetGetSplitCarton
        /// endCustomer: C00339
        /// fileName: GetSplitCarton.groovy
        /// fileDir: avnet/common
        /// </summary>
        GET_SPLIT_CARTON = 6418289849517744128,

        /// <summary>
        /// name: AvnetOperationLog
        /// endCustomer: C00339
        /// fileName: OperationLog.groovy
        /// fileDir: avnet/common
        /// </summary>
        OPERATION_LOG = 6418404698088280064,

        /// <summary>
        /// name: AvnetDimension
        /// endCustomer: C00339
        /// fileName: Dimension.groovy
        /// fileDir: avnet/resolver/common
        /// </summary>
        DIMENSION = 6420095813136949248,

        /// <summary>
        /// name: AvnetGetDifficultyGradeMapping
        /// endCustomer: C00339
        /// fileName: GetDifficultyGradeMapping.groovy
        /// fileDir: avnet/wms/kpi
        /// </summary>
        GET_DIFFICULTY_GRADE_MAPPING = 6422269716739395584,

        /// <summary>
        /// name: AvnetGetDifficultyGrade
        /// endCustomer: C00339
        /// fileName: GetDifficultyGrade.groovy
        /// fileDir: avnet/wms/kpi
        /// </summary>
        GET_DIFFICULTY_GRADE = 6422270866477817856,

        /// <summary>
        /// name: AvnetGetIdleTime
        /// endCustomer: C00339
        /// fileName: GetIdleTime.groovy
        /// fileDir: avnet/wms/kpi
        /// </summary>
        GET_IDLE_TIME = 6422283473431564288,

        /// <summary>
        /// name: AvnetInsertProductInfo
        /// endCustomer: C00339
        /// fileName: InsertProductInfo.groovy
        /// fileDir: avnet/wms/kpi
        /// </summary>
        INSERT_PRODUCT_INFO = 6422286720447815680,

        /// <summary>
        /// name: AvnetKpiCaching
        /// endCustomer: C00339
        /// fileName: KpiCaching.groovy
        /// fileDir: avnet/wms/kpi
        /// </summary>
        KPI_CACHING = 6422660222543732736,

        /// <summary>
        /// name: AvnetKpiAmoebaDifficultyGrade
        /// endCustomer: C00339
        /// fileName: KpiAmoebaDifficultyGrade.groovy
        /// fileDir: avnet/wms/kpi
        /// </summary>
        KPI_AMOEBA_DIFFICULTY_GRADE = 6422697081516855296,

        /// <summary>
        /// name: AvnetReturnDaysBetween
        /// endCustomer: C00339
        /// fileName: ReturnDaysBetween.groovy
        /// fileDir: avnet/wms/common
        /// </summary>
        RETURN_DAYS_BETWEEN = 6428184431860654080,

        /// <summary>
        /// name: AvnetDeleteKpi
        /// endCustomer: C00339
        /// fileName: DeleteKpi.groovy
        /// fileDir: avnet/wms/kpi
        /// </summary>
        DELETE_KPI = 6432872283785666560,

        /// <summary>
        /// name: FitipowerCollectingPackage
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingPackage.groovy
        /// fileDir: fitipower/wes/collecting
        /// </summary>
        COLLECTING_PACKAGE = 6442235980719067136,

        /// <summary>
        /// name: FitipowerCheckPxt
        /// endCustomer: C03707
        /// fileName: FitipowerCheckPxt.groovy
        /// fileDir: fitipower/wes/pickAndSow
        /// </summary>
        CHECK_PXT = 6442274169567580160,

        /// <summary>
        /// name: FitipowerLabelingTruckOrder
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingTruckOrder.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_TRUCK_ORDER = 6442283811676889088,

        /// <summary>
        /// name: FitipowerLabelingPackage
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingPackage.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_PACKAGE = 6442301157569204224,

        /// <summary>
        /// name: FitipowerCollectingSave
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingSave.groovy
        /// fileDir: fitipower/wes/collecting
        /// </summary>
        COLLECTING_SAVE = 6442332641147494400,

        /// <summary>
        /// name: FitipowerCollectingDelete
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingDelete.groovy
        /// fileDir: fitipower/wes/collecting
        /// </summary>
        COLLECTING_DELETE = 6442340385363599360,

        /// <summary>
        /// name: FitipowerLabelingSave
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingSave.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_SAVE = 6442559623227514880,

        LABELING_REEL_END = 6479302770032640,

        /// <summary>
        /// name: FitipowerLabelingDelete
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingDelete.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_DELETE = 6442560403242229760,

        /// <summary>
        /// name: FitipowerCollectingCartonEnd
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingCartonEnd.groovy
        /// fileDir: fitipower/wes/collecting
        /// </summary>
        COLLECTING_CARTON_END = 6442560877324406784,

        /// <summary>
        /// name: FitiPowerGetLabelInfo
        /// endCustomer: C03707
        /// fileName: FitiPowerGetLabelInfo.groovy
        /// fileDir: fitipower/wes/cartonLabel
        /// </summary>
        GET_LABEL_INFO = 6442622111205822464,

        /// <summary>
        /// name: FitiPowerPrintCartonLabel
        /// endCustomer: C03707
        /// fileName: FitiPowerPrintCartonLabel.groovy
        /// fileDir: fitipower/wes/cartonLabel
        /// </summary>
        PRINT_CARTON_LABEL = 6442622138338779136,

        /// <summary>
        /// name: FitiPowerPrintCartonLabelLog
        /// endCustomer: C03707
        /// fileName: FitiPowerPrintCartonLabelLog.groovy
        /// fileDir: fitipower/wes/cartonLabel
        /// </summary>
        PRINT_CARTON_LABEL_LOG = 6442622161445195776,

        /// <summary>
        /// name: FitiPowerUpdateInfo
        /// endCustomer: C03707
        /// fileName: FitiPowerUpdateInfo.groovy
        /// fileDir: fitipower/wes/cartonLabel
        /// </summary>
        UPDATE_INFO = 6442622176607608832,

        /// <summary>
        /// name: FitiPowerCheckPalletId
        /// endCustomer: C03707
        /// fileName: FitiPowerCheckPalletId.groovy
        /// fileDir: fitipower/wes/convert
        /// </summary>
        CHECK_PALLET_ID = 6442622192722120704,

        /// <summary>
        /// name: FitiPowerCheckReceivingNo
        /// endCustomer: C03707
        /// fileName: FitiPowerCheckReceivingNo.groovy
        /// fileDir: fitipower/wes/convert
        /// </summary>
        CHECK_RECEIVING_NO = 6442622209390288896,

        /// <summary>
        /// name: FitiPowerDeleteCartonNo
        /// endCustomer: C03707
        /// fileName: FitiPowerDeleteCartonNo.groovy
        /// fileDir: fitipower/wes/convert
        /// </summary>
        DELETE_CARTON_NO = 6442622226930864128,

        /// <summary>
        /// name: FitiPowerGenerateCartonList
        /// endCustomer: C03707
        /// fileName: FitiPowerGenerateCartonList.groovy
        /// fileDir: fitipower/wes/convert
        /// </summary>
        GENERATE_CARTON_LIST = 6442622250259587072,

        /// <summary>
        /// name: FitiPowerGetCartonList
        /// endCustomer: C03707
        /// fileName: FitiPowerGetCartonList.groovy
        /// fileDir: fitipower/wes/convert
        /// </summary>
        GET_CARTON_LIST = 6442622265732370432,

        /// <summary>
        /// name: FitiPowerReceivingEnd
        /// endCustomer: C03707
        /// fileName: FitiPowerReceivingEnd.groovy
        /// fileDir: fitipower/wes/convert
        /// </summary>
        RECEIVING_END = 6442622281213550592,

        /// <summary>
        /// name: FitipowerGetInfoByPxt
        /// endCustomer: C03707
        /// fileName: FitipowerGetInfoByPxt.groovy
        /// fileDir: fitipower/wes/pickAndSow
        /// </summary>
        GET_INFO_BY_PXT = 6442626488146599936,

        /// <summary>
        /// name: FitipowerSaveData
        /// endCustomer: C03707
        /// fileName: FitipowerSaveData.groovy
        /// fileDir: fitipower/wes/pickAndSow
        /// </summary>
        SAVE_DATA = 6442626957623431168,

        /// <summary>
        /// name: FitipowerDeleteData
        /// endCustomer: C03707
        /// fileName: FitipowerDeleteData.groovy
        /// fileDir: fitipower/wes/pickAndSow
        /// </summary>
        DELETE_DATA = 6442627009750245376,

        /// <summary>
        /// name: FitipowerAddPalletSize
        /// endCustomer: C03707
        /// fileName: FitipowerAddPalletSize.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        ADD_PALLET_SIZE = 6442685180829245440,

        /// <summary>
        /// name: FitipowerCheckPackageLabeled
        /// endCustomer: C03707
        /// fileName: FitipowerCheckPackageLabeled.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        CHECK_PACKAGE_LABELED = 6442685362270638080,

        /// <summary>
        /// name: FitipowerCheckShippingPackageState
        /// endCustomer: C03707
        /// fileName: FitipowerCheckShippingPackageState.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        CHECK_SHIPPING_PACKAGE_STATE = 6442685393488846848,

        /// <summary>
        /// name: FitipowerDeleteCartons
        /// endCustomer: C03707
        /// fileName: FitipowerDeleteCartons.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        DELETE_CARTONS = 6442685421993332736,

        /// <summary>
        /// name: FitipowerDeletePallets
        /// endCustomer: C03707
        /// fileName: FitipowerDeletePallets.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        DELETE_PALLETS = 6442685450439106560,

        /// <summary>
        /// name: FitipowerGetCartonLabel
        /// endCustomer: C03707
        /// fileName: FitipowerGetCartonLabel.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        GET_CARTON_LABEL = 6442685476179546112,

        /// <summary>
        /// name: FitipowerGetConsigneeInfo
        /// endCustomer: C03707
        /// fileName: FitipowerGetConsigneeInfo.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        GET_CONSIGNEE_INFO = 6442686625364320256,

        /// <summary>
        /// name: FitipowerGetErrorScan
        /// endCustomer: C03707
        /// fileName: FitipowerGetErrorScan.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        GET_ERROR_SCAN = 6442686658985857024,

        /// <summary>
        /// name: FitipowerGetOperatorImageList
        /// endCustomer: C03707
        /// fileName: FitipowerGetOperatorImageList.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        GET_OPERATOR_IMAGE_LIST = 6442686686584381440,

        /// <summary>
        /// name: FitipowerGetPalletLabel
        /// endCustomer: C03707
        /// fileName: FitipowerGetPalletLabel.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        GET_PALLET_LABEL = 6442686716527513600,

        /// <summary>
        /// name: FitipowerGetTruckPackages
        /// endCustomer: C03707
        /// fileName: FitipowerGetTruckPackages.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        GET_TRUCK_PACKAGES = 6442686749339557888,

        /// <summary>
        /// name: FitipowerPickingConfirm
        /// endCustomer: C03707
        /// fileName: FitipowerPickingConfirm.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        PICKING_CONFIRM = 6442686779538542592,

        /// <summary>
        /// name: FitipowerSaveErrorScan
        /// endCustomer: C03707
        /// fileName: FitipowerSaveErrorScan.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        SAVE_ERROR_SCAN = 6442686807518748672,

        /// <summary>
        /// name: FitipowerUpdateCarton
        /// endCustomer: C03707
        /// fileName: FitipowerUpdateCarton.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        UPDATE_CARTON = 6442686836778209280,

        /// <summary>
        /// name: FitipowerUpdatePalletInfo
        /// endCustomer: C03707
        /// fileName: FitipowerUpdatePalletInfo.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        UPDATE_PALLET_INFO = 6442686862002757632,

        /// <summary>
        /// name: FitipowerUpdatePallets
        /// endCustomer: C03707
        /// fileName: FitipowerUpdatePallets.groovy
        /// fileDir: fitipower/wes/shipping
        /// </summary>
        UPDATE_PALLETS = 6442686886640095232,

        /// <summary>
        /// name: FitipowerLabelingTemplate
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingTemplate.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_TEMPLATE = 6442707098785550336,

        /// <summary>
        /// name: FitipowerLabelingBoxEnd
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingBoxEnd.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_BOX_END = 6442707525681815552,

        /// <summary>
        /// name: FitipowerLabelingCartonEnd
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingCartonEnd.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_CARTON_END = 6442707946085294080,

        /// <summary>
        /// name: FitipowerLabelingReprint
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingReprint.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_REPRINT = 6442955126696058880,

        /// <summary>
        /// name: FitipowerCollectingException
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingException.groovy
        /// fileDir: fitipower/wes/collecting
        /// </summary>
        COLLECTING_EXCEPTION = 6442988160094834688,

        /// <summary>
        /// name: FitipowerPartRules
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingPartRules.groovy
        /// fileDir: fitipower/wes/collecting
        /// </summary>
        PART_RULES = 6443012840256184320,

        /// <summary>
        /// name: FitipowerCollectingHelper
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingHelper.groovy
        /// fileDir: fitipower/wes/collecting
        /// </summary>
        COLLECTING_HELPER = 6443018815704932352,

        /// <summary>
        /// name: FitipowerCollectingIsComplete
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingIsComplete.groovy
        /// fileDir: fitipower/wes/Collecting
        /// </summary>
        COLLECTING_IS_COMPLETE = 6443020831114137600,

        /// <summary>
        /// name: FitipowerCollectingPalletEnd
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingPalletEnd.groovy
        /// fileDir: fitipower/wes/Collecting
        /// </summary>
        COLLECTING_PALLET_END = 6443021639171973120,

        /// <summary>
        /// name: FitipowerCollectingRcvEnd
        /// endCustomer: C03707
        /// fileName: FitipowerCollectingRcvEnd.groovy
        /// fileDir: fitipower/wes/Collecting
        /// </summary>
        COLLECTING_RCV_END = 6443022350991495168,

        /// <summary>
        /// name: FitipowerLabelingLoadingNo
        /// endCustomer: C03707
        /// fileName: FitipowerLabelingLoadingNo.groovy
        /// fileDir: fitipower/wes/labeling
        /// </summary>
        LABELING_LOADING_NO = 6443657672322453504,

    }
}
