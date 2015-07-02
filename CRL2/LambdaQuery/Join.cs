using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using CRL.LambdaQuery;
using System.Text.RegularExpressions;

namespace CRL
{
    public partial class LambdaQuery<T> where T : IModel, new()
    {
        #region join

        /// <summary>
        /// Join,并返回筛选值
        /// </summary>
        /// <typeparam name="TInner">关联类型</typeparam>
        /// <param name="expression">关关表达式</param>
        /// <param name="resultSelector">返回值</param>
        /// <param name="joinType">join类型,默认Left</param>
        /// <returns></returns>
        public LambdaQuery<T> Join<TInner>(
            Expression<Func<T, TInner, bool>> expression,
            Expression<Func<T, TInner, object>> resultSelector,JoinType joinType= JoinType.Left) where TInner : IModel, new()
        {
            var innerType = typeof(TInner);
            //TypeCache.SetDBAdapterCache(innerType, dBAdapter);

            string condition = FormatJoinExpression(expression);
            var allFilds1 = TypeCache.GetProperties(typeof(T), true);
            var allFilds2 = TypeCache.GetProperties(innerType, true);
            var dic = new Dictionary<string, Type>();
            foreach (var item in resultSelector.Parameters)
            {
                dic.Add(item.Name, item.Type);
            }
            QueryFields.Clear();
            List<Attribute.FieldAttribute> resultFields = new List<Attribute.FieldAttribute>();
            var newExpression = resultSelector.Body as NewExpression;
            int i = 0;
            foreach (var item in newExpression.Arguments)
            {
                var arry = item.ToString().Split('.');
                var aliasName = arry[0];//like a
                var type1 = dic[aliasName];
                aliasName = GetPrefix(type1);//前缀
                Attribute.FieldAttribute f;
                string name = newExpression.Members[i].Name;
                if (type1 == innerType)//关联
                {
                    //f = allFilds2.Find(b => b.Name == arry[1]);
                    f = allFilds2[arry[1]];
                    f.AliasesName = name;
                    if (resultFields.Find(b => b.ToString() == f.ToString()) != null)
                    {
                        throw new Exception("不能指定多次相同的字段" + f.Name);
                    }
                    resultFields.Add(f);
                }
                else//自已
                {
                    //f = allFilds1.Find(b => b.Name == arry[1]);
                    f = allFilds1[arry[1]];
                    if (QueryFields.Find(b => b.ToString() == f.ToString()) != null)
                    {
                        throw new Exception("不能指定多次相同的字段" + f.Name);
                    }
                    f.AliasesName = name;
                    f.SetFieldQueryScript(GetPrefix(), true, true);
                    QueryFields.Add(f);
                }
                i += 1;
            }
            AddInnerRelation(innerType, condition, resultFields, true, joinType);
            return this;
        }

        /// <summary>
        /// 字段映射
        /// 属性名,字段名
        /// </summary>
        internal ParameCollection FieldMapping = new ParameCollection();
        /// <summary>
        /// 存入关联值到对象内部索引
        /// </summary>
        /// <typeparam name="TInner">关联类型</typeparam>
        /// <param name="expression">关联表达式</param>
        /// <param name="resultSelector">关联的字段</param>
        /// <param name="joinType">join类型,默认Left</param>
        /// <returns></returns>
        public LambdaQuery<T> AppendJoinValue<TInner>(
            Expression<Func<T, TInner, bool>> expression,
            Expression<Func<TInner, object>> resultSelector, JoinType joinType = JoinType.Left) where TInner : IModel, new()
        {
            var innerType = typeof(TInner);
            //TypeCache.SetDBAdapterCache(innerType, dBAdapter);
            if (QueryFields.Count == 0)
            {
                SelectAll();
            }

            string condition = FormatJoinExpression(expression);

            //var allFilds1 = TypeCache.GetProperties(typeof(T), true);
            var allFilds2 = TypeCache.GetProperties(innerType, true);
            List<Attribute.FieldAttribute> resultFields = new List<Attribute.FieldAttribute>();
            var newExpression = resultSelector.Body as NewExpression;
            int i = 0;
            foreach (var item in newExpression.Arguments)
            {
                var arry = item.ToString().Split('.');
                //var f = allFilds2.Find(b => b.Name == arry[1]);
                var f = allFilds2[arry[1]];
                f.MappingName = newExpression.Members[i].Name;
                resultFields.Add(f);
                i += 1;
            }
            AddInnerRelation(innerType, condition, resultFields, true, joinType);
            return this;
        }
        #endregion 
    }
}
