using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Person
{
    public class SmsSendRecordManage : CRL.BaseProvider<SmsSendRecord>
    {
        public static SmsSendRecordManage Instance
        {
            get
            {
                return new SmsSendRecordManage();
            }
        }
    }
    public class SmsSendRecord : CRL.IModelBase
    {
        [CRL.Attribute.Field(FieldIndexType = CRL.Attribute.FieldIndexType.非聚集)]
        public string Mobile
        {
            get;
            set;
        }
        public string Code
        {
            get;
            set;
        }
        public string ModuleName
        {
            get;
            set;
        }
    }
}
