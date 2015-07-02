using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CRL.Product
{
    /// <summary>
    /// 产品维护
    /// </summary>
    public class ProductAction<TType> : BaseAction<TType, IProductBase> where TType : class
    {
        /// <summary>
        /// 更改状态
        /// </summary>
        /// <param name="product"></param>
        public static void UpdateStatus<T>(T product)where T: IProductBase,new()
        {
            ParameCollection c = new ParameCollection();
            c["ProductStatus"] = (int)product.ProductStatus;
            Update<T>(b => b.Id == product.Id, c);
        }
        /// <summary>
        /// 单个增加销量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="productId"></param>
        /// <param name="num"></param>
        public static void SoldAdd<T>(int productId,int num) where T : IProductBase,new()
        {
            ParameCollection c2 = new ParameCollection();
            c2["$SoldCount"] = "SoldCount+" + num;
            try
            {
                Update<T>(b => b.Id == productId, c2);
            }
            catch(Exception ero)
            {
                Log("更新销量时发生错误:" + ero.Message);
            }
        }
        /// <summary>
        /// 根据订单批量增加销量
        /// </summary>
        /// <typeparam name="TProduct"></typeparam>
        /// <typeparam name="TOrderDetail"></typeparam>
        /// <param name="orderId"></param>
        public static void SoldAdd<TProduct, TOrderDetail>(string orderId)
            where TProduct : IProductBase, new()
            where TOrderDetail : ShoppingCart.ICartItemBase, new()
        {
            string sql = "update $IProductBase set SoldCount=SoldCount+b.num from $IOrderDetail b where $IProductBase.id=b.ProductId and b.OrderId=@orderId";
            var helper = dbHelper;
            helper.AddParam("orderId", orderId);
            try
            {
                helper.Execute(sql, typeof(TProduct), typeof(TOrderDetail));
            }
            catch(Exception ero)
            {
                Log("增加商品销量时发生错误" + ero.Message);
            }
        }

        /// <summary>
        /// 总是返回简单结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parame"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<IProductBase> QuerySimpleListByPage<T>(ParameCollection parame, out int count) where T : IProductBase, new()
        {
            parame.Remove("Fields");
            parame.SetQueryFields(@"[AddTime]
      ,[CategoryCode]
      ,[Id]
      ,[ProductImage]
      ,[ProductName]
      ,[SettlementPrice]
      ,[SoldPrice]
      ,[SupplierId],[ProductStatus]");
            List<T> list = QueryListByPage<T>(parame, out count);
            return ObjectConvert.CloneToSimple<IProductBase, T>(list);
        }
        
    }
}
