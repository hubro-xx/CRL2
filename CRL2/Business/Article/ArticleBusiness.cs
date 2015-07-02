using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Article
{
    public class ArticleBusiness<TType, TModel> : BaseProvider<TModel>
        where TType : class
        where TModel : Article, new()
    {
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }

        //public static ArticleBusiness<TType, TModel> Instance
        //{
        //    get { return new ArticleBusiness<TType, TModel>(); }
        //}

        
    }
}
