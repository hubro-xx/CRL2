using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace CRL
{
    public partial class DBExtend
    {
        #region query item
        /// <summary>
        /// 按ID查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public TItem QueryItem<TItem>(int id) where TItem : IModelBase, new()
        {
            return QueryItem<TItem>(b => b.Id == id);
        }
        /// <summary>
        /// 查询返回单个结果
        /// 如果只查询ID,调用QueryItem(id)
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <param name="idDest">是否按主键倒序</param>
        /// <param name="compileSp">是否编译成存储过程</param>
        /// <returns></returns>
        public TItem QueryItem<TItem>(Expression<Func<TItem, bool>> expression, bool idDest = true, bool compileSp = false) where TItem : IModel, new()
        {
            LambdaQuery<TItem> query = new LambdaQuery<TItem>(this);
            query.Top(1);
            query.Where(expression);
            query.OrderByPrimaryKey(idDest);
            List<TItem> list = QueryList<TItem>(query, 0, compileSp);
            if (list.Count == 0)
                return null;
            return list[0];
        }
        #endregion

        #region query list
        /// <summary>
        /// 使用lamada设置条件查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        /// <param name="compileSp">是否编译成储过程</param>
        /// <returns></returns>
        public List<TItem> QueryList<TItem>(Expression<Func<TItem, bool>> expression =null, bool compileSp = false) where TItem : IModel, new()
        {
            LambdaQuery<TItem> query = new LambdaQuery<TItem>(this);
            query.Where(expression);
            string key;
            return QueryList<TItem>(query, 0, out key, compileSp);
        }
        

        /// <summary>
        /// 使用完整的LamadaQuery查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheTime">过期时间,分</param>
        /// <param name="compileSp">是否编译成储过程</param>
        /// <returns></returns>
        public List<TItem> QueryList<TItem>(LambdaQuery<TItem> query, int cacheTime = 0, bool compileSp = false) where TItem : IModel, new()
        {
            string key;
            return QueryList<TItem>(query, cacheTime, out key, compileSp);
        }
        /// <summary>
        /// 使用完整的LamadaQuery查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <param name="cacheKey">过期时间,分</param>
        /// <param name="compileSp">是否编译成储过程</param>
        /// <returns></returns>
        public List<TItem> QueryList<TItem>(LambdaQuery<TItem> query, int cacheTime, out string cacheKey, bool compileSp=false) where TItem : IModel, new()
        {
            CheckTableCreated<TItem>();
            if (query.PageSize > 0)//按分页
            {
                cacheKey = "";
                int count;
                return AutoSpPage(query, out count);
            }
            string sql = "";
            bool setConstraintObj = true;
            cacheKey = "";
            //foreach (var n in query.QueryParames)
            //{
            //    AddParam(n.Key, n.Value);
            //}
            query.FillParames(this);
            sql = query.GetQuery();
            sql = _DBAdapter.SqlFormat(sql);
            //DataTable dt;
            System.Data.Common.DbDataReader reader;
            List<TItem> list;
            if (cacheTime <= 0)
            {
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
                list = ObjectConvert.DataReaderToList<TItem>(reader,setConstraintObj, query.FieldMapping);
            }
            else
            {
                list = MemoryDataCache.GetCacheList<TItem>(sql, cacheTime, helper, out cacheKey).Values.ToList();
            }
            ClearParame();
            query.RowCount = list.Count;
            SetOriginClone(list);
            return list;
        }
       
        /// <summary>
        /// 使用封装的参数进行分页
        /// Fields(string),Sortfield(string),SortType(bool),PageSize(int),PageIndex(int),Condition(string)
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="parames"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TItem> QueryListByPage<TItem>(ParameCollection parames, out int count) where TItem : IModel, new()
        {
            string fields = parames["Fields"] + "";
            if (string.IsNullOrEmpty(fields) || fields.Trim() == "*")
            {
                var typeArry = TypeCache.GetProperties(typeof(TItem), true).Values.ToList();
                typeArry.RemoveAll(b => b.Length > 500 || b.PropertyType == typeof(byte[]));//默认移除长度大于500的字段
                fields = Base.GetQueryFields(typeArry, false);
            }
            string sort = parames["sort"] + "";
            if (string.IsNullOrEmpty(sort))
            {
                sort = "id desc";
            }
            string pageSize = parames["PageSize"] + "";
            if (string.IsNullOrEmpty(pageSize))
            {
                pageSize = "20";
            }
            string pageIndex = parames["PageIndex"] + "";
            if (string.IsNullOrEmpty(pageIndex))
            {
                pageIndex = "1";
            }
            string condition = parames["Condition"] + "";
            return QueryListByPage<TItem>(condition, fields, sort, int.Parse(pageSize), int.Parse(pageIndex), out count);
        }

        List<TItem> QueryListByPage<TItem>(string query, string fields, string sort, int pageSize, int pageIndex, out int count) where TItem : IModel, new()
        {
            query = _DBAdapter.SqlFormat(query);
            CheckTableCreated<TItem>();
            string tableName = TypeCache.GetTableName(typeof(TItem));
            query = tableName + " t1 " + _DBAdapter.GetWithNolockFormat() + "  where " + query;
            var reader = _DBAdapter.GetPageData(helper, query, fields, sort, pageSize, pageIndex);
            List<TItem> list = new List<TItem>();
            list = reader.GetData<TItem>(out count);
            SetOriginClone(list);
            return list;
        }
        #endregion

        #region count
        /// <summary>
        /// count
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public int Count<TType>(Expression<Func<TType, bool>> expression, bool compileSp = false) where TType : IModel, new()
        {
            return GetFunction<int, TType>(expression, b=>0, FunctionType.COUNT, compileSp);
        }

        #endregion
        /// <summary>
        /// 最小值
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Min<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : IModel, new()
        {
            return GetFunction<TType, TModel>(expression, field, FunctionType.MIN, compileSp);
        }
        #region Max
        /// <summary>
        /// 最大值
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Max<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : IModel, new()
        {
            return GetFunction<TType, TModel>(expression, field, FunctionType.MAX, compileSp);
        }

        #endregion

        #region SUM
        /// <summary>
        /// sum
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Sum<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : IModel, new()
        {
            return GetFunction<TType, TModel>(expression, field, FunctionType.SUM, compileSp);
        }
        #endregion

        TType GetFunction<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> selectField, FunctionType functionType, bool compileSp = false) where TModel : IModel, new()
        {
            LambdaQuery<TModel> query = new LambdaQuery<TModel>(this, false);
            string condition = query.FormatExpression(expression);
            query.FillParames(this);
            string field = "";
            if (selectField.Body is MemberExpression)
            {
                MemberExpression mExp = (MemberExpression)selectField.Body;
                field = mExp.Member.Name;
                if (!field.Contains(","))
                {
                    field = _DBAdapter.KeyWordFormat(field);
                }
            }
            else
            {
                var constant = (ConstantExpression)selectField.Body;
                field = constant.Value.ToString();
            }
            CheckTableCreated<TModel>();
            string tableName = TypeCache.GetTableName<TModel>();
            
            string sql = "select " + functionType + "(" + field + ") from " + tableName + ' ' + _DBAdapter.GetWithNolockFormat() + " where " + condition;
            if (compileSp)
            {
                return AutoExecuteScalar<TType>(sql);
            }
            return ExecScalar<TType>(sql);
        }
        /// <summary>
        /// 创建副本
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="list"></param>
        void SetOriginClone<TItem>(List<TItem> list) where TItem : IModel, new()
        {
            if (SettingConfig.UsePropertyChange)
            {
                return;
            }
            foreach (var item in list)
            {
                TItem clone = item.Clone() as TItem;
                clone.OriginClone = null;
                item.OriginClone = clone;
            }
        }
    }
}
