using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.TradeQuery
{
    /// <summary>
    /// 交易查询
    /// </summary>
    public class Request:RequestBase
    {
        protected override string Host
        {
            get
            {
                return "http://210.22.91.77:8404/channel";
            }
        }
        protected override string InterfacePath
        {
            get { return "/Business/EasMerchant/" + RequestName; }
        }

        public override string MessageCode
        {
            get { return "201814"; }
        }

        public override string RequestName
        {
            get { return "A101TranStaInqRq"; }
        }
        /// <summary>
        /// 原交易请求流水号
        /// </summary>
        public string OrgSrcReqId
        {
            get;
            set;
        }
        /// <summary>
        /// 原交易系统日期
        /// </summary>
        public string OrgSrcReqDate
        {
            get;
            set;
        }
        /// <summary>
        /// 原交易类型
        /// </summary>
        public string OrgTxnType
        {
            get;
            set;
        }
        /// <summary>
        /// 扩展域
        /// </summary>
        public string Reserved
        {
            get;
            set;
        }

    }
}
