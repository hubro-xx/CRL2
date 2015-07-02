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
    public class ProductOrderBusiness<TType, TModel> : OrderBusiness<TType, TModel>
        where TType : class
        where TModel : ProductOrder, new()
    {
        public static ProductOrderBusiness<TType, TModel> Instance
        {
            get { return new ProductOrderBusiness<TType, TModel>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        /// <summary>
        /// 是否启用库存
        /// </summary>
        public bool EnableStock
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
        public bool UseFreight
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
        public string MakeOrderId()
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
        public List<IOrderDetail> QueryOrderDetail(string orderId)
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
        public IOrderDetail QueryFirstOrderDetail(string orderId)
        {
            var helper = dbHelper;
            var orderDetail = helper.QueryItem<IOrderDetail>(b => b.OrderId == orderId);
            return orderDetail;
        }


        /// <summary>
        /// 提交订单
        /// </summary>
        /// <typeparam name="TMain"></typeparam>
        /// <param name="order"></param>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public bool SubmitOrder(TModel order, List<IOrderDetail> orderDetail)
        {
            var helper = dbHelper;
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
            SubmitOrder(order);
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
        public bool UpdateOrderStatus(TModel order, ProductOrderStatus status, string remark)
        {
            UpdateOrderStatus(order, (int)status, remark);
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
        public void AffectStock<TProduct, TStock, TRecord>(string orderId, ProductOrderStatus status)
            where TProduct : Product.IProduct, new()
            where TStock : CRL.Stock.Style, new()
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
                        Product.ProductBusiness<TType,TProduct>.Instance.SoldAdd(item.ProductId, item.Num);
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
        public bool CancelOrder<TMain>(TModel order, string remark)
        {
            UpdateOrderStatus(order, ProductOrderStatus.已取消, remark);
            return true;
        }
        /// <summary>
        /// 确认订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public bool ConfirmOrder(TModel order, string remark)
        {
            UpdateOrderStatus(order, ProductOrderStatus.已付款, remark);
            return true;
        }

    }
}
