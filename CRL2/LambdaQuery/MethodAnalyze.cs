using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRL.LambdaQuery
{
    internal delegate string MethodHandler(string field, ref int parIndex, AddParameHandler addParame, params object[] args);
    internal delegate void AddParameHandler(string name, object value);
    internal class MethodAnalyze<T> where T : IModel, new()
    {
        DBAdapter.DBAdapterBase dBAdapter;
        public MethodAnalyze()
        {
            var table = TypeCache.GetTable(typeof(T));
            dBAdapter = table.DBAdapter;
            if (dBAdapter == null)
            {
                throw new Exception("dBAdapter尚未初始化");
            }
        }
        public string Substring(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            return dBAdapter.SubstringFormat(field, (int)args[0], (int)args[1]);
            //return string.Format(" SUBSTRING({0},{1},{2})", field, args[0], args[1]);
        }
        public string StringLike(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string parName = string.Format("@like{0}", parIndex);
            addParame(parName, args[0]);
            return dBAdapter.StringLikeFormat(field, parName);
            //return string.Format("{0} LIKE {1}", field, parName);
        }
        public string StringNotLike(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string parName = string.Format("@like{0}", parIndex);
            addParame(parName, args[0]);
            return dBAdapter.StringNotLikeFormat(field, parName);
            //return string.Format("{0} not LIKE {1}", field, parName);
        }
        public string StringContains(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string parName = string.Format("@contrains{0}", parIndex);
            addParame(parName, args[0]);
            return dBAdapter.StringContainsFormat(field, parName);
            //return string.Format("CHARINDEX({1},{0})>0", field, parName);
        }
        public string DateTimeBetween(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string parName = string.Format("@between{0}", parIndex);
            addParame(parName, args[0]);
            parIndex += 1;
            string parName2 = string.Format("@between{0}", parIndex);
            addParame(parName2, args[1]);
            return dBAdapter.BetweenFormat(field, parName, parName2);
            //return string.Format("{0} between {1} and {2}", field, parName, parName2);
        }
        public string DateTimeDateDiff(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string parName = string.Format("@DateDiff{0}", parIndex);
            addParame(parName, args[1]);
            //DateDiff(2015/2/5 17:59:44,t1.AddTime,@DateDiff1)>1 
            return dBAdapter.DateDiffFormat(field, args[0].ToString(), parName);
            //return string.Format("DateDiff({0},{1},{2}){3}", args[0], field, parName, args[2]);
        }
        #region group用
        /// <summary>
        /// 表示COUNT此字段
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parIndex"></param>
        /// <param name="addParame"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string Count(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            return string.Format("count({0})", field);
        }
        /// <summary>
        /// 表示SUM此字段
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parIndex"></param>
        /// <param name="addParame"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string Sum(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            return string.Format("sum({0})", field);
        }
        #endregion
        string InFormat(object value, ref int parIndex, AddParameHandler addParame)
        {
            string str = "";
            var par2 = value;
            if (par2 is string)
            {
                string parName = string.Format("@in{0}", parIndex);
                addParame(parName, value);
                str = parName;
            }
            else if (par2 is string[])
            {
                IEnumerable list = par2 as IEnumerable;
                foreach (var s in list)
                {
                    string parName = string.Format("@in{0}", parIndex);
                    addParame(parName, s);
                    parIndex += 1;
                    str += string.Format("{0},", parName);
                }
                if (str.Length > 1)
                {
                    str = str.Substring(0, str.Length - 1);
                }
            }
            else//按数字
            {
                IEnumerable list = par2 as IEnumerable;
                foreach (var s in list)
                {
                    string parName = string.Format("@in{0}", parIndex);
                    addParame(parName, (int)s);
                    parIndex += 1;
                    str += string.Format("{0},", parName);
                }
                if (str.Length > 1)
                {
                    str = str.Substring(0, str.Length - 1);
                }
            }
            return str;
        }
        public  string In(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string str = InFormat(args[0], ref parIndex, addParame);
            return dBAdapter.InFormat(field, str);
            //return string.Format("{0} IN ({1})", field, str);
        }
        public  string NotIn(string field, ref int parIndex, AddParameHandler addParame, object[] args)
        {
            string str = InFormat(args[0], ref parIndex, addParame);
            return dBAdapter.NotInFormat(field, str);
            //return string.Format("{0} NOT IN ({1})", field, str);
        }
    }
}
