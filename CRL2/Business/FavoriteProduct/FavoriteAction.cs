using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.FavoriteProduct
{
    /// <summary>
    /// 商品收藏
    /// </summary>
    public class FavoriteAction<TType> : BaseAction<TType, IFavoriteProduct> where TType : class
    {
        /// <summary>
        /// 添加到收藏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="product"></param>
        public static void AddToFavorite<T>(T product) where T : IFavoriteProduct,new()
        {
            //Delete<IFavoriteProduct>(b => b.UserId == product.UserId && b.ProductId == product.ProductId);
            if (QueryItem<T>(b => b.UserId == product.UserId && b.ProductId == product.ProductId) == null)
            {
                Add(product);
            }
        }
        /// <summary>
        /// 查询用户收藏
        /// </summary>
        /// <typeparam name="TProduct"></typeparam>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        //public List<Product.IProductBase> QueryUserFavorite<TProduct>(string userId, int pageIndex, int pageSize, out int count)
        //    where TProduct:Product.IProduct,new()
        //{
        //    List<TProduct> products=new List<TProduct>();
        //    ParameCollection c = new ParameCollection();
        //    c.SetQueryPageIndex(pageIndex);
        //    c.SetQueryPageSize(pageSize);
        //    c["userId"] = userId;
        //    c.SetQuerySortField("AddTime");
        //    c.SetQuerySortDesc(true);
        //    //查询收藏
        //    List<IFavoriteProduct> list = QueryListByPage<IFavoriteProduct>(c,out count);
        //    if (list.Count == 0)
        //        return new List<Product.IProductBase>();

        //    Dictionary<int, DateTime> list2 = new Dictionary<int, DateTime>();
        //    ParameCollection c2 = new ParameCollection();
        //    string inStr = "";
        //    foreach (var item in list)
        //    {
        //        list2.Add(item.ProductId, item.AddTime);
        //        inStr += string.Format("{0},",item.ProductId);
        //    }
        //    inStr = inStr.Substring(0, inStr.Length - 1);
        //    c["$id"] = string.Format(" in({0})", inStr);
        //    c2.SetQueryFields("id,ProductName,SoldPrice,ProductImage");
        //    //关联出产品
        //    products = Product.ProductAction.QueryList<TProduct>(c2);
        //    foreach(TProduct item in products)
        //    {
        //        item.AddTime = list2[item.Id];//更新时间
        //    }
        //    return Base.CloneToSimple<Product.IProductBase, TProduct>(products);
        //}
    }
}
