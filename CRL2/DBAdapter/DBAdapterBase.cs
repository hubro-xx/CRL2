using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CRL.DBAdapter
{
    internal abstract class DBAdapterBase
    {
        /// <summary>
        /// 根据数据库类型获取适配器
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static DBAdapterBase GetDBAdapterBase(CoreHelper.DBType dbType)
        {
            DBAdapterBase db = null;
            switch (dbType)
            {
                case CoreHelper.DBType.MSSQL:
                    db = new MSSQLDBAdapter();
                    break;
                case CoreHelper.DBType.MSSQL2000:
                    db = new MSSQL2000DBAdapter();
                    break;
                case CoreHelper.DBType.ACCESS:
                    break;
                case CoreHelper.DBType.MYSQL:
                    db = new MySQLDBAdapter();
                    break;
                case CoreHelper.DBType.ORACLE:
                    db = new ORACLEDBAdapter();
                    break;
            }
            if (db == null)
            {
                throw new Exception("找不到对应的DBAdapte" + dbType);
            }
            return db;
        }
        public abstract CoreHelper.DBType DBType { get; }
        #region 创建结构
        /// <summary>
        ///获取列类型和默认值
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public abstract string GetColumnType(CRL.Attribute.FieldAttribute info,out string defaultValue);
        /// <summary>
        /// 获取字段类型转换
        /// </summary>
        /// <returns></returns>
        public abstract System.Collections.Generic.Dictionary<Type, string> GetFieldMapping();
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="filed"></param>
        /// <returns></returns>
        public abstract string GetColumnIndexScript(CRL.Attribute.FieldAttribute filed);
        /// <summary>
        /// 增加列
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public abstract string GetCreateColumnScript(CRL.Attribute.FieldAttribute field);
        /// <summary>
        /// 创建存储过程
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public abstract string GetCreateSpScript(string spName, string script);
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        public abstract void CreateTable(DBExtend helper, List<Attribute.FieldAttribute> fields, string tableName);
        #endregion

        #region SQL查询
        /// <summary>
        /// 批量插入方法
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="helper"></param>
        /// <param name="details"></param>
        /// <param name="keepIdentity">否保持自增主键</param>
        public abstract void BatchInsert<TItem>(CoreHelper.DBHelper helper, List<TItem> details, bool keepIdentity = false) where TItem : IModel, new();
        /// <summary>
        /// 获取UPDATE语法
        /// </summary>
        /// <param name="table"></param>
        /// <param name="setString"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual string GetUpdateSql(string table, string setString, string where)
        {
            string sql = string.Format("update {0} set {1} where {2}", table, setString, where);
            return sql;
        }
        /// <summary>
        /// 获取删除语法
        /// </summary>
        /// <param name="table"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual string GetDeleteSql(string table, string where)
        {
            string sql = string.Format("delete from {0}  where {1}", table, where);
            return sql;
        }

        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public abstract int InsertObject(CRL.IModel obj, CoreHelper.DBHelper helper);
        /// <summary>
        /// 获取查询前几条
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="query"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public abstract string GetSelectTop(string fields, string query,string sort, int top);

        /// <summary>
        /// 获取with nolock语法
        /// </summary>
        /// <returns></returns>
        public abstract string GetWithNolockFormat();
        #endregion

        #region #region 系统查询
        /// <summary>
        /// 获取所有存储过程
        /// </summary>
        /// <returns></returns>
        public abstract string GetAllSPSql(CoreHelper.DBHelper helper);
        /// <summary>
        /// 获取所有表,查询需要转为小写
        /// </summary>
        /// <returns></returns>
        public abstract string GetAllTablesSql(CoreHelper.DBHelper helper);
        #endregion

        #region 模版
        /// <summary>
        /// 存储过程参数格式货
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public abstract string SpParameFormat(string name,string type,bool output);
        /// <summary>
        /// 关键字格式化,可能会增加后辍
        /// </summary>
        public abstract string KeyWordFormat(string value);
        /// <summary>
        /// GROUP分页模版
        /// </summary>
        public abstract string TemplateGroupPage { get; }
        /// <summary>
        /// 查询分页模版
        /// </summary>
        public abstract string TemplatePage { get; }
        /// <summary>
        /// 存储过程模版
        /// </summary>
        public abstract string TemplateSp { get; }
        /// <summary>
        /// 语句自定义格式化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract string SqlFormat(string sql);
        #endregion

        /// <summary>
        /// page
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <param name="sort"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        internal virtual CallBackDataReader GetPageData(CoreHelper.DBHelper helper, string query, string fields, string sort, int pageSize, int pageIndex)
        {
            helper.AddParam("query_", query);
            helper.AddParam("fields_", fields);
            helper.AddParam("sort_", sort);
            helper.AddParam("pageSize_", pageSize);
            helper.AddParam("pageIndex_", pageIndex);
            helper.AddOutParam("count_",1);
            //var reader = helper.RunDataReader("sp_page");
            var reader = new CallBackDataReader(helper.RunDataReader("sp_page"), () =>
            {
                return Convert.ToInt32(helper.GetOutParam("count_"));
            });
            return reader;
        }
        #region 函数语法
        public virtual string SubstringFormat(string field, int index, int length)
        {
            return string.Format(" SUBSTRING({0},{1},{2})", field, index, length);
        }

        public virtual string StringLikeFormat(string field, string parName)
        {
            return string.Format("{0} LIKE {1}", field, parName);
        }

        public virtual string StringNotLikeFormat(string field, string parName)
        {
            return string.Format("{0} NOT LIKE {1}", field, parName);
        }

        public virtual string StringContainsFormat(string field, string parName)
        {
            return string.Format("CHARINDEX({1},{0})>0", field, parName);
        }

        public virtual string BetweenFormat(string field, string parName, string parName2)
        {
            return string.Format("{0} between {1} and {2}", field, parName, parName2);
        }

        public virtual string DateDiffFormat(string field, string format, string parName)
        {
            return string.Format("DateDiff({0},{1},{2})", format, field, parName);
        }

        public virtual string InFormat(string field, string parName)
        {
            return string.Format("{0} IN ({1})", field, parName);
        }
        public virtual string NotInFormat(string field, string parName)
        {
            return string.Format("{0} NOT IN ({1})", field, parName);
        }
        #endregion
    }
}
