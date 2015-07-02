using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Collections;

namespace CRL.Order.ProductOrder
{
    /// <summary>
    /// 产品订单维护
    /// </summary>
    public class ProductOrderAction<TType> : BaseAction<TType, IProductOrder> where TType:class
    {
        /// <summary>
        /// 是否启用库存
        /// </summary>
        public static bool EnableStock
        {
            get{
                bool a = false;
                try
                {
                    a =Convert.ToBoolean( CoreHelper.CustomSetting.GetConfigKey("CRL_EnableStock"));
                }
                catch { }
                return a;
            }
        }
        /// <summary>
        /// 最否使用运费
        /// </summary>
        public static bool UseFreight
        {
            get
            {
                bool a = false;
                try
                {
                    a = Convert.ToBoolean(CoreHelper.CustomSetting.GetConfigKey("CRL_UseFreight"));
                }
                catch { }
                return a;
            }
        }
        static int serialNumber = 0;
        /// <summary>
        /// 生成订单号
        /// </summary>
        /// <returns></returns>
        public static string MakeOrderId()
        {
            string no;
            lock (lockObj)
            {
                serialNumber += 1;
                if (serialNumber > 10000)
                    serialNumber = 1;
                no = DateTime.Now.ToString("yyMMddhhmmssff") + serialNumber.ToString().PadLeft(5, '0');
            }
            return no;
        }
        /// <summary>
        /// 查询订单明细
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<IOrderDetail> QueryOrderDetail(string orderId)
        {
            List<IOrderDetail> orderDetail = new List<IOrderDetail>();
            DBExtend helper = dbHelper;
            orderDetail = helper.QueryList<IOrderDetail>(b => b.OrderId == orderId);
            return orderDetail;
        }
        /// <summary>
        /// 查询订单第一个产品
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static IOrderDetail QueryFirstOrderDetail(string orderId)
        {
            DBExtend helper = dbHelper;
            var orderDetail = helper.QueryItem<IOrderDetail>(b => b.OrderId == orderId);
            return orderDetail;
        }
        /// <summary>
        /// 从购物车中转为订单
        /// </summary>
        /// <typeparam name="TMain"></typeparam>
        /// <param name="order"></param>
        /// <param name="catrType"></param>
        /// <param name="groupBySupplier">true时会按商家分为不同订单</param>
        /// <returns></returns>
        public static bool SubmitOrder<TMain>(TMain order,int catrType,bool groupBySupplier)
            where TMain : IProductOrder, new()
        {
            string userId = order.UserId;
            Dictionary<int, List<ShoppingCart.ICartItem>> list = ShoppingCart.CartAction<TType>.Query(catrType, userId, groupBySupplier);
            if (list.Count == 0)
            {
                return false;
            }
            //购物车类型转为订单详细
            List<Attribute.FieldAttribute> typeArry = Base.GetProperties(typeof(ShoppingCart.ICartItem), false);
            foreach (KeyValuePair<int, List<ShoppingCart.ICartItem>> v in list)
            {
                var cartItems = v.Value;
                cartItems.RemoveAll(b => b.Selected == false);//移除未选中的
                List<IOrderDetail> details = new List<IOrderDetail>();

                details = ObjectConvert.CloneToSimple<IOrderDetail, ShoppingCart.ICartItem>(cartItems);
                order.SupplierId = v.Key;
                SubmitOrder<TMain>(order, details);
            }
            //清空购物车
            ShoppingCart.CartAction<TType>.RemoveAll(userId, catrType);
            return true;
        }

        /// <summary>
        /// 提交订单
        /// </summary>
        /// <typeparam name="TMain"></typeparam>
        /// <param name="order"></param>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public static bool SubmitOrder<TMain>(TMain order, List<IOrderDetail> orderDetail)
            where TMain : IProductOrder, new()
        {
            DBExtend helper = dbHelper;
            order.OrderId = MakeOrderId();//生成订单号
            decimal orderAmount = 0;
            decimal orderIntegral = 0;
            orderDetail.ForEach(b => {

                b.OrderId = order.OrderId;
                orderAmount += b.Price * b.Num;
                orderIntegral += b.Integral * b.Num;
            });

            //if (UseFreight)
            //{
            //    decimal freightAmount = 0;
            //    double freight1, freight2;
            //    Freight.FreightAction<TType>.CalculateFreight(orderDetail, order.AreaId, supplier, out freight1, out freight2);
            //    freightAmount = order.DeliverType == Freight.DeliverType.物流 ? (decimal)freight1 : (decimal)freight2;
            //    order.FreightAmount = freightAmount;
            //}
            order.OriginalAmount = orderAmount;
            order.Integral = orderIntegral;
            OrderAction<TType>.SubmitOrder<TMain>(order);
            //详细
            helper.BatchInsert<IOrderDetail>( orderDetail);
            return true;
        }
        /// <summary>
        /// 更改订单状态
        /// </summary>
        /// <param name="order"></param>
        /// <param name="status"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool UpdateOrderStatus<TMain>(TMain order, ProductOrderStatus status, string remark)
            where TMain : IProductOrder, new()
        {
            DBExtend helper = dbHelper;
            OrderAction<TType>.UpdateOrderStatus<TMain>(order, (int)status, remark);
            order.Status = (int)status;
            return true;
        }
        /// <summary>
        /// 影响库存,在更改状态后使用
        /// </summary>
        /// <typeparam name="TProduct"></typeparam>
        /// <typeparam name="TStock"></typeparam>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        public static void AffectStock<TProduct, TStock, TRecord>(string orderId, ProductOrderStatus status) where TProduct : Product.IProduct,new()
            where TStock : CRL.Stock.IStyle, new()
            where TRecord : CRL.Stock.IStockRecord, new()
        {
            #region 库存操作
            var op = Stock.StockOperateType.出;
            bool exc = false;
            if (status == ProductOrderStatus.已付款)
            {
                exc = true;
            }
            if (status == ProductOrderStatus.已取消)
            {
                op = Stock.StockOperateType.入;
                exc = true;
            }
            if (exc)
            {
                List<IOrderDetail> list = QueryOrderDetail(orderId);
                List<TRecord> stockChanges = new List<TRecord>();
                string batchNo = System.DateTime.Now.ToString("yyMMddhhmmssff");
                foreach (var item in list)
                {
                    TRecord s = new TRecord() { Num = item.Num, StyleId = item.StyleId };
                    stockChanges.Add(s);
                    if (op == Stock.StockOperateType.出)
                    {
                        Product.ProductAction<TType>.SoldAdd<TProduct>(item.ProductId, item.Num);
                    }
                }
                if (EnableStock)
                {
                    Stock.StockRecordBusiness<TType, TRecord>.Instance.SubmitRecord(stockChanges, batchNo, Stock.FromType.订单);
                    Stock.StockRecordBusiness<TType, TRecord>.Instance.ConfirmSubmit<TStock>(batchNo, op);
                }
            }
            #endregion
        }
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool CancelOrder<TMain>(TMain order, string remark)
            where TMain : IProductOrder, new()
        {
            UpdateOrderStatus<TMain>(order, ProductOrderStatus.已取消, remark);
            return true;
        }
        /// <summary>
        /// 确认订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool ConfirmOrder<TMain>(TMain order, string remark)
            where TMain : IProductOrder, new()
        {
            UpdateOrderStatus<TMain>(order, ProductOrderStatus.已付款, remark);
            return true;
        }

    }
}
