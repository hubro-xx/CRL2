using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;

namespace CRL
{
    /// <summary>
    /// 键值的集合,不区分大小写
    /// 如果不需要以参数形式处理,名称前加上$ 如 c2["$SoldCount"]="SoldCount+" + num;
    /// 分页参数仅在分页时才会用到,查询缓存参数则相反
    /// </summary>
    public class ParameCollection : IgnoreCaseDictionary<object>
    {
        #region 扩展方法
        /// <summary>
        /// 设置查询字段
        /// </summary>
        /// <param name="fields"></param>
        public void SetQueryFields(string fields)
        {
            Add("Fields", fields);
        }
        /// <summary>
        /// 设置排序字段
        /// </summary>
        /// <param name="sort"></param>
        public void SetQuerySort(string sort)
        {
            Add("Sort", sort);
        }
        /// <summary>
        /// 设置每页大小
        /// </summary>
        /// <param name="pageSize"></param>
        public void SetQueryPageSize(int pageSize)
        {
            Add("PageSize", pageSize);
        }
        /// <summary>
        /// 设置页索引
        /// </summary>
        /// <param name="pageIndex"></param>
        public void SetQueryPageIndex(int pageIndex)
        {
            Add("PageIndex", pageIndex);
        }
        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="condition"></param>
        public void SetQueryCondition(string condition)
        {
            Add("Condition", condition);
        }
        /// <summary>
        /// 设置查询前几条
        /// </summary>
        /// <param name="top"></param>
        public void SetQueryTop(int top)
        {
            Add("Top", top);
        }
        /// <summary>
        /// 设置查询缓存时间,分
        /// 大于0则会产生缓存
        /// </summary>
        /// <param name="minute"></param>
        public void SetCacheTime(int minute)
        {
            Add("CacheTime", minute);
        }
        #endregion
    }
    
}
