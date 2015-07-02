using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CoreHelper;
namespace CRL.LambdaQuery
{
    internal class ExpressionVisitor<T> where T : IModel, new()
    {
        /// <summary>
        /// 处理后的查询参数
        /// </summary>
        internal ParameCollection QueryParames = new ParameCollection();
        int parIndex = 0;
        public string BinaryExpressionHandler(Expression left, Expression right, ExpressionType type)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            string needParKey = "=,>,<,>=,<=,<>";
            string leftPar = RouteExpressionHandler(left);//like {0}id{1}
            string typeStr = ExpressionTypeCast(type);
            var isRight = needParKey.IndexOf(typeStr) > -1;
            string rightPar = RouteExpressionHandler(right, isRight);
            //按运算符判断
            bool or = leftPar.IndexOf('&') > -1 || leftPar.IndexOf('|') > -1 || rightPar.IndexOf('&') > -1 || rightPar.IndexOf('|') > -1;
            isRight = needParKey.IndexOf(typeStr) > -1 && !or;

            string appendLeft = leftPar;
            if (left is MemberExpression)//处理虚拟字段条件
            {
                var fieldName = string.Format(leftPar, "", "");
                var filed = TypeCache.GetProperties(typeof(T), true)[fieldName];
                if (filed == null)
                {
                    throw new Exception("找不到字段 " + typeof(T).Name + " " + fieldName);
                }
                if (!string.IsNullOrEmpty(filed.VirtualField))
                {
                    appendLeft = filed.VirtualField;
                }
            }
            sb.Append(appendLeft);//字段名称
            var isRightFiled = rightPar.StartsWith("{FIELD}");
            //需要作参数处理
            if (isRight && !isRightFiled)
            {
                //去掉替换符
                string value = string.Format(rightPar, "");
                if (value.ToLower() == "true" || value.ToLower() == "false")
                {
                    value = value.ToLower() == "true" ? "1" : "0";
                }
                //参数加上前辍
                //QueryParames.Add(string.Format(leftPar, "", parIndex.ToString()), value);
                QueryParames.Add("parame" + parIndex, value);
            }
            if (rightPar.ToUpper() == "NULL")
            {
                if (typeStr == "=")
                    rightPar = " IS NULL ";
                else if (typeStr == "<>")
                    rightPar = " IS NOT NULL ";
            }
            else
            {
                sb.Append(typeStr);
                if (isRight)
                {
                    if (isRightFiled)
                    {
                        rightPar = rightPar.Replace("{FIELD}", "{0}");
                    }
                    else
                    {
                        //参数加上前辍
                        //rightPar = "@" + string.Format(leftPar,"", parIndex.ToString());
                        rightPar = "@parame" + parIndex;
                    }
                }
            }
            parIndex += 1;
            sb.Append(rightPar);
            sb.Append(")");
            return sb.ToString();
        }
        public string RouteExpressionHandler(Expression exp, bool isRight = false)
        {
            if (exp is BinaryExpression)
            {
                BinaryExpression be = (BinaryExpression)exp;
                return BinaryExpressionHandler(be.Left, be.Right, be.NodeType);
            }
            else if (exp is MemberExpression)
            {
                MemberExpression mExp = (MemberExpression)exp;
                if (isRight)//按表达式右边值
                {
                    if (mExp.Member.ReflectedType == typeof(T))
                    {
                        return "{FIELD}" + mExp.Member.Name;//格式化为别名
                    }
                    var obj = Expression.Lambda(mExp).Compile().DynamicInvoke();
                    if (obj is Enum)
                    {
                        obj = (int)obj;
                    }
                    return obj + "";
                }
                return "{0}" + mExp.Member.Name + "{1}";//格式化为别名
            }
            else if (exp is NewArrayExpression)
            {
                #region 数组
                NewArrayExpression naExp = (NewArrayExpression)exp;
                StringBuilder sb = new StringBuilder();
                foreach (Expression expression in naExp.Expressions)
                {
                    sb.AppendFormat(",{0}", RouteExpressionHandler(expression));
                }
                return sb.Length == 0 ? "" : sb.Remove(0, 1).ToString();
                #endregion
            }
            else if (exp is MethodCallExpression)
            {
                if (isRight)
                {
                    return Expression.Lambda(exp).Compile().DynamicInvoke() + "";
                }
                var methodAnalyze = new CRL.LambdaQuery.MethodAnalyze<T>();
                var dic = new Dictionary<string, CRL.LambdaQuery.MethodHandler>();
                dic.Add("Like", methodAnalyze.StringLike);
                dic.Add("NotLike", methodAnalyze.StringNotLike);
                dic.Add("Contains", methodAnalyze.StringContains);
                dic.Add("Between", methodAnalyze.DateTimeBetween);
                dic.Add("DateDiff", methodAnalyze.DateTimeDateDiff);
                dic.Add("In", methodAnalyze.In);
                dic.Add("NotIn", methodAnalyze.NotIn);
                dic.Add("Substring", methodAnalyze.Substring);
                dic.Add("COUNT", methodAnalyze.Count);
                dic.Add("SUM", methodAnalyze.Sum);
                #region 方法
                //请扩展ExtensionMethod的方法
                MethodCallExpression mcExp = (MethodCallExpression)exp;
                string methodName = mcExp.Method.Name;
                parIndex += 1;
                if (!dic.ContainsKey(methodName))
                {
                    //return Expression.Lambda(exp).Compile().DynamicInvoke() + "";
                    throw new Exception("LamadaQuery不支持方法" + mcExp.Method.Name);
                }
                string field = "";
                List<object> args = new List<object>();
                if (mcExp.Object == null)
                {
                    field = RouteExpressionHandler(mcExp.Arguments[0]);
                }
                else
                {
                    field = mcExp.Object.ToString().Split('.')[1];
                    args.Add(Expression.Lambda(mcExp.Arguments[0]).Compile().DynamicInvoke());
                }

                if (mcExp.Arguments.Count > 1)
                {
                    args.Add(Expression.Lambda(mcExp.Arguments[1]).Compile().DynamicInvoke());
                }
                if (mcExp.Arguments.Count > 2)
                {
                    args.Add(Expression.Lambda(mcExp.Arguments[2]).Compile().DynamicInvoke());
                }
                if (mcExp.Arguments.Count > 3)
                {
                    args.Add(Expression.Lambda(mcExp.Arguments[3]).Compile().DynamicInvoke());
                }
                return dic[methodName](field, ref parIndex, AddParame, args.ToArray());

                #endregion
            }
            else if (exp is ConstantExpression)
            {
                #region 常量
                ConstantExpression cExp = (ConstantExpression)exp;
                if (cExp.Value == null)
                    return "null";
                else
                {
                    return cExp.Value.ToString();
                }
                #endregion
            }
            else if (exp is UnaryExpression)
            {
                UnaryExpression ue = ((UnaryExpression)exp);
                return RouteExpressionHandler(ue.Operand, isRight);
            }
            return null;
        }
        void AddParame(string name, object value)
        {
            QueryParames.Add(name, value);
        }
        public string ExpressionTypeCast(ExpressionType expType)
        {
            switch (expType)
            {
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    throw new InvalidCastException("不支持的运算符");
            }
        }

    }
}
