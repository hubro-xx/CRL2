using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.RoleAuthorize
{
    /// <summary>
    /// 菜单
    /// </summary>
    [Attribute.Table(TableName="Menu")]
    public sealed class Menu : Category.ICategory
    {
        [Attribute.Field(Length = 200)]
        public string Url
        {
            get;
            set;
        }
        /// <summary>
        /// 只在设置权限时使用
        /// </summary>
        public bool Que;
        private bool showInNav = true;
        /// <summary>
        /// 是否在导航菜单中显示
        /// </summary>
        public bool ShowInNav
        {
            get { return showInNav; }
            set { showInNav = value; }
        }
        public bool Enabled = true;
        public string GetPadding()
        {
            string str = "";
            int n = SequenceCode.Length / 2;
            if (n > 1)
            {
                str = "└";
            }
            for (int i = 1; i < n;i++ )
            {
                str += "────";
            }
            return str;
        }
        public override string ToString()
        {
            return string.Format("{0} {1}",SequenceCode,Name);
        }
        [CRL.Attribute.Field(MappingField = false)]
        public int Hit
        {
            get;
            set;
        }
    }
}
