using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreHelper;
namespace CRL.Order
{
    /// <summary>
    /// 一般订单状态
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// 已提交
        /// </summary>
        已提交,

        /// <summary>
        /// 已确认
        /// </summary>
        已确认,

        ///<summary>
        /// 已取消
        /// </summary>
        已取消
    }
}
