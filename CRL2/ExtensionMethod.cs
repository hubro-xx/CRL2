using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;
namespace CRL
{
    #region 比较时间格式
    /// <summary>
    /// 比较时间格式
    /// </summary>
    public enum DatePart
    {
        /// <summary>
        /// 年
        /// </summary>
        yy,
        /// <summary>
        /// 季度
        /// </summary>
        qq,
        /// <summary>
        /// 月
        /// </summary>
        mm,
        /// <summary>
        /// 年中的日
        /// </summary>
        dy,
        /// <summary>
        /// 日
        /// </summary>
        dd,
        /// <summary>
        /// 周
        /// </summary>
        ww,
        /// <summary>
        /// 星期
        /// </summary>
        dw,
        /// <summary>
        /// 小时
        /// </summary>
        hh,
        /// <summary>
        /// 分
        /// </summary>
        mi,
        /// <summary>
        /// 秒
        /// </summary>
        ss,
        /// <summary>
        /// 毫秒
        /// </summary>
        ms,
        /// <summary>
        /// 微妙
        /// </summary>
        mcs,
        /// <summary>
        /// 纳秒
        /// </summary>
        ns
    }
    #endregion
    /// <summary>
    /// 查询扩展方法,请引用CRL命名空间
    /// </summary>
    public static partial class ExtensionMethod
    {
        #region 手动更改值,以代替ParameCollection
        /// <summary>
        /// 用==表示值被更改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="expression"></param>
        public static void Change<T>(this T obj, Expression<Func<T, bool>> expression) where T : CRL.IModel, new()
        {
            BinaryExpression be = ((BinaryExpression)expression.Body);
            MemberExpression mExp = (MemberExpression)be.Left;
            string name = mExp.Member.Name;
            var right = be.Right;
            object value;
            if (right is ConstantExpression)
            {
                ConstantExpression cExp = (ConstantExpression)right;
                value = cExp.Value;
            }
            else
            {
                value = Expression.Lambda(right).Compile().DynamicInvoke();
            }
            obj.SetChanges(name,value);
        }
        /// <summary>
        /// 表示值被更改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="obj"></param>
        /// <param name="expression"></param>
        public static void Change<T, TKey>(this T obj, Expression<Func<T, TKey>> expression) where T : CRL.IModel, new()
        {
            MemberExpression mExp = (MemberExpression)expression.Body;
            string name = mExp.Member.Name;
            var field = TypeCache.GetProperties(typeof(T), true)[name];
            object value = field.GetValue(obj);
            obj.SetChanges(name, value);
        }
        /// <summary>
        /// 传参表示值被更改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="obj"></param>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        public static void Change<T, TKey>(this T obj, Expression<Func<T, TKey>> expression,TKey value) where T : CRL.IModel, new()
        {
            MemberExpression mExp = (MemberExpression)expression.Body;
            string name = mExp.Member.Name;
            obj.SetChanges(name, value);
        }
        #endregion
        ///// <summary>
        ///// lamada传入方法,传入要查询的字段
        ///// 示例:b.SelectField(b.Id, b.Name)
        ///// </summary>
        ///// <param name="s"></param>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //public static bool SelectField(this IModel s, params object[] args)
        //{
        //    return true;
        //}
        /// <summary>
        /// Like("%key%")
        /// </summary>
        /// <param name="s"></param>
        /// <param name="likeString"></param>
        /// <returns></returns>
        public static bool Like(this string s,string likeString)
        {
            if (string.IsNullOrEmpty(likeString))
                throw new Exception("参数值不能为空:likeString");
            return s.IndexOf(likeString) > -1;
        }
        /// <summary>
        /// NotLike("%key%")
        /// </summary>
        /// <param name="s"></param>
        /// <param name="likeString"></param>
        /// <returns></returns>
        public static bool NotLike(this string s, string likeString)
        {
            if (string.IsNullOrEmpty(likeString))
                throw new Exception("参数值不能为空:likeString");
            return s.IndexOf(likeString) == -1;
        }

