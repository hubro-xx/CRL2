using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.ReceiveAddress
{
    public class IAddress : Address
    {
    }
    /// <summary>
    /// 地址
    /// </summary>
    [Attribute.Table(TableName = "ReceiveAddress")]
    public class Address : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public string UserId
        {
            get;
            set;
        }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province
        {
            get;
            set;
        }
        /// <summary>
        /// 市
        /// </summary>
        public string City
        {
            get;
            set;
        }
        /// <summary>
        /// 县、区
        /// </summary>
        public string County
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
        /// 详细地址
        /// </summary>
        [Attribute.Field(Length = 100)]
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
        public bool DefaultAddress
        {
            get;
            set;
        }
        public override string ToString()
        {
            return Province + City + County + ReceiveAddress;
        }
    }
}
