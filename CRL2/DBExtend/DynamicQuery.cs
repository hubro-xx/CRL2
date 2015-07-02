using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRL
{
    //返回动态类型
    public partial class DBExtend
    {
        /// <summary>
        /// 返回dynamic集合
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public List<dynamic> ExecDynamicList(string sql, params Type[] types)
        {
            var reader = GetDataReader(sql, types);
            return Dynamic.DynamicObjConvert.DataReaderToDynamic(reader);
        }
        /// <summary>
        /// 返回dynamic集合
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public List<dynamic> RunDynamicList(string sp)
        {
            var reader = helper.RunDataReader(sp);
            ClearParame();
            return Dynamic.DynamicObjConvert.DataReaderToDynamic(reader);
        }

        /// <summary>
        /// 返回动态对象的查询
        /// 已支持join group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<dynamic> QueryDynamic<T>(LambdaQuery<T> query, int cacheTime = 0, bool compileSp = false)
            where T : IModel, new()
        {
            CheckTableCreated<T>();
            string sql = "";
            //foreach (var n in query.QueryParames)
            //{
            //    AddParam(n.Key, n.Value);
            //}
            query.FillParames(this);
            sql = query.GetQuery();
            sql = _DBAdapter.SqlFormat(sql);
            //DataTable dt;
            System.Data.Common.DbDataReader reader;
            List<dynamic> list;
            if (!compileSp)
            {
                if (query.QueryTop > 0)
                {
                    helper.AutoFormatWithNolock = false;
                }
                reader = helper.ExecDataReader(sql);
            }
            else//生成储过程
            {
                string sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
                reader = helper.RunDataReader(sql);
            }
            list = Dynamic.DynamicObjConvert.DataReaderToDynamic(reader);

            ClearParame();
            return list;
        }

        /// <summary>
        /// 返回动态类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="resultSelector"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<TResult> QueryDynamic<T, TResult>(LambdaQuery<T> query, Expression<Func<T, TResult>> resultSelector, bool compileSp = false) where T : IModel, new()
        {
            CheckTableCreated<T>();
            string sql = "";
            query.FillParames(this);
            sql = query.GetQuery();
            sql = _DBAdapter.SqlFormat(sql);
            System.Data.Common.DbDataReader reader;
            List<TResult> list;
            if (!compileSp)
            {
                if (query.QueryTop > 0)
                {
                    helper.AutoFormatWithNolock = false;
                }
                reader = helper.ExecDataReader(sql);
            }
            else//生成储过程
            {
                string sp = CompileSqlToSp(_DBAdapter.TemplateSp, sql);
                reader = helper.RunDataReader(sql);
            }
            list = Dynamic.DynamicObjConvert.DataReaderToDynamic(reader, resultSelector, query.FieldMapping);
            ClearParame();
            return list;
        }
    }
}
