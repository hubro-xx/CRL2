using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Comment
{
    /// <summary>
    /// 评论维护
    /// </summary>
    public class CommentAction<TType> : BaseAction<TType, IComment> where TType : class
    {
        /// <summary>
        /// 查询指定项目的评论
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="type"></param>
        /// <param name="_checked"></param>
        /// <param name="objId"></param>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<TItem> QueryObjComment<TItem>(int type,bool _checked, int objId, string userId, int pageIndex, int pageSize, out int count) where TItem : IComment, new()
        {
            DBExtend helper = dbHelper;
            ParameCollection c = new ParameCollection();
            c.SetQueryPageIndex(pageIndex);
            c.SetQueryPageSize(pageSize);
            string where = "type=" + type + " and objId=" + objId + " and checked=" + Convert.ToInt32(_checked);
            if (!string.IsNullOrEmpty(userId))
            {
                where += " and userId=" + userId;
            }
            c.SetQueryCondition(where);
            c.SetQuerySortField("id");
            c.SetQuerySortDesc(true);
            return QueryListByPage<TItem>(c, out count);
        }
    }
}
