using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Category
{
    public class ICategory : Category
    {
    }
    /// <summary>
    /// 分类,由于缓存,只能实现一种类型,不要继承此类实现多个实例
    /// </summary>
    [Attribute.Table(TableName = "Category")]
    public class Category : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        /// <summary>
        /// 类型,以作不同用途
        /// </summary>
        public int DataType
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public string SequenceCode
        {
            get;
            set;
        }
        public string ParentCode
        {
            get;
            set;
        }
        public string Remark
        {
            get;
            set;
        }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disable
        {
            get;
            set;
        }
        public int Sort
        {
            get;
            set;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
