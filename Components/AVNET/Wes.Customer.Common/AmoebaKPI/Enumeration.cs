using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wes.Customer.Common
{
    public enum EnumListeningActionId : long
    {
        #region Avnet
        AvnetPickingInitId = 6412901264241205248,
        AvnetPickingAddKpiId = 6413638872684568576,
        AvnetSowInitId = 6415129929175797760,
        AvnetSowAddKpiId = 6415139884503146496,
        AvnetPickDispatchingInitId = 6471610973743484928,
        AvnetPickDispatchingAddKpiId = 6471620061349023744,
        #endregion

        #region 组板
        AvnetShippingPalletEndAddKpiId = 6432782857596309504,
        AvnetShippingCartonEndAddKpiId = 6432783148219637760,
        AvnetShippingDeletePalletAddKpiId = 6420101420141256704,
        AvnetShippingDeleteCartonAddKpiId = 6437537634364428288,
        #endregion

        #region 板转箱

        AvnetPallet2CartonInitId = 6415479801422815232,
        AvnetPallet2CartonAddKpiId = 6415780069146370048,
        AvnetPallet2CartonDelAddKpiId = 6432917738397634560,
        #endregion

        #region 贴箱标

        AvnetCartonLabelingInitId = 6418283844952133632,
        AvnetCartonLabelingAddKpiId = 6418356205290266624,
        AvnetCartonLabelingAddKpiId2 = 6418359187801120768,
        #endregion

        #region 采集
        AvnetCollectingStartInitId = 6402348645168979969,
        AvnetCollectingScanSaveAddKpiId = 6402348645168979970,
        AvnetCollectingCartonEndAddKpiId = 6438317547052863488,
        #endregion

        #region 贴标
        AvnetLabelingStartInitId = 6421990780402929664,
        AvnetLabelingSaveAddKpiId = 6433231853867503616,
        AvnetLabelingBoxEndAddKpiId = 6422073996916826112,
        AvnetLabelingCartonEndAddKpiId = 6437992870661070848,
        #endregion
    }
}
