using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.RoleAuthorize
{
    /// <summary>
    /// 用户
    /// </summary>
    [Attribute.Table(TableName = "Employee")]
    public class Employee : Person.IPerson
    {
        /// <summary>
        /// qq
        /// </summary>
        public string QQ
        {
            get;
            set;
        }
        /// <summary>
        /// 角色
        /// </summary>
        public int Role
        {
            get;
            set;
        }
        /// <summary>
        /// 角色名称
        /// </summary>
        public String RoleName
        {
            get{
                return CRL.RoleAuthorize.RoleBusiness.Instance.QueryItem(b => b.Id == Role).Name;
            } 
        }
        [CRL.Attribute.Field(Length=100)]
        public string Token
        {
            get;
            set;
        }
        public string Ip
        {
            get;
            set;
        }
        public DateTime Birthday
        {
            get;
            set;
        }
        public string Sex
        {
            get;
            set;
        }
        /// <summary>
        /// 身份证号
        /// </summary>
        [CRL.Attribute.Field(Length = 50)]
        public string IdentityNo
        {
            get;
            set;
        }
        [Attribute.Field(Length = 100)]
        public string Address
        {
            get;
            set;
        }
        [Attribute.Field(Length = 100)]
        public string HeadImg
        {
            get;
            set;
        }
        /// <summary>
        /// 部门
        /// </summary>
        public string Department
        {
            get;
            set;
        }
    }
}
