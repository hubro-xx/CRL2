using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Area
{
    /// <summary>
    /// 查询地区区域
    /// 数据不依赖数据库,从资源中加载Resources.area
    /// </summary>
    public class AreaAction 
    {
        static Dictionary<string, IArea> cache = new Dictionary<string, IArea>();

        public static Dictionary<string, IArea> Cache
        {
            get
            {
                if (cache.Count == 0)
                {
                    #region 读取
                    //source select ID,Name,OwnerID,LevelID from area
                    string area = Properties.Resources.area;
                    string[] arry = area.Split('\r');
                    foreach (string s in arry)
                    {
                        string item = s.Trim();
                        if (string.IsNullOrEmpty(item))
                            continue;
                        item = System.Text.RegularExpressions.Regex.Replace(item, @"\s+", ",");
                        string[] arry1 = item.Split(',');
                        if (arry1.Length == 0)
                            continue;
                        IArea a = new IArea();
                        a.Code = arry1[0];
                        a.Name = arry1[1];
                        a.ParentCode = arry1[2];
                        if (a.ParentCode == "1")
                        {
                            a.Level = 1;
                        }
                        else if (a.Code.Substring(3, 3) == "100")
                        {
                            a.Level = 2;
                        }
                        else
                        {
                            a.Level = 3;
                        }
                        cache.Add(a.Code, a);
                    }
                    #endregion
                }
                return AreaAction.cache;
            }
            set { AreaAction.cache = value; }
        }
        /// <summary>
        /// 根据父级获取所有子级
        /// </summary>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        public static List<IArea> GetChild(string parentCode)
        {
            List<IArea> list = new List<IArea>();
            foreach (IArea a in Cache.Values)
            {
                if (a.ParentCode == parentCode)
                {
                    list.Add(a);
                }
            }
            return list;
        }
        public static IArea Get(string code)
        {
            if (Cache.ContainsKey(code))
                return Cache[code];
            return null;
        }
    }
}
