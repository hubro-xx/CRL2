using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.TradeType
{
    /// <summary>
    /// 交易类型
    /// </summary>
    [CRL.Attribute.Table(TableName = "TradeType")]
    public class TradeType : CRL.Category.ICategory
    {
        /// <summary>
        /// 交易方向
        /// </summary>
        public TradeDirection TradeDirection
        {
            get;
            set;
        }
        /// <summary>
        /// 重新计算后的交易类型代码
        /// </summary>
        [CRL.Attribute.Field(FieldIndexType = CRL.Attribute.FieldIndexType.非聚集唯一)]
        public string TradeCode
        {
            get;
            set;
        }
    }
    
}
