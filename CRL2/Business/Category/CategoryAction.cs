using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
namespace CRL.Category
{
    /// <summary>
    /// 分类维护
    /// </summary>
    public class CategoryAction<TType> : BaseAction<TType, ICategory> where TType : class
    {
        static Dictionary<Type, object> typeCache = new Dictionary<Type, object>();
        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void ClearCache()
        {
            typeCache.Clear();
        }
        static void InItCache<T>() where T : ICategory, new()
        {
            lock (lockObj)
            {
                if (!typeCache.ContainsKey(typeof(T)))
                {
                    var list = QueryList<T>();
                    typeCache.Add(typeof(T),list);
                }
            }
        }
        public static List<T> GetAllCache<T>(int dataType) where T : ICategory, new()
        {
            InItCache<T>();
            var obj = typeCache[typeof(T)];
            List<T> list= obj as List<T>;
            return list.Where(b => b.DataType == dataType).OrderBy(b=>b.SequenceCode).ToList();
        }
        public static List<T> GetAllCache<T>() where T : ICategory, new()
        {
            InItCache<T>();
            var obj = typeCache[typeof(T)];
            List<T> list = obj as List<T>;
            return list.OrderBy(b => b.SequenceCode).ToList();
        }
        public static string MakeNewCode<T>(string parentSequenceCode, T category) where T : ICategory, new()
        {
            DBExtend helper = dbHelper;
            string newCode = parentSequenceCode + "";
            #region 生成新编码

            var list = QueryList<T>(b => b.ParentCode == parentSequenceCode && b.DataType == category.DataType).OrderByDescending(b => b.SequenceCode).ToList();
            if (list.Count() == 0)
            {
                newCode += "01";
            }
            else
            {
                int len = !string.IsNullOrEmpty(parentSequenceCode) ? parentSequenceCode.Length : 0;
                string max = list[0].SequenceCode;
                max = max.Substring(len, 2);
                int n = int.Parse(max);
                if (n >= 99)
                {
                    throw new Exception("子级分类已到最大级99");
                }
                newCode += (n + 1).ToString().PadLeft(2, '0');
            }
            #endregion
            return newCode;
        }
        /// <summary>
        /// 指定父级,添加分类
        /// 如果父级为空,则为第一级
        /// </summary>
        /// <param name="parentSequenceCode"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static T Add<T>(string parentSequenceCode, T category)where T:ICategory,new()
        {
            DBExtend helper = dbHelper;
            InItCache<T>();
            string newCode = MakeNewCode<T>(parentSequenceCode, category);
            //helper.Clear();
            category.SequenceCode = newCode;
            category.ParentCode = parentSequenceCode;
            int id = helper.InsertFromObj( category);
            category.Id = id;
            typeCache.Remove(typeof(T));
            return category;
        }
        /// <summary>
        /// 获取一个分类
        /// </summary>
        /// <param name="sequenceCode"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T Get<T>(string sequenceCode,int type)where T:ICategory,new()
        {
            var list = GetAllCache<T>(type);
            return list.Find(b => b.SequenceCode == sequenceCode && b.DataType == type);
        }
        

        /// <summary>
        /// 指定代码和类型删除
        /// 会删除下级
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequenceCode"></param>
        /// <param name="type"></param>
        public static void Delete<T>(string sequenceCode,int type) where T : ICategory, new()
        {
            Delete<T>(b => (b.SequenceCode == sequenceCode || b.ParentCode == sequenceCode) && b.DataType == type);
            typeCache.Remove(typeof(T));
        }
        /// <summary>
        /// 获取子分类
        /// </summary>
        /// <param name="parentSequenceCode"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> GetChild<T>(string parentSequenceCode, int type)where T:ICategory,new()
        {
            var allCache = GetAllCache<T>(type);

            return allCache.FindAll(b => b.ParentCode == parentSequenceCode).OrderByDescending(b => b.Sort).ToList();
        }

        /// <summary>
        /// 获取所有父级串
        /// </summary>
        /// <param name="sequenceCode"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> GetParents<T>(string sequenceCode, int type)where T:ICategory,new()
        {
            List<string> list = new List<string>();
            List<T> list2 = new List<T>();
            int i = 0;
            while (i <= sequenceCode.Length)
            {
                list.Add(sequenceCode.Substring(0, i));
                i += 2;
            }
            foreach(string s in list)
            {
                T item = Get<T>(s,type);
                if (item != null)
                {
                    list2.Add(item);
                }
            }
            return list2;
        }
    }
}