        /// <summary>
        /// 字符串 in
        /// </summary>
        /// <param name="s"></param>
        /// <param name="inString"></param>
        /// <returns></returns>
        public static bool In(this string s, params string[] inString)
        {
            return true;
        }
        /// <summary>
        /// 字符串 NotIn("'1312','123123'")
        /// </summary>
        /// <param name="s"></param>
        /// <param name="inString"></param>
        /// <returns></returns>
        public static bool NotIn(this string s, params string[] inString)
        {
            return true;
        }
        /// <summary>
        /// 数字 In(12312,12312)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool In(this int s, params int[] values)
        {
            if (values==null)
                throw new Exception("参数值不能为空:inString");
            return true;
        }
        /// <summary>
        /// Enum in 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool In(this Enum s, params Enum[] values)
        {
            if (values == null)
                throw new Exception("参数值不能为空:inEnum");
            return true;
        }
        /// <summary>
        /// 数字 NotIn(1231,1231)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool NotIn(this int s, params int[] values)
        {
            if (values == null)
                throw new Exception("参数值不能为空:inString");
            return true;
        }
        /// <summary>
        /// 枚举转换为INT
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static int ToInt(this Enum e)
        {
            return Convert.ToInt32(e);
        }
        /// <summary>
        /// DateTime Between
        /// </summary>
        /// <param name="time"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool Between(this DateTime time, DateTime begin, DateTime end)
        {
            return true;
        }
        /// <summary>
        /// DateDiff
        /// </summary>
        /// <param name="time"></param>
        /// <param name="format">DatePart</param>
        /// <param name="compareTime">比较的时间</param>
        /// <returns></returns>
        public static int DateDiff(this DateTime time, DatePart format, DateTime compareTime)
        {
            return 1;
        }
        #region group用
        /// <summary>
        /// 表示Sum此字段
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static int SUM(this object origin)
        {
            return 0;
        }
        /// <summary>
        /// 表示Count此字段
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static int COUNT(this object origin)
        {
            return 0;
        }
        
        #endregion

        /// <summary>
        /// 转换共同属性的对象
        /// </summary>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDest ToType<TDest>(this object source)
            where TDest : class,new()
        {
            var simpleTypes = typeof(TDest).GetProperties();
            List<PropertyInfo> complexTypes = source.GetType().GetProperties().ToList();
            complexTypes.RemoveAll(b => b.Name == "Item");
            TDest obj = new TDest();
            foreach (var info in simpleTypes)
            {
                var complexInfo = complexTypes.Find(b => b.Name == info.Name);
                if (complexInfo != null)
                {
                    object value = complexInfo.GetValue(source, null);
                    info.SetValue(obj, value, null);
                }
            }
            return obj;
        }
        /// <summary>
        /// 转换为共同属性的集合
        /// </summary>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<TDest> ToType<TDest>(this IEnumerable source)
            where TDest : class,new()
        {
            var simpleTypes = typeof(TDest).GetProperties();
            List<PropertyInfo> complexTypes = null;
            List<TDest> list = new List<TDest>();
            foreach (var item in source)
            {
                TDest obj = new TDest();
                if (complexTypes == null)
                {
                    complexTypes = item.GetType().GetProperties().ToList();
                    complexTypes.RemoveAll(b => b.Name == "Item");
                }
                foreach (var info in simpleTypes)
                {
                    var complexInfo = complexTypes.Find(b => b.Name == info.Name);
                    if (complexInfo != null)
                    {
                        object value = complexInfo.GetValue(item,null);
                        value = ObjectConvert.ConvertObject(info.PropertyType, value);
                        info.SetValue(obj, value, null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
        public static T Find<T>(this IEnumerable<T> source,Func<T, bool> predicate)
        {
            return source.Where(predicate).FirstOrDefault();
        }
        public static List<T> FindAll<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.Where(predicate).ToList();
        }
    }
}
