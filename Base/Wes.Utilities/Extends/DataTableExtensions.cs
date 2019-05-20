using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;

namespace Wes.Utilities.Extends
{
    public static class DataTableExtensions
    {
        /// <summary>  
        /// 利用反射和泛型  
        /// </summary>  
        /// <param name="dt"></param>  
        /// <returns></returns>  
        public static ObservableCollection<T> ToObservableCollection<T>(this DataTable dt) where T : class, new()
        {
            ObservableCollection<T> ts = new ObservableCollection<T>();
            // 获得此模型的类型  
            System.Type type = typeof(T);
            //定义一个临时变量  
            string tempName = string.Empty;
            //遍历DataTable中所有的数据行  
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性  
                PropertyInfo[] propertys = t.GetType().GetProperties();
                //遍历该对象的所有属性  
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name; //将属性名称赋值给临时变量  
                    //检查DataTable是否包含此列（列名==对象的属性名）    
                    if (!dt.Columns.Contains(tempName)) continue;
                    // 判断此属性是否有Setter  
                    if (!pi.CanWrite) continue; //该属性不可写，直接跳出  
                    //取值  
                    object value = dr[tempName];
                    //如果非空，则赋给对象的属性  
                    if (value != DBNull.Value)
                        pi.SetValue(t, Convert.ChangeType(value, pi.PropertyType), null);
                }
                //对象添加到泛型集合中  
                ts.Add(t);
            }
            return ts;
        }

        public static List<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            if (dt == null)
            {
                return new List<T>();
            }
            List<T> ts = new List<T>();
            // 获得此模型的类型  
            System.Type type = typeof(T);
            //定义一个临时变量  
            string tempName = string.Empty;
            //遍历DataTable中所有的数据行  
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性  
                PropertyInfo[] propertys = t.GetType().GetProperties();
                //遍历该对象的所有属性  
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name; //将属性名称赋值给临时变量  
                    //检查DataTable是否包含此列（列名==对象的属性名）    
                    if (!dt.Columns.Contains(tempName)) continue;
                    // 判断此属性是否有Setter  
                    if (!pi.CanWrite) continue; //该属性不可写，直接跳出  
                    //取值  
                    object value = dr[tempName];
                    //如果非空，则赋给对象的属性  
                    if (value != DBNull.Value)
                        pi.SetValue(t, Convert.ChangeType(value, pi.PropertyType), null);
                }
                //对象添加到泛型集合中  
                ts.Add(t);
            }
            return ts;
        }

        public static T GetEntity<T>(this DataTable table) where T : class, new()
        {
            var entity = new T();
            foreach (DataRow row in table.Rows)
            {
                foreach (var item in entity.GetType().GetProperties())
                {
                    if (row.Table.Columns.Contains(item.Name))
                    {
                        if (DBNull.Value != row[item.Name])
                        {
                            item.SetValue(entity, Convert.ChangeType(row[item.Name], item.PropertyType), null);
                        }
                    }
                }
            }
            return entity;
        }

        public static DataTable ToDataTable(this IList list)
        {
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();

                foreach (PropertyInfo pi in propertys)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }
    }
}
