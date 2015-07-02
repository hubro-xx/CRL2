using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace CRL.DicConfig
{
    /// <summary>
    /// 字典设置维护
    /// 通过TYPE区分不同的用途
    /// </summary>
    public class DicConfigAction<TType> : BaseAction<TType, IDicConfig> where TType : class
    {

        //static List<IDicConfig> _allCache = new List<IDicConfig>();
        public static List<IDicConfig> allCache
        {
            get
            {
                return GetCache<IDicConfig>(null, 3);
            }
        }
        public static void ClearCache()
        {
            ClearCache<IDicConfig>();
        }
        /// <summary>
        /// 添加一项
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static int Add(IDicConfig dic)
        {
            if (allCache.Find(b => b.Name == dic.Name && b.DicType == dic.DicType) != null)
            {
                return 0;
            }
            DBExtend helper = dbHelper;
            int id = helper.InsertFromObj(dic);
            dic.Id = id;
            allCache.Add(dic);
            return id;
        }
        /// <summary>
        /// 取名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetName(int id)
        {
            IDicConfig dic = Get(id);
            if (dic == null)
                return "";
            return dic.Name;
        }
        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetValue(int id)
        {
            IDicConfig dic = Get(id);
            if (dic == null)
                return "";
            return dic.Value;
        }
        /// <summary>
        /// 取对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IDicConfig Get(int id, bool noCache = false)
        {
            if (noCache)
            {
                return QueryItem<IDicConfig>(b => b.Id == id);
            }
            else
            {
                return allCache.Find(b => b.Id == id);
            }
        }
        /// <summary>
        /// 取该类型的对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<IDicConfig> Get(string type, bool noCache = false)
        {
            if (noCache)
            {
                return QueryList<IDicConfig>(b => b.DicType == type);
            }
            return allCache.FindAll(b => b.DicType == type);
        }
        public static CRL.DicConfig.IDicConfig Get(string type, string dName, bool noCache = false)
        {
            IDicConfig dic;
            if (noCache)
            {
                dic= QueryItem<IDicConfig>(b => b.DicType == type && b.Name == dName);
            }
            else
            {
                dic = allCache.Find(b => b.DicType == type && b.Name == dName);
            }
            if (dic == null)
            {
                throw new Exception(string.Format("找不到对应的字典 类型:{0} 名称:{1}", type, dName));
            }
            return dic;
        }
        /// <summary>
        /// 获取对象分组
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTypeGroup()
        {
            List<string> list = new List<string>();
            foreach (IDicConfig v in allCache)
            {
                if (!list.Contains(v.DicType))
                {
                    list.Add(v.DicType);
                }
            }
            return list;
        }
        /// <summary>
        /// 更新值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Update(int id,string name,string value,string remark="")
        {
            IDicConfig dic = Get(id);
            if (dic == null)
                return false;
            //if (!dic.CanChange)
            //{
            //    return false;
            //}
            CRL.ParameCollection c = new ParameCollection();
            c["name"] = name;
            c["value"] = value;
            if (!string.IsNullOrEmpty(remark))
            {
                c["remark"] = remark;
                dic.Remark = remark;
            }
            Update<IDicConfig>(b => b.Id == id, c);
            dic.Name = name;
            dic.Value = value;
            return true;
        }
        /// <summary>
        /// 删除一项
        /// </summary>
        /// <param name="id"></param>
        public static void Delete(int id)
        {
            DBExtend helper = dbHelper;
            DeleteById<IDicConfig>(id);
            allCache.RemoveAll(b => b.Id == id);
        }
    }
}
