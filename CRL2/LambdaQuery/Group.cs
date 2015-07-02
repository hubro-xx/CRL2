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
        #region group
        /// <summary>
        /// 设置GROUP字段
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="resultSelector">like b=>new{b.Name,b.Id}</param>
        /// <returns></returns>
        public LambdaQuery<T> GroupBy<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            var fields = GetSelectField(resultSelector);
            GroupFields = fields;
            return this;
            //var allFilds2 = TypeCache.GetProperties(typeof(T), false);
            //string aliasName = GetPrefix();
            //List<Attribute.FieldAttribute> resultFields = new List<Attribute.FieldAttribute>();
            //foreach (var item in (resultSelector.Body as NewExpression).Arguments)
            //{
            //    var memberExpression = item as MemberExpression;//转换为属性访问表达式
            //    var f = allFilds2.Find(b => b.Name == memberExpression.Member.Name);
            //    f.SetFieldQueryScript(aliasName, true, false);
            //    resultFields.Add(f);
            //}
            //GroupFields = resultFields;
            //return this;
        }
        /// <summary>
        /// 设置group having条件
        /// like b => b.Number.SUM() > 1
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public LambdaQuery<T> GroupHaving(Expression<Func<T, bool>> expression)
        {
            string condition = FormatExpression(expression);
            Having += string.IsNullOrEmpty(Having) ? condition : " and " + condition;
            return this;
        }
        #endregion
    }
}
