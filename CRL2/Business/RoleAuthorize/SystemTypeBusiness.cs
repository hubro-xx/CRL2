using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.RoleAuthorize
{
    public class SystemTypeBusiness : BaseProvider<SystemType>
    {
        public static SystemTypeBusiness Instance
        {
            get { return new SystemTypeBusiness(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<SystemTypeBusiness>(); }
        }
        //static List<SystemType> systemTypes;
        public List<SystemType> SystemTypes
        {
            get
            {
                //if (systemTypes == null)
                //{
                    var systemTypes = QueryList().OrderByDescending(b=>b.Id).ToList();
                //}
                return systemTypes;
            }
        }
    }
}
