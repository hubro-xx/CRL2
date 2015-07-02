using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTest.Code
{
    [CRL.Attribute.Table(TableName = "OrderMain")]//重新指定对应的表名
    public class Order : CRL.Order.IOrder
    {
        //新增Channel属性
        public string Channel
        {
            get;
            set;
        }
        
    }
}