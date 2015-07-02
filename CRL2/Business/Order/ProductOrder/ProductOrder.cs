using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Order.ProductOrder
{
    public class IProductOrder : ProductOrder
    {
    }
    /// <summary>
    /// 产品订单
    /// </summary>
    [Attribute.Table(TableName = "ProductOrderMain")]
    public class ProductOrder : IOrder
    {
        public new string CheckData()
        {
            return "";
        }
        /// <summary>
        /// 商家号
        /// </summary>
        public int SupplierId
        {
            get;
            set;
        }
        /// <summary>
        /// 总金额 加运费计算出
        /// </summary>
        public decimal TotalAmount
        {
            get
            {
                return Convert.ToDecimal(OrderAmount) + Convert.ToDecimal(FreightAmount);
            }
        }
        /// <summary>
        /// 原始总额
        /// </summary>
        public decimal OriginalAmount
        {
            get;
            set;
        }
        /// <summary>
        /// 付款金额,在生成订单前计算
        /// </summary>
        public decimal OrderAmount
        {
            get;
            set;
        }
        /// <summary>
        /// 运费,在生成订单时计算
        /// </summary>
        public decimal FreightAmount
        {
            get;
            set;
        }
        /// <summary>
        /// 收货人
        /// </summary>
        public string ReceiveName
        {
            get;
            set;
        }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string ReceiveAddress
        {
            get;
            set;
        }
        /// <summary>
        /// 收货人电话
        /// </summary>
        public string ReceivePhone
        {
            get;
            set;
        }
        /// <summary>
        /// 区域ID
        /// </summary>
        public string AreaId
        {
            get;
            set;
        }
        /// <summary>
        /// 送货方式
        /// </summary>
        public Freight.DeliverType DeliverType
        {
            get;
            set;
        }
        /// <summary>
        /// 赠送积分
        /// </summary>
        public decimal Integral
        {
            get;
            set;
        }
        /// <summary>
        /// 获取转换过后的状态,同Status
        /// </summary>
        public CRL.Order.ProductOrder.ProductOrderStatus OrderStatus
        {
            get
            {
                return (CRL.Order.ProductOrder.ProductOrderStatus)Status;
            }
        }
    }
}
