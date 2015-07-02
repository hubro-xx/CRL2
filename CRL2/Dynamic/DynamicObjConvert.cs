using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRL.Dynamic
{
    internal class DynamicObjConvert
    {
        static dynamic getDataRow(DataRow dr)
        {
            dynamic obj = new System.Dynamic.ExpandoObject();
            var dict = obj as IDictionary<string, object>;
            foreach (DataColumn col in dr.Table.Columns)
            {
                dict.Add(col.ColumnName, dr[col.ColumnName]);
            }
            return obj;
        }
        public static IEnumerable<dynamic> _DataTableToDynamic(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                var d = getDataRow(row);
                yield return d;
            }
        }
        static dynamic getDataRow(System.Data.Common.DbDataReader reader)
        {
            dynamic obj = new System.Dynamic.ExpandoObject();
            var dict = obj as IDictionary<string, object>;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                object value = reader[columnName];
                dict.Add(columnName, value);
            }
            return obj;
        }
        public static List<dynamic> DataReaderToDynamic(System.Data.Common.DbDataReader reader)
        {
            List<dynamic> list = new List<dynamic>();
            while (reader.Read())
            {
                var d = getDataRow(reader);
                list.Add(d);
            }
            reader.Close();
            return list;
        }
        public static List<TResult> DataReaderToDynamic<T, TResult>(System.Data.Common.DbDataReader reader, Expression<Func<T, TResult>> resultSelector, ParameCollection fieldMapping) where T : IModel, new()
        {
            List<TResult> list = new List<TResult>();
            var typeArry = TypeCache.GetProperties(typeof(T), true).Values;
            while (reader.Read())
            {
                var detailItem = ObjectConvert.DataReaderToObj(reader, typeof(T), typeArry, fieldMapping) as T;
                var result = resultSelector.Compile()(detailItem);
                list.Add(result);
            }
            reader.Close();
            return list;
        } 
    }
}
