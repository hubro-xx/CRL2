using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Property
{
    /// <summary>
    /// 属性值
    /// 不要继承
    /// </summary>
    [Attribute.Table(TableName = "PropertyValue")]
    public sealed class PropertyValue : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        public int PropertyId
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }
}
