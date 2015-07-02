using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay
{

    public abstract class RequestBase
    {
        /// <summary>
        /// 接口地址,后台请求请重写此地址
        /// </summary>
        protected virtual string Host
        {
            get
            {
                return "http://210.22.91.77:8004";
            }
        }
        protected abstract string InterfacePath { get; }
        public abstract string MessageCode { get; }
        /// <summary>
        /// 请求名称
        /// </summary>
        public abstract string RequestName
        {
            get;
        }
        public void SetCode()
        {
            TransCode = MessageCode;
        }
        public string InterFaceUrl
        {
            get
            {
                return Host + InterfacePath;
            }
        }
        /// <summary>
        /// 版本号
        /// </summary>
        [FieldIndex(1)]
        public string Ver
        {
            get;
            set;
        }
        /// <summary>
        /// 数字交易码
        /// </summary>
        [FieldIndex(2)]
        public string TransCode
        {
            get;
            set;
        }
        /// <summary>
        /// 商户代码
        /// </summary>
        [FieldIndex(3)]
        public string MerId
        {
            get;
            set;
        }
        /// <summary>
        /// 请求日期
        /// </summary>
        [FieldIndex(4)]
        public string SrcReqDate
        {
            get;
            set;
        }
        /// <summary>
        /// 请求流水号
        /// </summary>
        [FieldIndex(5)]
        public string SrcReqId
        {
            get;
            set;
        }
        /// <summary>
        /// 渠道号
        /// </summary>
        [FieldIndex(6)]
        public string ChannelId
        {
            get;
            set;
        }
        /// <summary>
        /// 签名
        /// </summary>
        [FieldIndex(7)]
        public string Signature
        {
            get;
            set;
        }
        
    }
}
