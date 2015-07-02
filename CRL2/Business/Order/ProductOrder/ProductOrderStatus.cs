using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreHelper;
namespace CRL.Order.ProductOrder
{
    /// <summary>
    /// 产品订单状态
    /// </summary>
    public enum ProductOrderStatus
    {

        /// <summary>
        /// 等待付款
        /// </summary>
        等待付款 = 0,
        /// <summary>
        /// 已付款
        /// </summary>
        已付款 = 1,

        #region 发货状态,如果需要调用
        /// <summary>
        /// 等待发货
        /// </summary>
        等待发货 = 2,
        /// <summary>
        /// 已发货
        /// </summary>
        已发货 = 3,
        /// <summary>
        /// 正常流程走完
        /// </summary>
        确认收货 = 4,
        #endregion

        /// <summary>
        /// 已提交申请取消
        /// </summary>
        已申请退货 = 5,
        同意退货 = 6,
        不同意退货 = 7,
        退货成功 = 8,
        //[ItemDisc("已取消但未退款")]
        ///// <summary>
        ///// 已取消但未退款(退款暂时未成功,如退还到网银)
        ///// </summary>
        //已取消但未退款,

        /// <summary>
        /// 已取消
        /// </summary>
        已取消 = 99
    }
}
