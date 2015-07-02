using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.RoleAuthorize
{
    [Attribute.Table(TableName = "Department")]
    public class Department:Category.Category
    {
    }
}
