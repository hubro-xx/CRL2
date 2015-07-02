using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Collections;

namespace CRL.Order
{
    /// <summary>
    /// 订单维护
    /// </summary>
    public class OrderAction<TType> : BaseAction<TType, IOrder> where TType:class
    {
        
        /// <summary>
        /// 查询一个订单
        /// </summary>
        public static TMain Query<TMain>(string orderId) where TMain : IOrder, new()
        {
            string table = Base.GetTableName(typeof(TMain));
            DBExtend helper = dbHelper;
            TMain orderMain = helper.QueryItem<TMain>(b => b.OrderId == orderId);
            return orderMain;
        }
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <typeparam name="TMain"></typeparam>
        /// <returns></returns>
        public static bool SubmitOrder<TMain>(TMain order)
            where TMain : IOrder, new()
        {
            DBExtend helper = dbHelper;
            int id = helper.InsertFromObj(order);
            order.Id = id;
            return true;
        }
        /// <summary>
        /// 更改订单状态
        /// </summary>
        /// <param name="order"></param>
        /// <param name="status"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool UpdateOrderStatus<TMain>(TMain order, int status, string remark) where TMain : IOrder, new()
        {
            if (remark.Length > 100)
                remark = remark.Substring(0, 100);
            ParameCollection c = new ParameCollection();
            c["status"] = status;
            if (!string.IsNullOrEmpty(remark))
            {
                c["remark"] = remark;
            }
            c["UpdateTime"] = DateTime.Now;
            //c["PayType"] = order.PayType;
            Update<TMain>(b => b.Id == order.Id, c);
            return true;
        }
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool CancelOrder<TMain>(TMain order, string remark) where TMain : IOrder, new()
        {
            UpdateOrderStatus(order, (int)OrderStatus.已取消, remark);
            return true;
        }
        /// <summary>
        /// 确认订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static bool ConfirmOrder<TMain>(TMain order, string remark) where TMain : IOrder, new()
        {
            UpdateOrderStatus(order, (int)OrderStatus.已确认, remark);
            return true;
        }

    }
}
