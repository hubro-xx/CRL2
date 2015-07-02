using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CRL
{
    public partial class DBExtend
    { 
      

        /// <summary>
        /// 对表进行分页并编译成存储过程
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TItem> AutoSpPage<TItem>(LambdaQuery<TItem> query, out int count) where TItem : IModel, new()
        {
            CheckTableCreated<TItem>();
            var fields = query.GetQueryFieldString(b => b.Length > 500 || b.PropertyType == typeof(byte[]));
            var rowOver = query.QueryOrderBy;
            if (string.IsNullOrEmpty(rowOver))
            {
                rowOver = "t1.id desc";
            }
            var orderBy = System.Text.RegularExpressions.Regex.Replace(rowOver, @"t\d\.", "t.");
            var condition = query.GetQueryConditions();
            //var parame = query.QueryParames;
            condition = _DBAdapter.SqlFormat(condition);
            count = 0;
            query.FillParames(this);
            //foreach (var n in parame)
            //{
            //    AddParam(n.Key, n.Value);
            //}
            var pageIndex = query.PageIndex;
            var pageSize = query.PageSize;
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            pageSize = pageSize == 0 ? 15 : pageSize;
            AddParam("pageIndex", pageIndex);
            AddParam("pageSize", pageSize);
            AddOutParam("count", 1);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("fields", fields);
            dic.Add("sort", orderBy);
            dic.Add("rowOver", rowOver);
            //string sql = string.Format("{0} with(nolock) where {1}", tableName, where);
            string sp = CompileSqlToSp(_DBAdapter.TemplatePage, condition, dic);
            CallBackDataReader reader;
            try
            {
                reader = new CallBackDataReader(helper.RunDataReader(sp), () =>
                {
                    return GetOutParam<int>("count");
                });
                //count = GetOutParam<int>("count");
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))//删除后自动创建
                {
                    spCahe.Remove(sp);
                    sp = CompileSqlToSp(_DBAdapter.TemplatePage, condition, dic);
                    reader = new CallBackDataReader(helper.RunDataReader(sp), () =>
                    {
                        return GetOutParam<int>("count");
                    });
                }
                else
                {
                    throw ero;
                }
            }
            var list = reader.GetData<TItem>(out count, query.FieldMapping);
            query.RowCount = count;
            ClearParame();
            //return ObjectConvert.DataReaderToList<TItem>(reader);
            return list;
        }
        /// <summary>
        /// 对GROUP进行分页
        /// </summary>
        /// <param name="query">包含了group的查询</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<dynamic> AutoSpGroupPage<T>(LambdaQuery<T> query, out int count) where T : IModel, new()
        {
            var conditions = query.GetQueryConditions();
            var fields = query.GetQueryFieldString();
            if (!conditions.Contains("group"))
            {
                throw new Exception("缺少group语法");
            }
            var rowOver = query.QueryOrderBy;
            if (string.IsNullOrEmpty(rowOver))
            {
                rowOver = "t1.id desc";
            }
            var sort1 = System.Text.RegularExpressions.Regex.Replace(rowOver, @"t\d\.", "");
            conditions = _DBAdapter.SqlFormat(conditions);
            count = 0;
            //var parame = query.QueryParames;
            //foreach (var n in parame)
            //{
            //    AddParam(n.Key, n.Value);
            //}
            query.FillParames(this);
            var pageIndex = query.PageIndex;
            var pageSize = query.PageSize;
            pageIndex = pageIndex == 0 ? 1 : pageIndex;
            pageSize = pageSize == 0 ? 15 : pageSize;
            AddParam("pageIndex", pageIndex);
            AddParam("pageSize", pageSize);
            helper.AddOutParam("count", 1);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("fields", fields);
            dic.Add("rowOver", rowOver);
            //dic.Add("sort", sort1);
            string sp = CompileSqlToSp(_DBAdapter.TemplateGroupPage, conditions, dic);
            System.Data.Common.DbDataReader reader;
            try
            {
                reader = helper.RunDataReader(sp);
                count = GetOutParam<int>("count");
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))//删除后自动创建
                {
                    spCahe.Remove(sp);
                    sp = CompileSqlToSp(_DBAdapter.TemplateGroupPage, conditions, dic);
                    reader = helper.RunDataReader(sp);
                    count = GetOutParam<int>("count");
                }
                else
                {
                    throw ero;
                }
            }
            ClearParame();
            return Dynamic.DynamicObjConvert.DataReaderToDynamic(reader);

        }
        #region sql to sp

        static Dictionary<string, int> spCahe = new Dictionary<string, int>();
        /// <summary>
        /// 将SQL语句编译成储存过程
        /// </summary>
        /// <param name="template">模版</param>
        /// <param name="sql">语桀犬吠尧</param>
        /// <param name="parames">模版替换参数</param>
        /// <returns></returns>
        string CompileSqlToSp(string template, string sql, Dictionary<string, string> parames = null)
        {
            sql = _DBAdapter.SqlFormat(sql);
            lock (lockObj)
            {
                if (spCahe.Count == 0)//初始已编译过的存储过程
                {
                    BackupParams();
                    spCahe = ExecDictionary<string, int>(_DBAdapter.GetAllSPSql(helper));
                    RecoveryParams();
                }
            }
            string fields = "";
            if (parames != null)
            {
                if (parames.ContainsKey("fields"))
                {
                    fields = parames["fields"];
                }
                if (parames.ContainsKey("sort"))
                {
                    fields += "_" + parames["sort"];
                }
                if (parames.ContainsKey("rowOver"))
                {
                    fields += "_" + parames["rowOver"];
                }
            }
            string sp = CoreHelper.StringHelper.EncryptMD5(fields + "_" + sql.Trim());
            sp = "ZautoSp_" + sp.Substring(8, 16);
            if (!spCahe.ContainsKey(sp))
            {
                sql = helper.FormatWithNolock(sql);
                string spScript = Base.SqlToProcedure(template, helper, sql, sp, parames);
                try
                {
                    BackupParams();
                    helper.Execute(spScript);
                    RecoveryParams();
                    lock (lockObj)
                    {
                        if (!spCahe.ContainsKey(sp))
                        {
                            spCahe.Add(sp, 0);
                        }
                    }
                    string log = string.Format("创建存储过程:{0}\r\n{1}", sp, spScript);
                    CoreHelper.EventLog.Log(log, "sqlToSp", false);
                }
                catch (Exception ero)
                {
                    RecoveryParams();
                    throw new Exception("动态创建存储过程时发生错误:" + ero.Message);
                }
            }
            return sp;
        }

        public T AutoSpQueryItem<T>(string sql, params Type[] types) where T : class, new()
        {
            var list = AutoSpQuery<T>(sql, types);
            return list.FirstOrDefault();
        }
        /// <summary>
        /// 将查询自动转化为存储过程执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<T> AutoSpQuery<T>(string sql, params Type[] types) where T : class, new()
        {
            var reader = AutoSpQuery(sql, types);
            return ObjectConvert.DataReaderToList<T>(reader);
        }
        System.Data.Common.DbDataReader AutoSpQuery(string sql, params Type[] types) 
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            string sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
            System.Data.Common.DbDataReader reader;
            try
            {
                reader = helper.RunDataReader(sp);
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))//删除后自动创建
                {
                    spCahe.Remove(sp);
                    sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
                    reader = helper.RunDataReader(sp);
                }
                else
                {
                    throw ero;
                }
            }
            ClearParame();
            return reader;
        }
        /// <summary>
        /// 将查询自动转化为存储过程执行
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public Dictionary<TKey, TValue> AutoSpQuery<TKey, TValue>(string sql, params Type[] types)
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            string sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
            System.Data.Common.DbDataReader reader = null;
            try
            {
                reader = helper.RunDataReader(sp);
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))//删除后自动创建
                {
                    spCahe.Remove(sp);
                    sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
                    reader = helper.RunDataReader(sp);
                }
                else
                {
                    throw ero;
                }
            }
            ClearParame();
            return ObjectConvert.DataReadToDictionary<TKey, TValue>(reader);
        }
        /// <summary>
        /// 将更新自动转化为存储过程执行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int AutoSpUpdate(string sql, params Type[] types)
        {
            int n;
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            string sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
            try
            {
                n = helper.Run(sp);
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))//删除后自动创建
                {
                    spCahe.Remove(sp);
                    sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
                    n = helper.Run(sp);
                }
                throw ero;
            }
            ClearParame();
            return n;
        }
        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T AutoExecuteScalar<T>(string sql,params Type[] types)
        {
            object obj;
            sql = _DBAdapter.SqlFormat(sql);
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            string sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
            try
            {
                obj = RunScalar(sp);
            }
            catch (Exception ero)
            {
                if (ero.Message.Contains("找不到存储过程"))//删除后自动创建
                {
                    spCahe.Remove(sp);
                    sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
                    obj = RunScalar(sp);
                }
                throw ero;
            }
            ClearParame();
            return ObjectConvert.ConvertObject<T>(obj);
        }
        #endregion
    }
}
