using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace CRL.Article
{
    /// <summary>
    /// 文章内容维护
    /// </summary>
    public class ArticleAction<TType> : BaseAction<TType, IArticle> where TType : class
    {

    }
}
