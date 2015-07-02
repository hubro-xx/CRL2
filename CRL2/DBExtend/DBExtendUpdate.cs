using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRL
{
    public partial class DBExtend
    {
        #region update
        ParameCollection GetUpdateField<T>(T obj) where T : IModel, new()
        {
            var c = new ParameCollection();
            var fields = TypeCache.GetProperties(typeof(T), true);
            if (obj.Changes.Count > 0)
            {
                foreach (var item in obj.Changes)
                {
                    var f = fields[item.Key];
                    if (f == null)
                        continue;
                    if (f.IsPrimaryKey || f.FieldType == Attribute.FieldType.虚拟字段)
                        continue;
                    c[item.Key] = item.Value;
                }
                return c;
            }
            var origin = obj.OriginClone;
            if (origin == null)
            {
                throw new Exception("_originClone为空,请确认此对象是由查询创建");
            }
            CheckData(obj);

            foreach (var f in fields.Values)
            {
                if (f.IsPrimaryKey)
                    continue;
                if (!string.IsNullOrEmpty(f.VirtualField))
                {
                    continue;
                }
                var originValue = f.GetValue(origin);
                var currentValue = f.GetValue(obj);
                if (!Object.Equals(originValue, currentValue))
                {
                    c.Add(f.Name, currentValue);
                }
            }
            return c;

        }

        /// <summary>
        /// 指定拼接条件更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setValue"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private int Update<T>(ParameCollection setValue, string where) where T : IModel,new()
        {
            CheckTableCreated<T>();
            Type type = typeof(T);
            string table = TypeCache.GetTableName(type);
            string setString = ForamtSetValue<T>(setValue);
            string sql = _DBAdapter.GetUpdateSql(table, setString, where);
            sql = _DBAdapter.SqlFormat(sql);
            int n = helper.Execute(sql);
            ClearParame();
            return n;
        }
        
        /// <summary>
        /// 按对象差异更新,由主键确定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int Update<T>(T obj) where T : IModel, new()
        {
            var c = GetUpdateField(obj);
            if (c.Count == 0)
            {
                return 0;
                //throw new Exception("更新集合为空");
            }
            var primaryKey = TypeCache.GetTable(obj.GetType()).PrimaryKey;
            var keyValue = primaryKey.GetValue(obj);
            string where = string.Format("{0}=@{0}", primaryKey.Name);
            AddParam(primaryKey.Name, keyValue);
            int n = Update<T>(c, where);
            UpdateCacheItem(obj, c);
            if (n == 0)
            {
                throw new Exception("更新失败,找不到主键为 " + keyValue + " 的记录");
            }
            return n;
        }
        /// <summary>
        /// 指定条件并按对象差异更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update<T>(Expression<Func<T, bool>> expression, T model) where T : IModel, new()
        {
            var c = GetUpdateField(model);
            if (c.Count == 0)
            {
                return 0;
                //throw new Exception("更新集合为空");
            }
            return Update(expression, c);
        }
        /// <summary>
        /// 指定条件和参数进行更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">条件</param>
        /// <param name="setValue">值</param>
        /// <returns></returns>
        public int Update<T>(Expression<Func<T, bool>> expression, ParameCollection setValue) where T : IModel, new()
        {
            if (setValue.Count == 0)
            {
                throw new Exception("更新时发生错误,参数值为空 ParameCollection setValue");
            }
            LambdaQuery<T> query = new LambdaQuery<T>(this,false);
            string condition = query.FormatExpression(expression);
            //foreach (var n in query.QueryParames)
            //{
            //    AddParam(n.Key, n.Value);
            //}
            query.FillParames(this);
            var count = Update<T>(setValue, condition);
            UpdateCacheItem<T>(expression, setValue);
            //CacheUpdated(typeof(T).Name);
            return count;
        }
        /// <summary>
        /// 按主键更新整个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public int UpdateById<T>(T item) where T : IModel,new()
        {
            return Update(item);//直接按差异更新
        }
        #endregion
    }
}
