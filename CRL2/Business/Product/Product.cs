using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Product
{
    public class IProduct : Product
    {
    }
    /// <summary>
    /// 产品
    /// </summary>
    [Attribute.Table(TableName = "Product")]
    public class Product : ProductBase
    {
        /// <summary>
        /// 原始商品ID
        /// </summary>
        public int OriginalId
        {
            get;
            set;
        }
        /// <summary>
        /// 传入购物车的TagData
        /// </summary>
        public string TagData;
        /// <summary>
        /// 属性值串
        /// </summary>
        [Attribute.Field(Length=200)]
        public string PropertyString
        {
            get;
            set;
        }
        /// <summary>
        /// 重量
        /// </summary>
        public double Weight
        {
            get;
            set;
        }
        /// <summary>
        /// 满多少个免运费，0为不免
        /// </summary>
        public int ExemptFreightCount
        {
            get;
            set;
        }
        /// <summary>
        /// 是否计入商家满就免运费金额
        /// </summary>
        public bool IncludedFreePost
        {
            get;
            set;
        }
        /// <summary>
        /// 积分
        /// </summary>
        public decimal Integral
        {
            get;
            set;
        }
        /// <summary>
        /// 商品图片,|分割
        /// </summary>
        [Attribute.Field(Length = 500)]
        public string DetailImage
        {
            get;
            set;
        }
        public List<string> GetDetailImage()
        {
            List<string> list = (DetailImage + "").Split('|').ToList();
            list.RemoveAll(b => b.Trim() == "");
            return list;
        }
        /// <summary>
        /// 商品介绍
        /// </summary>
        [CRL.Attribute.Field(Length = 4000)]
        public string Content
        {
            get;
            set;
        }
        /// <summary>
        /// 转换过后的属性值对象
        /// </summary>
        public Dictionary<CRL.Property.PropertyName, CRL.Property.PropertyValue> PropertyObj;

        /// <summary>
        /// 生成自定义数据,放入购物车
        /// </summary>
        /// <returns></returns>
        public virtual string GetTagData()
        {
            return OriginalId.ToString();
        }
    }
}
