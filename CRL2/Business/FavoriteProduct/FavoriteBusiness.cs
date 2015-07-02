using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.FavoriteProduct
{
    /// <summary>
    /// 商品收藏
    /// </summary>
    public class FavoriteBusiness<TType, TModel> : BaseProvider<TModel>
        where TType : class
        where TModel : FavoriteProduct,new()
    {
        public static FavoriteBusiness<TType, TModel> Instance
        {
            get { return new FavoriteBusiness<TType, TModel>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        /// <summary>
        /// 添加到收藏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="product"></param>
        public void AddToFavorite(TModel product)
        {
            //Delete<IFavoriteProduct>(b => b.UserId == product.UserId && b.ProductId == product.ProductId);
            if (QueryItem(b => b.UserId == product.UserId && b.ProductId == product.ProductId) == null)
            {
                Add(product);
            }
        }

    }
}
