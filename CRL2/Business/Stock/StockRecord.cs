using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Stock
{
    #region 枚举
    /// <summary>
    /// 出入类型
    /// </summary>
    public enum StockOperateType
    {
        入,
        出
    }
    /// <summary>
    /// 操作来源
    /// </summary>
    public enum FromType
    {
        正常出入库,
        订单
    }
    #endregion
    public class IStockRecord : StockRecord
    {
    }
    /// <summary>
    /// 库存变动记录
    /// </summary>
    [Attribute.Table(TableName = "StockRecord")]
    public class StockRecord : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public int StyleId
        {
            get;
            set;
        }
        private DateTime updateTime = DateTime.Now;

        public DateTime UpdateTime
        {
            get { return updateTime; }
            set { updateTime = value; }
        }
        /// <summary>
        /// 数量
        /// </summary>
        public int Num
        {
            get;
            set;
        }
        /// <summary>
        /// 出(1)还是入(0)
        /// </summary>
        public StockOperateType OperateType
        {
            get;
            set;
        }
        /// <summary>
        /// 操作类型,区分干什么用
        /// </summary>
        public FromType OperateTypeFrom
        {
            get;
            set;
        }
        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
        public string BatchNo
        {
            get;
            set;
        }
    }
}
