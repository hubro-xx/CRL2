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
    public class OrderBusiness<TType, TModel> : BaseProvider<TModel>
        where TType : class
        where TModel : Order,new ()
    {
        public static OrderBusiness<TType, TModel> Instance
        {
            get { return new OrderBusiness<TType, TModel>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        /// <summary>
        /// 查询一个订单
        /// </summary>
        public TModel Query(string orderId)
        {
            var orderMain = QueryItem(b => b.OrderId == orderId);
            return orderMain;
        }
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <typeparam name="TMain"></typeparam>
        /// <returns></returns>
        public bool SubmitOrder(TModel order)
        {
            Add(order);
            //order.Change(b => b.Status==10);
            return true;
        }
        /// <summary>
        /// 更改订单状态
        /// </summary>
        /// <param name="order"></param>
        /// <param name="status"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public bool UpdateOrderStatus(TModel order, int status, string remark) 
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
            Update(b => b.Id == order.Id, c);
            return true;
        }
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public bool CancelOrder(TModel order, string remark)
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
        public bool ConfirmOrder(TModel order, string remark)
        {
            UpdateOrderStatus(order, (int)OrderStatus.已确认, remark);
            return true;
        }

    }
}
