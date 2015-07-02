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
        bool DistinctFields = false;
        /// <summary>
        /// 表示 Distinct字段
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public LambdaQuery<T> DistinctBy<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            var fields = GetSelectField(resultSelector);
            DistinctFields = true;
            QueryFields = fields;
            return this;
        }
        bool distinctCount = false;
        /// <summary>
        /// 表示count Distinct
        /// 结果名为Total
        /// </summary>
        /// <returns></returns>
        public LambdaQuery<T> DistinctCount()
        {
            distinctCount = true;
            return this;
        }
    }
}
