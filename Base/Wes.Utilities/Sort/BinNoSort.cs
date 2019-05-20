using System.Collections.Generic;
using System.Data;

namespace Wes.Utilities.Sort
{
    public  static class BinNoSort
    {
        #region SortSetpLine

        /// <summary>
        ///     儲位排序
        /// </summary>
        /// <param name="_dt">要排序的数据表</param>
        /// <param name="_groupFields">线路的分组字段</param>
        /// <param name="_sortFieldName">线路排序字段</param>
        /// <param name="_orderModeByFieldName">排序依据的列(目前必须是数字)</param>
        /// <param name="_isOddOnAsc">奇数是否升序排序</param>
        /// <param name="_tailOrderBy">动态排序后，再排序规则</param>
        /// <returns></returns>
        public static DataTable SortSetpLine(DataTable _dt, string _groupFields, string _sortFieldName,
            string _orderModeByFieldName, bool _isOddOnAsc, string _tailOrderBy)
        {
            //get group list
            var _dir = new Dictionary<string, DataTable>();
            var _gpFlds = _groupFields.Split(',');
            foreach (DataRow _row in _dt.Rows)
            {
                var _key = "";
                foreach (var _fld in _gpFlds) _key = _key + "_" + _row[_fld];

                _key = _key.Substring(1);
                if (!_dir.ContainsKey(_key))
                {
                    var _dtGroup = _dt.Clone();
                    _dtGroup.ImportRow(_row);
                    _dir.Add(_key, _dtGroup);
                }
                else
                {
                    _dir[_key].ImportRow(_row);
                }
            }

            //sort data(dynamic)
            var _dtResult = _dt.Clone();
            foreach (var _key in _dir.Keys)
            {
                var _dtch = _dir[_key];
                var _dv = _dtch.DefaultView;

                var _orderValue = _dv[0].Row[_orderModeByFieldName].ToString();
                var _isOdd = int.Parse(_orderValue) % 2 == 1;
                var _sortName = _isOdd && _isOddOnAsc ? "ASC" : "DESC";

                var _orderBy = _sortFieldName + " " + _sortName;
                if (!string.IsNullOrEmpty(_tailOrderBy)) _orderBy = _orderBy + "," + _tailOrderBy;
                _dv.Sort = _orderBy;

                for (var i = 0; i < _dv.Count; i++) _dtResult.ImportRow(_dv[i].Row);
            }

            //return 
            return _dtResult;
        }

        #endregion

        #region SortTodoList

        /// <summary>
        ///  SortTodoList
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="packageId">当前箱需要按储位排序的列表</param>
        /// <returns></returns>
        public static DataTable SortTodoList(DataTable dt,string packageId)
        {
            if (!string.IsNullOrEmpty(packageId))
            {
                var temp = dt.Select("PackageID='" + packageId + "'");
                if (temp.Length == 0) return dt;
                var row = temp[0];
                var index = dt.Rows.IndexOf(row);
                if (index == 0) return dt;
                //var temRows = dt.AsEnumerable().Take(index);
                var _dtChild = dt.Clone();
                for (var i = 0; i < index; i++) _dtChild.ImportRow(dt.Rows[i]);

                for (var i = 0; i < index; i++) dt.Rows.RemoveAt(0);


                for (var i = 0; i < _dtChild.Rows.Count; i++) dt.ImportRow(_dtChild.Rows[i]);
            }

            return dt;
        }

        #endregion
    }
}
