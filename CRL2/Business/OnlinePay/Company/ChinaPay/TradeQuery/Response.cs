using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.TradeQuery
{
    public class Response : ResponseBase
    {
        public override string ResponseName
        {
            get { return "A101TranStaInqRs"; }
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
        /// 交易状态 00 交易成功 01 交易不存在 02 交易失败
        /// </summary>
        public string Status
        {
            get;
            set;
        }
        /// <summary>
        /// 失败原因
        /// </summary>
        public string ReturnMsg
        {
            get;
            set;
        }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrgOrderNo
        {
            get;
            set;
        }
    }
}
