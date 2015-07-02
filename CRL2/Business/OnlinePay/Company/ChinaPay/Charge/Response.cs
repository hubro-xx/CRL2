using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.Charge
{
    public class Response:ResponseBase
    {
        public override string ResponseName
        {
            get { return "C101RechargeCbRq"; }
        }
        /// <summary>
        /// 商户私有域
        /// </summary>
        public string MerPriv
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
