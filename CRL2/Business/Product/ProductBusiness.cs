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
    public class ProductBusiness<TType, TModel> : BaseProvider<TModel>
        where TType : class
        where TModel : ProductBase,new()
    {
        public static ProductBusiness<TType, TModel> Instance
        {
            get { return new ProductBusiness<TType, TModel>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        /// <summary>
        /// 更改状态
        /// </summary>
        /// <param name="product"></param>
        public void UpdateStatus(TModel product)
        {
            ParameCollection c = new ParameCollection();
            c["ProductStatus"] = (int)product.ProductStatus;
            Update(b => b.Id == product.Id, c);
        }
        /// <summary>
        /// 单个增加销量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="productId"></param>
        /// <param name="num"></param>
        public void SoldAdd(int productId,int num)
        {
            ParameCollection c2 = new ParameCollection();
            c2["$SoldCount"] = "SoldCount+" + num;
            try
            {
                Update(b => b.Id == productId, c2);
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
        public void SoldAdd<TModel, TOrderDetail>(string orderId)
            where TOrderDetail : ShoppingCart.ICartItem, new()
        {
            string sql = "update $IProductBase set SoldCount=SoldCount+b.num from $IOrderDetail b where $IProductBase.id=b.ProductId and b.OrderId=@orderId";
            var helper = dbHelper;
            helper.AddParam("orderId", orderId);
            try
            {
                helper.Execute(sql, typeof(TModel), typeof(TOrderDetail));
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
        public List<ProductBase> QuerySimpleListByPage<T>(ParameCollection parame, out int count)
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
            var list = QueryListByPage(parame, out count);
            return ObjectConvert.CloneToSimple<ProductBase, TModel>(list);
        }
        
    }
}
