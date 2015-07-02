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
//Lambda 表达式参考
//http://msdn.microsoft.com/zh-cn/library/bb397687.aspx
//http://www.cnblogs.com/hubro/p/4381337.html
namespace CRL
{
    /// <summary>
    /// Lamada表达式查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class LambdaQuery<T> where T : IModel, new()
    {
        ExpressionVisitor<T> visitor = new ExpressionVisitor<T>();
        /// <summary>
        /// 返回查询唯一值
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}{1}{2}", QueryTableName, Condition, QueryTop);
        }
        #region 字段
        /// <summary>
        /// 查询返回的总行数
        /// </summary>
        public int RowCount = 0;
        /// <summary>
        /// 缓存查询过期时间
        /// </summary>
        public int ExpireMinute = 0;
        /// <summary>
        /// 处理后的查询参数
        /// </summary>
        ParameCollection QueryParames
        {
            get
            {
                return visitor.QueryParames;
            }
        }
        /// <summary>
        /// 填充参数
        /// </summary>
        /// <param name="db"></param>
        internal void FillParames(DBExtend db)
        {
            //db.ClearParams();
            foreach (var n in QueryParames)
            {
                db.SetParam(n.Key, n.Value);
            }
        }
        /// <summary>
        /// 查询的字段
        /// </summary>
        List<Attribute.FieldAttribute> QueryFields = new List<CRL.Attribute.FieldAttribute>();
        internal List<Attribute.FieldAttribute> GetQueryFields()
        {
            if (QueryFields.Count == 0)
            {
                SelectAll();
            }
            return QueryFields;
        }
        /// <summary>
        /// 查询的表名
        /// </summary>
        string QueryTableName = "";
        /// <summary>
        /// 条件
        /// </summary>
        string Condition = "";
        /// <summary>
        /// 前几条
        /// </summary>
        internal int QueryTop = 0;
        /// <summary>
        /// 排序
        /// </summary>
        internal string QueryOrderBy = "";

        bool useTableAliasesName = true;

        /// <summary>
        /// group字段
        /// </summary>
        List<Attribute.FieldAttribute> GroupFields = new List<CRL.Attribute.FieldAttribute>();

        internal int PageSize = 0;
        internal int PageIndex = 0;
        internal DBAdapter.DBAdapterBase dBAdapter;
        /// <summary>
        /// group having
        /// </summary>
        string Having = "";
        DBExtend dBExtend;
        #endregion

        #region 别名
        /// <summary>
        /// 别名
        /// </summary>
        Dictionary<Type, string> prefixs = new Dictionary<Type, string>();
        int prefixIndex = 0;
        /// <summary>
        /// 获取别名,如t1.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal string GetPrefix(Type type = null)
        {
            if (type == null)
            {
                type = typeof(T);
            }
            if (!useTableAliasesName)
            {
                return "";
            }
            if (!prefixs.ContainsKey(type))
            {
                prefixIndex += 1;
                prefixs[type] = string.Format("t{0}.", prefixIndex);
            }
            return prefixs[type];
        }
        #endregion
        DateTime startTime;
        /// <summary>
        /// lambda查询
        /// </summary>
        /// <param name="_dbExtend"></param>
        /// <param name="_useTableAliasesName">查询是否生成别名</param>
        internal LambdaQuery(DBExtend _dbExtend, bool _useTableAliasesName = true)
        {
            dBExtend = _dbExtend;
            dBAdapter = _dbExtend._DBAdapter;
            useTableAliasesName = _useTableAliasesName;
            TypeCache.SetDBAdapterCache(typeof(T), dBAdapter);
            GetPrefix(typeof(T));
            QueryTableName = TypeCache.GetTableName(typeof(T));
            startTime = DateTime.Now;
        }
        #region 对外方法
        /// <summary>
        /// 设置查询TOP
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public LambdaQuery<T> Top(int top)
        {
            QueryTop = top;
            //return Select(top, null);
            return this;
        }
        /// <summary>
        /// 投置缓存查询过期时间
        /// </summary>
        /// <param name="expireMinute"></param>
        /// <returns></returns>
        public LambdaQuery<T> Expire(int expireMinute)
        {
            ExpireMinute = expireMinute;
            return this;
        }
        /// <summary>
        /// 设定分页参数
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public LambdaQuery<T> Page(int pageSize=15,int pageIndex=1)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;
            return this;
        }
        
        #region UnSelect

        /// <summary>
        /// 按条件排除字段
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public LambdaQuery<T> UnSelect(Predicate<Attribute.FieldAttribute> match)
        {
            var fields = TypeCache.GetProperties(typeof(T), false).Values.ToList();
            if (match != null)
            {
                fields.RemoveAll(match);
            }
            string aliasName = GetPrefix();
            foreach(var item in fields)
            {
                item.SetFieldQueryScript(aliasName, true, false);
            }
            QueryFields = fields;
            return this;
        }
        #endregion

        #region Select
        void SelectAll()
        {
            var all = TypeCache.GetProperties(typeof(T), false).Values;
            QueryFields.Clear();
            var aliasName = GetPrefix();
            foreach(var item in all)
            {
                item.SetFieldQueryScript(aliasName, true, false);
                QueryFields.Add(item);
            }
        }
        /// <summary>
        /// 使用匿名类型选择查询字段
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="resultSelector">like b=>new {b.Name}</param>
        /// <returns></returns>
        public LambdaQuery<T> Select<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            //var allFilds2 = TypeCache.GetProperties(typeof(T), false);
            if (resultSelector == null)
            {
                SelectAll();
                return this;
            }
            var fields = GetSelectField(resultSelector);
            QueryFields = fields;
            return this;
        }
        string GetPropertyMethod(Expression item,out string methodName)
        {
            var method = item as MethodCallExpression;
            MemberExpression memberExpression;
            if (method.Arguments[0] is UnaryExpression)
            {
                memberExpression = (method.Arguments[0] as UnaryExpression).Operand as MemberExpression;
            }
            else
            {
                memberExpression = method.Arguments[0] as MemberExpression;
            }
            methodName = method.Method.Name;
            return memberExpression.Member.Name;
        }
        #endregion

        #region where
        /// <summary>
        /// 设置条件 可累加，按and
        /// </summary>
        /// <param name="expression">最好用变量代替属性或方法</param>
        /// <returns></returns>
        public LambdaQuery<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
                return this;
            var b = expression.ToString();
            if (QueryFields.Count == 0)
            {
                SelectAll();
            }
            string condition = FormatExpression(expression);
            this.Condition += string.IsNullOrEmpty(Condition) ? condition : " and " + condition;
            return this;
        }
        /// <summary>
        /// 直接字符串查询
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        LambdaQuery<T> Where(string condition)
        {
            this.Condition += string.IsNullOrEmpty(Condition) ? condition : " and " + condition;
            return this;
        }
        #endregion

        #region order
        /// <summary>
        /// 设置排序 可累加
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="expression"></param>
        /// <param name="desc">是否倒序</param>
        /// <returns></returns>
        public LambdaQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> expression, bool desc = true)
        {
            var fields = GetSelectField(expression);
            QueryOrderBy += string.Format(" {0} {1}", fields.First().QueryFullName, desc ? "desc" : "asc");
            return this;
        }
        /// <summary>
        /// 按主键排序
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public LambdaQuery<T> OrderByPrimaryKey(bool desc)
        {
            if (!string.IsNullOrEmpty(QueryOrderBy))
            {
                QueryOrderBy += ",";
            }
            var key = TypeCache.GetTable(typeof(T)).PrimaryKey;
            QueryOrderBy += string.Format(" {2}{0} {1}", key.Name, desc ? "desc" : "asc", GetPrefix());
            return this;
        }
        #endregion

        #region OR
        /// <summary>
        /// 按当前条件累加OR条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public LambdaQuery<T> Or(Expression<Func<T, bool>> expression)
        {
            string condition1 = FormatExpression(expression);
            this.Condition = string.Format("({0}) or {1}", Condition, condition1);
            return this;
        }
        #endregion

        #region returnSelect
        /// <summary>
        /// 按select返回匿名对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public List<TResult> ReturnSelect<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            //按索引值取动态值
            return dBExtend.QueryDynamic(this, resultSelector);
        }
        #endregion

        #endregion

        #region 获取解析值
        Dictionary<Type, string> Relations = new Dictionary<Type, string>();
        internal void AddInnerRelation(Type inner, string condition, IEnumerable<Attribute.FieldAttribute> resultFields, bool useAliasesName = true, JoinType joinType = JoinType.Left)
        {
            dBExtend.CheckTableCreated(inner);
            var table = TypeCache.GetTable(inner);
            var tableName = table.TableName;
            string aliasName = GetPrefix(inner);
            tableName = string.Format("{0} {1} ", tableName, aliasName.Substring(0, aliasName.Length - 1));
            string str = string.Format(" {0} join {1} on {2}", joinType, tableName + " " + dBAdapter.GetWithNolockFormat(),
               condition);
            if (!Relations.ContainsKey(inner))
            {
                Relations.Add(inner, str);
            }
            foreach (var f in resultFields)
            {
                f.SetFieldQueryScript(aliasName, true, useAliasesName);
                FieldMapping[f.MappingName] = f.AliasesName;
                QueryFields.Add(f);
            }
        }
        /// <summary>
        /// 转换为SQL条件，并提取参数
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal string FormatExpression(Expression<Func<T, bool>> expression)
        {
            string condition;
            if (expression == null)
                return "";
            condition = visitor.RouteExpressionHandler(expression.Body);
            //string formatAgs = useTableAliasesName ? "." : "";//前辍
            //like (Id{0}=@Id0)
            condition = string.Format(condition, GetPrefix(), "");
            return condition;
        }
        internal string FormatJoinExpression<TInner>(Expression<Func<T, TInner, bool>> expression)
            where TInner : IModel, new()
        {
            var joinVisitoer = new JoinExpressionVisitor<T, TInner>();
            string condition;
            condition = joinVisitoer.RouteExpressionHandler(expression.Body);
            condition = condition.Replace("[left]", GetPrefix(typeof(T)));
            condition = condition.Replace("[right]", GetPrefix(typeof(TInner)));
            return condition;
        }
        /// <summary>
        /// 获取查询字段字符串
        /// </summary>
        /// <param name="removes"></param>
        /// <returns></returns>
        internal string GetQueryFieldString(Predicate<Attribute.FieldAttribute> removes = null)
        {
            if (QueryFields.Count == 0)
            {
                SelectAll();
            }
            List<Attribute.FieldAttribute> queryFields = QueryFields;
            if (removes != null)
            {
                queryFields.RemoveAll(removes);
            }
            //找出需要关联的字段
            List<Attribute.FieldAttribute> constraint = queryFields.FindAll(b => b.FieldType == Attribute.FieldType.关联字段 || b.FieldType == Attribute.FieldType.关联对象);
            //找出关联和对应的字段
            int tabIndex = 2;
            foreach (Attribute.FieldAttribute a in constraint)
            {
                #region 关联约束
                tabIndex += 1;
                if (a.FieldType == Attribute.FieldType.关联字段 && a.ConstraintType == null)//虚拟字段,没有设置关联类型
                {
                    throw new Exception(string.Format("需指定关联类型:{0}.{1}.Attribute.Field.ConstraintType", typeof(T), a.Name));
                }
                if (string.IsNullOrEmpty(a.ConstraintField))//约束为空
                {
                    continue;
                }
                var arry = a.ConstraintField.Replace("$", "").Split('=');
                string leftField = GetPrefix() + arry[0];
                var innerType = a.ConstraintType;
                TypeCache.SetDBAdapterCache(innerType,dBAdapter);
                string rightField = GetPrefix(innerType) + arry[1];
                string condition = string.Format("{0}={1}", leftField, rightField);
                if (!string.IsNullOrEmpty(a.Constraint))
                {
                    a.Constraint = Regex.Replace(a.Constraint, @"(.+?)\=", GetPrefix(innerType) + "$1=");//加上前缀
                    condition += " and " + a.Constraint;
                }
                
                var innerFields = TypeCache.GetProperties(innerType, true);
                if (a.FieldType == Attribute.FieldType.关联字段)//只是关联字段
                {
                    //var resultField = innerFields.Find(b => b.Name.ToUpper() == a.ConstraintResultField.ToUpper());
                    var resultField = innerFields[a.ConstraintResultField];
                    if (resultField == null)
                    {
                        throw new Exception(string.Format("在类型{0}找不到 ConstraintResultField {1}", innerType, a.ConstraintResultField));
                    }
                    AddInnerRelation(innerType, condition, new List<Attribute.FieldAttribute>() { resultField });
                }
                else//关联对象
                {
                    AddInnerRelation(innerType, condition, innerFields.Values);
                }
                #endregion
            }
            queryFields = queryFields.FindAll(b => b.FieldType == Attribute.FieldType.数据库字段 || b.FieldType == Attribute.FieldType.虚拟字段);
            string fields = Base.GetQueryFields(queryFields, false);
            return fields;
        }
        /// <summary>
        /// 获取查询条件串,带表名
        /// </summary>
        /// <returns></returns>
        internal string GetQueryConditions()
        {
            string join = string.Join(" ", Relations.Values);
            string where = Condition;
            where = string.IsNullOrEmpty(where) ? " 1=1 " : where;
            #region group判断
            if (GroupFields.Count > 0)
            {
                where += " group by ";
                foreach (var item in GroupFields)
                {
                    where += item.QueryFullName + ",";
                }
                where = where.Substring(0, where.Length - 1);
            }
            if (!string.IsNullOrEmpty(Having))
            {
                where += " having " + Having;
            }
            #endregion

            var part = string.Format(" {0} t1 {1}  {2}  where {3}", QueryTableName, dBAdapter.GetWithNolockFormat(), join, where);
            return part;
        }
        /// <summary>
        /// 获取排序 带 order by
        /// </summary>
        /// <returns></returns>
        internal string GetOrderBy()
        {
            string orderBy = QueryOrderBy;
            orderBy = string.IsNullOrEmpty(orderBy) ? orderBy : " order by " + orderBy;
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = TypeCache.GetTable(typeof(T)).DefaultSort;
            }
            return orderBy;
        }
        /// <summary>
        /// 获取完整查询
        /// </summary>
        /// <returns></returns>
        internal string GetQuery()
        {
            string fields = GetQueryFieldString();
            if (DistinctFields)
            {
                fields = string.Format(" distinct {0}", fields);
                if (distinctCount)
                {
                    fields = string.Format(" count({0}) as Total", fields);
                }
            }
            var part = " from " + GetQueryConditions();
            
            var orderBy = GetOrderBy();
            string sql = dBAdapter.GetSelectTop(fields, part, orderBy, QueryTop);
            var ts = DateTime.Now - startTime;
            var n = ts.TotalMilliseconds;
            return sql;
        }
        #endregion

        List<Attribute.FieldAttribute> GetSelectField<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            var allFilds2 = TypeCache.GetProperties(typeof(T), false);
            string aliasName = GetPrefix();
            List<Attribute.FieldAttribute> resultFields = new List<Attribute.FieldAttribute>();
            
            if (resultSelector.Body is NewExpression)//按匿名对象
            {
                #region 按匿名对象
                var newExpression = resultSelector.Body as NewExpression;
                int i = 0;
                foreach (var item in newExpression.Arguments)
                {
                    if (item is MethodCallExpression)//group用
                    {
                        string methodName;
                        var memberName = newExpression.Members[i].Name;
                        string propertyName = GetPropertyMethod(item, out methodName);
                        //var f = allFilds2.Find(b => b.Name == propertyName).Clone();
                        var f = allFilds2[propertyName].Clone();
                        f.QueryFullName = string.Format("{0}({1})", methodName, aliasName + f.KeyWordName);
                        if (propertyName != memberName)//有别名
                        {
                            f.QueryFullName += " as " + memberName;
                        }
                        resultFields.Add(f);
                    }
                    else
                    {
                        var memberExpression = item as MemberExpression;//转换为属性访问表达式
                        //var f = allFilds2.Find(b => b.Name == memberExpression.Member.Name).Clone();
                        var f = allFilds2[memberExpression.Member.Name];
                        f.SetFieldQueryScript(aliasName, true, false);
                        resultFields.Add(f);
                    }
                    i += 1;
                }
                #endregion
            }
            else if (resultSelector.Body is MethodCallExpression)
            {
                #region 方法
                string methodName;
                string propertyName = GetPropertyMethod(resultSelector.Body, out methodName);
                //var f = allFilds2.Find(b => b.Name == propertyName).Clone();
                var f = allFilds2[propertyName].Clone();
                f.QueryFullName = string.Format("{0}({1})", methodName, aliasName + f.KeyWordName);
                resultFields.Add(f);
                #endregion
            }
            else//按成员
            {
                MemberExpression mExp = (MemberExpression)resultSelector.Body;
                //var f = allFilds2.Find(b => b.Name == mExp.Member.Name).Clone();
                var f = allFilds2[mExp.Member.Name].Clone();
                f.SetFieldQueryScript(aliasName, true, false);
                resultFields.Add(f);
            }
            return resultFields;
        }
    }
}