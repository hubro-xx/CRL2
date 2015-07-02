using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.FavoriteProduct
{
    public class IFavoriteProduct : FavoriteProduct
    {
    }
    [Attribute.Table(TableName = "FavoriteProduct")]
    public class FavoriteProduct:IModelBase
    {
        public override string CheckData()
        {
            return "";
        }
        /// <summary>
        /// 会员号
        /// </summary>
        [Attribute.Field(FieldIndexType = Attribute.FieldIndexType.非聚集)]
        public string UserId
        {
            get;
            set;
        }
        /// <summary>
        /// 产品编号
        /// </summary>
        public int ProductId
        {
            get;
            set;
        }
    }
}
