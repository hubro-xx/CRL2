using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Comment
{
    public class IComment : Comment
    {
    }
    /// <summary>
    /// 评论
    /// </summary>
    [Attribute.Table(TableName = "Comment")]
    public class Comment : IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public string UserId
        {
            get;
            set;
        }
        public string Author
        {
            get;
            set;
        }
        /// <summary>
        /// 源对象ID
        /// </summary>
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public int ObjId
        {
            get;
            set;
        }
        public int Type
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
        public string IP
        {
            get;
            set;
        }
        /// <summary>
        /// 评级
        /// </summary>
        public int Rating
        {
            get;
            set;
        }
        [Attribute.Field(Length = 500)]
        public string Content
        {
            get;
            set;
        }
    }
}
