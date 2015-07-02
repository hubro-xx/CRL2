using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.Chinapnr
{
    public class MessageBase
    {
        public string GetSignData()
        {
            var fields = GetType().GetFields();
            string returnStr = "";
            foreach (var item in fields)
            {
                if (item.Name == "ChkValue")
                {
                    continue;
                }
                string paramValue = item.GetValue(this) + "";
                returnStr += paramValue;
            }
            return returnStr;
        }
        static string privateKey = CoreHelper.CustomSetting.GetConfigKey("汇付天下私钥文件");
        static string publicKey = CoreHelper.CustomSetting.GetConfigKey("汇付天下公钥文件");
        public string MakeSign()
        {
            string data = GetSignData();
            return data;
        }

        public bool CheckSign(string sign)
        {
            var signMsgVal = GetSignData();
            return sign == signMsgVal;
        }
    }
}
