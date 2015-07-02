using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Advert
{
    /// <summary>
    /// 广告管理
    /// </summary>
    public class AdvertAction<TType> : BaseAction<TType, IAdvert> where TType : class
    {
        static List<IAdvert> AllCache
        {
            get
            {
                return GetCache<IAdvert>(b => b.Disable == false);
            }
        }
        public static void ClearCache()
        {
            ClearCache<IAdvert>();
        }
        public static IAdvert QueryItem(int id)
        {
            var item = AllCache.Find(b => b.Id == id);
            if (item == null)
            {
                item = new IAdvert();
                item.Title = "尚未添加";
            }
            return item;
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static List<IAdvert> Query(string categoryCode, int top)
        {
            //ParameCollection c = new ParameCollection();
            //c.SetQuerySortField("Sort");
            //c.SetQuerySortType(true);
            //c["categoryCode"] = categoryCode;
            //c["Disable"] = 0;
            //c.SetCacheTime(10);
            //c.SetQueryTop(top);
            //List<IAdvert> list = QueryList<IAdvert>(c);
            //return list;
            List<IAdvert> list = AllCache.FindAll(b => b.CategoryCode == categoryCode).Skip(0).Take(top).OrderByDescending(b => b.Sort).ToList();
            return list;
        }
        /// <summary>
        /// 输出HTML
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static string GetAdHtmlOutput(string categoryCode, int top)
        {
            List<IAdvert> list = Query(categoryCode, top);
            string html = "";
            foreach(var item in list)
            {
                string code = "adf=" + item.CategoryCode + "_" + item.Id;
                string title = item.Title.Replace("\"","'");
                html += string.Format("<a href='{0}{5}' target='_blank' ><img src='{1}' with='{3}' height='{4}' title='{2}' /></a>", item.Url, item.ImageUrl, title, item.Width, item.Height,
                    item.Url.IndexOf("?") > 1 ? "&" + code : "?" + code);
            }
            return html;
        }
        /// <summary>
        /// 输出JS
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static string GetAdJsOutput(string categoryCode, int top)
        {
            string html = GetAdHtmlOutput(categoryCode, top);
            string js = string.Format("document.write(\"{0}\")", html);
            return js;
        }
        /// <summary>
        /// 输出JSON
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static string GetAdJsonOutput(string categoryCode, int top)
        {
            List<IAdvert> list = Query(categoryCode, top);
            return CoreHelper.SerializeHelper.SerializerToJson(list);
        }
    }
}
