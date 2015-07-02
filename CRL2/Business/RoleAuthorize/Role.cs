using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.RoleAuthorize
{
    /// <summary>
    /// 角色组
    /// </summary>
    [Attribute.Table( TableName="Roles")]
    public sealed class Role : IModelBase
    {
        [CRL.Attribute.Field(FieldIndexType = CRL.Attribute.FieldIndexType.非聚集唯一)]
        public string Name
        {
            get;
            set;
        }
        public string Remark
        {
            get;
            set;
        }
    }
}
