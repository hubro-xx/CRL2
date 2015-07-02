using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Stock
{
    /// <summary>
    /// 规格
    /// </summary>
    [Attribute.Table(TableName = "Style")]
    public class Style : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        public string Name
        {
            get;
            set;
        }
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public int ProductId
        {
            get;
            set;
        }
        public int Num
        {
            get;
            set;
        }
        public int Num2
        {
            get;
            set;
        }
        public string BarCode
        {
            get;
            set;
        }
        /// <summary>
        /// SKU值串
        /// </summary>
        [Attribute.Field(Length = 200)]
        public string SKU
        {
            get;
            set;
        }
    }
}
