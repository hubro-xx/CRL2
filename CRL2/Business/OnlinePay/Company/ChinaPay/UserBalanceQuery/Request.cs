using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.UserBalanceQuery
{
    /// <summary>
    /// 查询用户余额
    /// </summary>
    public class Request : RequestBase
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
            get { return "201811"; }
        }

        public override string RequestName
        {
            get { return "A101AcctBalInqRq"; }
        }
        /// <summary>
        /// 用户号
        /// </summary>
        public string UserId
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
