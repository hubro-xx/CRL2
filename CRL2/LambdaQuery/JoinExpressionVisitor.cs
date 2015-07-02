using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRL.LambdaQuery
{
    internal class JoinExpressionVisitor<TOuter,TInner> where TOuter : IModel, new()
        where TInner : IModel, new()
    {
        Dictionary<Type, string> dic = new Dictionary<Type, string>();
        public JoinExpressionVisitor()
        {
            dic.Add(typeof(TOuter),"left");
            dic.Add(typeof(TInner), "right");
        }

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

            string appendLeft = leftPar;

            sb.Append(appendLeft);//字段名称

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
                if (mExp.Expression.Type == typeof(TOuter) || mExp.Expression.Type == typeof(TInner))
                {
                    return string.Format("[{0}]{1}", dic[mExp.Expression.Type], mExp.Member.Name);
                }
                if (isRight)//按表达式右边值
                {
                    var obj = Expression.Lambda(mExp).Compile().DynamicInvoke();
                    if (obj is Enum)
                    {
                        obj = (int)obj;
                    }
                    return obj + "";
                }
                return "";
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
                return "";
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
