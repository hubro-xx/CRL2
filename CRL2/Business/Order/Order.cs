using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Order
{
    public class IOrder : Order
    {
    }
    /// <summary>
    /// 订单原型
    /// </summary>
    public class Order : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        /// <summary>
        /// 订单ID 会自动生成
        /// </summary>
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集唯一)]
        public string OrderId
        {
            get;
            set;
        }
        /// <summary>
        /// 用户编号
        /// </summary>
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public string UserId
        {
            get;
            set;
        }
        /// <summary>
        /// 状态,为了可扩展,不直接用枚举
        /// </summary>
        [CRL.Attribute.Field(FieldIndexType = CRL.Attribute.FieldIndexType.非聚集)]
        public int Status
        {
            get;
            set;
        }
        /// <summary>
        /// 备注
        /// </summary>
        [Attribute.Field(Length = 3000, NotNull = false)]
        public string Remark
        {
            get;
            set;
        }
        /// <summary>
        /// 付款方式
        /// </summary>
        public PayType PayType
        {
            get;
            set;
        }
        public DateTime UpdateTime
        {
            get;
            set;
        }
    }
    public enum PayType
    {
        余额,
        在线支付
    }
}
