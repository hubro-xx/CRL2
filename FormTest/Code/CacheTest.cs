using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormTest.Code
{
    public class CacheTest:ITest
    {
        public int Data
        {
            get;
            set;
        }

        public override void Do(int data)
        {
            var item = WebTest.Code.ProductDataManage.Instance.QueryItemFromAllCache(b => b.Id > 0 && b.ProductName.Contains("product"));
        }
    }
}
