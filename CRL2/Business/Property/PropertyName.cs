using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Property
{
    /// <summary>
    /// 属性名称
    /// 不要继承
    /// </summary>
    [Attribute.Table(TableName = "PropertyName")]
    public sealed class PropertyName : IModelBase
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
        /// <summary>
        /// 分类Code
        /// </summary>
        public string CategoryCode
        {
            get;
            set;
        }
        public string Remark
        {
            get;
            set;
        }
        public int Sort
        {
            get;
            set;
        }
        /// <summary>
        /// 类型,以作不同用途
        /// </summary>
        public PropertyType Type
        {
            get;
            set;
        }
    }
}
