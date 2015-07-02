using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Order.ProductOrder
{
    public class IOrderDetail : OrderDetail
    {
    }
    /// <summary>
    /// 订单明细,不继承
    /// </summary>
    [Attribute.Table(TableName = "OrderDetail")]
    public class OrderDetail : ShoppingCart.ICartItem
    {
        /// <summary>
        /// 订单号
        /// </summary>
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public string OrderId
        {
            get;
            set;
        }
    }
}
