using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Freight
{
    public class IFreight : Freight
    {
    }
    /// <summary>
    /// 运费
    /// </summary>
    [Attribute.Table(TableName = "Freight")]
    public class Freight : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        /// <summary>
        /// 地区ID
        /// </summary>
        public string AreaId
        {
            get;
            set;
        }
        /// <summary>
        /// 地区名称
        /// </summary>
        public string AreaName
        {
            get;
            set;
        }
        /// <summary>
        /// 首重
        /// </summary>
        public double Heavy
        {
            get;
            set;
        }
        /// <summary>
        /// 首重费用
        /// </summary>
        public double HeavyMoney
        {
            get;
            set;
        }
        /// <summary>
        /// 续重
        /// </summary>
        public double ContinuedHeavy
        {
            get;
            set;
        }
        /// <summary>
        /// 续重费用
        /// </summary>
        public double ContinuedHeavyMoney
        {
            get;
            set;
        }
        /// <summary>
        /// 发货方式（1、物流；2、快递）
        /// </summary>
        public DeliverType DeliverType
        {
            get;
            set;
        }
        /// <summary>
        /// 是否禁用了
        /// </summary>
        public bool Disable
        {
            get;
            set;
        }
        /// <summary>
        /// 供应商编号
        /// </summary>
        public int SupplierId
        {
            get;
            set;
        }
        /// <summary>
        /// 运送时间，以天为单位，0为没设定
        /// </summary>
        public int TransitTime
        {
            get;
            set;
        }
    }
}
