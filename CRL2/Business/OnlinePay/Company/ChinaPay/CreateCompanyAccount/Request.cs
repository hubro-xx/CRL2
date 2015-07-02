using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.CreateCompanyAccount
{
    public class Request : RequestBase
    {
        public override string RequestName
        {
            get { return ""; }
        }
        public override string MessageCode
        {
            get { return "201802"; }
        }
        protected override string InterfacePath
        {
            get { return "/account/reg/corp.htm"; }
        }
        /// <summary>
        /// 商户端用户号
        /// </summary>
        public string UserIdMer
        {
            get;
            set;
        }
        /// <summary>
        /// 组织机构代码证
        /// </summary>
        public string CorpOrgCode
        {
            get;
            set;
        }
        /// <summary>
        /// 营业执照
        /// </summary>
        public string CorpBusiCode
        {
            get;
            set;
        }
        /// <summary>
        /// 税务登记证
        /// </summary>
        public string CorpTaxCode
        {
            get;
            set;
        }
        /// <summary>
        /// 法人姓名
        /// </summary>
        public string LegalPerName
        {
            get;
            set;
        }
        /// <summary>
        /// 法人证件类型
        /// </summary>
        public string LegalPerIdType
        {
            get;
            set;
        }
        /// <summary>
        /// 法人证件号码
        /// </summary>
        public string LegalPerIdNo
        {
            get;
            set;
        }
        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string ContactName
        {
            get;
            set;
        }
        /// <summary>
        /// 联系人证件类型
        /// </summary>
        public string ContactIdType
        {
            get;
            set;
        }
        /// <summary>
        /// 联系人证件号码
        /// </summary>
        public string ContactIdNo
        {
            get;
            set;
        }
        /// <summary>
        /// 联系人手机
        /// </summary>
        public string ContactMobile
        {
            get;
            set;
        }
        /// <summary>
        /// 联系人邮箱
        /// </summary>
        public string ContactEmail
        {
            get;
            set;
        }
        /// <summary>
        /// 机构名称
        /// </summary>
        public string CorpName
        {
            get;
            set;
        }
        /// <summary>
        /// 机构地址
        /// </summary>
        public string CorpAddress
        {
            get;
            set;
        }
        /// <summary>
        /// 银行账户开户行
        /// </summary>
        public string OpenBank
        {
            get;
            set;
        }
        /// <summary>
        /// 银行账户号
        /// </summary>
        public string OpenAcctNo
        {
            get;
            set;
        }
        /// <summary>
        /// 银行账户户名
        /// </summary>
        public string OpenAcctName
        {
            get;
            set;
        }
        /// <summary>
        /// 银行账户开户省份
        /// </summary>
        public string OpenProv
        {
            get;
            set;
        }
        /// <summary>
        /// 银行账户开户地区
        /// </summary>
        public string OpenCity
        {
            get;
            set;
        }
        /// <summary>
        /// 银行账户开户支行
        /// </summary>
        public string OpenSubbank
        {
            get;
            set;
        }
        /// <summary>
        /// 审核标识
        /// </summary>
        public string IsChecked
        {
            get;
            set;
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
        /// 商户后台通知地址
        /// </summary>
        public string NotifyUrl
        {
            get;
            set;
        }
        /// <summary>
        /// 商户前台返回地址
        /// </summary>
        public string ReturnUrl
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
