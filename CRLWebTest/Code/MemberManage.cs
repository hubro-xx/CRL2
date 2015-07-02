using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTest.Code
{
    public class MemberManage : CRL.BaseProvider<Member>
    {
        public static MemberManage ContextInstance<T>(CRL.BaseProvider<T> baseProvider) where T : CRL.IModel, new()
        {
            var instance = Instance;
            instance.SetContext(baseProvider);
            return instance;
        }

        /// <summary>
        /// 实例访问入口
        /// </summary>
        public static MemberManage Instance
        {
            get { return new MemberManage(); }
        }
    }
}