using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRL
{
    public partial class DBExtend
    {
        #region delete
        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(string where) where T : IModel,new()
        {
            CheckTableCreated<T>();
            string table = TypeCache.GetTableName(typeof(T));
            string sql = _DBAdapter.GetDeleteSql(table, where);
            sql = _DBAdapter.SqlFormat(sql);
            int n = helper.Execute(sql);
            ClearParame();
            return n;
        }
        /// <summary>
        /// 按主键删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete<T>(int id) where T : IModelBase, new()
        {
            return Delete<T>(b => b.Id == id);
        }
        /// <summary>
        /// 指定条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int Delete<T>(Expression<Func<T, bool>> expression) where T : IModel, new()
        {
            LambdaQuery<T> query = new LambdaQuery<T>(this,false);
            string condition = query.FormatExpression(expression);
            query.FillParames(this);
            return Delete<T>(condition);
        }
        #endregion
    }
}
