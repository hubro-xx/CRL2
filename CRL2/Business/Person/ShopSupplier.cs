using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Person
{
    public class IShopSupplier : ShopSupplier
    {
    }
    /// <summary>
    /// 商家/供货商
    /// </summary>
    [Attribute.Table(TableName = "ShopSupplier")]
    public class ShopSupplier : Person
    {
        /// <summary>
        /// 免运费金额
        /// </summary>
        public decimal MinFreePostAmount
        {
            get;
            set;
        }
        //todo 其它字段
    }
}
