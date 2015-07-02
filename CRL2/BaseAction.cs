using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace CRL
{
    /// <summary>
    /// 逻辑基类
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    [Obsolete("请改为继承BaseProvider实现", false)]
    public class BaseAction<TType, TModel>
        where TType : class
        where TModel : IModelBase,new()
    {
        /// <summary>
        /// 数据访问对象[基本方法]
        /// </summary>
        protected static DBExtend dbHelper
        {
            get
            {
                if (SettingConfig.GetDbAccess == null)
                {
                    throw new Exception("请配置CRL数据访问对象,实现CRL.SettingConfig.GetDbAccess");
                }
                var helper = SettingConfig.GetDbAccess(typeof(TType));
                return new DBExtend(helper);
            }
        }
        /// <summary>
        /// 创建当前类型查询表达式实列
        /// </summary>
        /// <returns></returns>
        public static LamadaQuery<TModel> GetLamadaQuery()
        {
            return new LamadaQuery<TModel>();
        }
        #region 创建缓存
        /// <summary>
        /// 按类型清除缓存
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        public static void ClearCache<TItem>() where TItem : TModel, new()
        {
            Type type = typeof(TItem);
            if (DBExtend.cacheKey.ContainsKey(type))
            {
                CRL.MemoryDataCache.RemoveCache(DBExtend.cacheKey[type]);
            }
        }

        /// <summary>
        /// 按类型获取缓存,只能在继承类实现,只能同时有一个类型
        /// 会按参数进行缓存,慎用
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <param name="expMinute"></param>
        /// <param name="unSelect"></param>
        /// <returns></returns>
        protected static List<TItem> GetCache<TItem>(Expression<Func<TItem, bool>> expression = null, int expMinute = 5, Predicate<Attribute.FieldAttribute> unSelect = null) where TItem : TModel, new()
        {
            Type type = typeof(TItem);
            string dataCacheKey;
            var query = new CRL.LamadaQuery<TItem>();
            query = query.UnSelect(unSelect).Where(expression);
            var list = dbHelper.QueryList<TItem>(query, expMinute, out dataCacheKey);
            if (!DBExtend.cacheKey.ContainsKey(type))
            {
                DBExtend.cacheKey.Add(type, dataCacheKey);
            }
            return list;
        }
        #endregion
        /// <summary>
        /// 创建TABLE[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <returns></returns>
        public static string CreateTable<TItem>() where TItem : TModel, new()
        {
            return CreateTable<TItem>(false);
        }
        /// <summary>
        /// 创建TABLE[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="is2000"></param>
        /// <returns></returns>
        public static string CreateTable<TItem>(bool is2000) where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            TItem obj1 = new TItem();
            return obj1.CreateTable(helper,is2000);
        }
        /// <summary>
        /// 创建表索引
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        public static void CreateTableIndex<TItem>() where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            TItem obj1 = new TItem();
            obj1.CheckIndexExists(helper);
        }
        /// <summary>
        /// 写日志[基本方法]
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            CoreHelper.EventLog.Log(message, "CRL", false);
        }
        /// <summary>
        /// lockObj
        /// </summary>
        protected static object lockObj = new object();

        /// <summary>
        /// 添加一条记录[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static int Add<TItem>(TItem p) where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            int id = helper.InsertFromObj(p);
            p.Id = id;
            return id;
        }
        /// <summary>
        /// 按ID获取一条记录[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TItem QueryItem<TItem>(int id) where TItem : TModel,new()
        {
            TItem article = QueryItem<TItem>(b => b.Id == id);
            return article;
        }
        /// <summary>
        /// 按条件取单个记录[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TItem QueryItem<TItem>(Expression<Func<TItem, bool>> expression, bool idDest = true) where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            return helper.QueryItem<TItem>(expression, idDest);
        }
        /// <summary>
        /// 按ID删除一条记录[基本方法]
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteById<TItem>(int id) where TItem : TModel,new()
        {
            Delete<TItem>(b => b.Id == id);
            TItem item = new TItem();
            item.Id = id;
            //_CleanDataCache(CommandType.DELETE, item);
        }
        /// <summary>
        /// 按条件删除[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static int Delete<TItem>(Expression<Func<TItem, bool>> expression) where TItem : TModel,new()
        {
            DBExtend helper = dbHelper;
            int n = helper.Delete<TItem>(expression);
            return n;
        }
        /// <summary>
        /// 返回全部结果[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <returns></returns>
        public static List<TItem> QueryList<TItem>() where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            return helper.QueryList<TItem>();
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static List<TItem> QueryList<TItem>(Expression<Func<TItem, bool>> expression) where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            return helper.QueryList<TItem>(expression);
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public static List<TItem> QueryList<TItem>(LamadaQuery<TItem> query, int cacheTime = 0) where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            return helper.QueryList<TItem>(query, cacheTime);
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public static List<TItem> QueryList<TItem>(LamadaQuery<TItem> query, int cacheTime,out string cacheKey) where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            return helper.QueryList<TItem>(query, cacheTime, out cacheKey);
        }
        /// <summary>
        /// 按条件查询[基本方法]
        /// </summary>
        /// <param name="parame"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<TItem> QueryListByPage<TItem>(ParameCollection parame, out int count) where TItem : TModel,new()
        {
            DBExtend helper = dbHelper;
            return helper.QueryListByPage<TItem>(parame, out count);
        }
        /// <summary>
        /// 按ID整体更新[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item"></param>
        public static void UpdateById<TItem>(TItem item) where TItem : TModel,new()
        {
            DBExtend helper = dbHelper;
            helper.UpdateById<TItem>(item);
            //_CleanDataCache(CommandType.UPDATE, item);
        }
        /// <summary>
        /// 指定条件和参数进行更新[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression">条件</param>
        /// <param name="setValue">值</param>
        /// <returns></returns>
        public static int Update<TItem>(Expression<Func<TItem, bool>> expression, ParameCollection setValue) where TItem : TModel,new()
        {
            DBExtend helper = dbHelper;
            int n = helper.Update<TItem>(expression, setValue);
            return n;
        }
        /// <summary>
        /// 批量插入[基本方法]
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="orderDetail"></param>
        public static void BatchInsert<TItem>(List<TItem> orderDetail) where TItem : TModel
        {
            DBExtend helper = dbHelper;
            helper.BatchInsert<TItem>(orderDetail);
        }
        #region 格式化命令查询

        /// <summary>
        /// 指定格式化查询列表[基本方法]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected static List<T> ExecListWithFormat<T>(string sql,ParameCollection parame, params Type[] types) where T : class, new()
        {
            DBExtend helper = dbHelper;
            foreach (var p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.ExecList<T>(sql, types);
        }
        /// <summary>
        /// 指定格式化更新[基本方法]
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected static int ExecuteWithFormat(string sql, ParameCollection parame, params Type[] types)
        {
            DBExtend helper = dbHelper;
            foreach (var p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.Execute(sql, types);
        }
        /// <summary>
        /// 指定格式化返回单个结果[基本方法]
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected static T ExecScalarWithFormat<T>(string sql, ParameCollection parame, params Type[] types)
        {
            DBExtend helper = dbHelper;
            foreach (var p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.ExecScalar<T>(sql, types);
        }

        #endregion

        #region 存储过程
        /// <summary>
        /// 执行存储过程返回结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        protected static List<T> RunList<T>(string sp) where T : class, new()
        {
            DBExtend helper = dbHelper;
            return helper.RunList<T>(sp);
        }
        /// <summary>
        /// 对GROUP进行分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">查询 如:ProductReceiptDetail with (nolock) group by styleid</param>
        /// <param name="fields">查询字段 如:styleid,sum(num) as total</param>
        /// <param name="rowOver">行排序 如:sum(num) desc</param>
        /// <param name="sortfield">排序字段 如:total desc</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected static List<T> GroupByPage<T>(string query, string fields, string rowOver, string sortfield,int pageSize,int pageIndex,out int count) where T : class, new()
        {
            count = 0;
            DBExtend helper = dbHelper;
            helper.AddParam("query", query);
            helper.AddParam("fields", fields);
            helper.AddParam("rowOver", rowOver);
            helper.AddParam("sortfield", sortfield);
            helper.AddParam("pageSize", pageSize);
            helper.AddParam("pageIndex", pageIndex);
            helper.AddOutParam("Counts");
            var list = helper.RunList<T>("sp_GroupPage");
            count =  Convert.ToInt32(helper.GetOutParam("Counts"));
            return list;
        }
        #endregion

        #region 统计
        public static int Count<TItem>(Expression<Func<TItem, bool>> expression) where TItem : TModel,new()
        {
            DBExtend helper = dbHelper;
            return helper.Count<TItem>(expression);
        }
        public static TType Sum<TType, TItem>(Expression<Func<TItem, bool>> expression, string field) where TItem : TModel, new()
        {
            DBExtend helper = dbHelper;
            return helper.Sum<TType, TItem>(expression, field);
        }
        #endregion
    }
}
