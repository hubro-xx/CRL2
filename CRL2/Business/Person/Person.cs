using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Person
{
    public class IPerson : Person
    {
    }
    /// <summary>
    /// 会员/人
    /// </summary>
    [Attribute.Table(TableName = "Person")]
    public class Person : IModelBase,CoreHelper.FormAuthentication.IUser
    {
        /// <summary>
        /// 用户组仅在验证时用
        /// </summary>
        public string RuleName;
        /// <summary>
        /// 存入自定义数据
        /// </summary>
        public string TagData;
        public override string CheckData()
        {
            return "";
        }
        #region FORM验证存取
        /// <summary>
        /// 转为登录用的IUSER
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public CoreHelper.FormAuthentication.IUser ConverFromArry(string content)
        {
            string[] arry = content.Split('|');
            IPerson p = new IPerson();
            p.Id = Convert.ToInt32(arry[0]);
            p.Name = arry[1];
            p.RuleName = arry[2];
            if (arry.Length > 3)
            {
                p.TagData = arry[3];
            }
            return p;
        }
        /// <summary>
        /// 转为可存储的STRING
        /// </summary>
        /// <returns></returns>
        public string ToArry()
        {
            return string.Format("{0}|{1}|{2}|{3}", Id, Name, RuleName, TagData);
        }
        #endregion
        /// <summary>
        /// 名称
        /// </summary>
        [CRL.Attribute.Field(Length = 50)]
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 帐号
        /// </summary>
        [Attribute.Field(FieldIndexType=Attribute.FieldIndexType.非聚集唯一,Length=50)]
        public string AccountNo
        {
            get;
            set;
        }
        [Attribute.Field( Length=100)]
        public string PassWord
        {
            get;
            set;
        }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email
        {
            get;
            set;
        }
        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile
        {
            get;
            set;
        }
        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool Locked
        {
            get;
            set;
        }
        /// <summary>
        /// 注册IP
        /// </summary>
        public string RegisterIp
        {
            get;
            set;
        }
    }
}
