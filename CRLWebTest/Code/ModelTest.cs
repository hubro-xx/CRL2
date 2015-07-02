using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTest.Code
{
    [CRL.Attribute.Table(TableName="table1")]//映射表名为table1
    public class ModelTest:CRL.IModelBase
    {
        public override string CheckData()
        {
            if (string.IsNullOrEmpty(BarCode))
            {
                return "BarCode不能为空";
            }
            return base.CheckData();
        }

        [CRL.Attribute.Field(FieldIndexType = CRL.Attribute.FieldIndexType.非聚集唯一)]//非聚集唯一索引
        public string No
        {
            get;
            set;
        }
        [CRL.Attribute.Field(FieldIndexType = CRL.Attribute.FieldIndexType.非聚集)]//非聚集索引
        public string DataType
        {
            get;
            set;
        }
        [CRL.Attribute.Field(Length=100)]//长度为100
        public string BarCode
        {
            get;
            set;
        }
        /// <summary>
        /// 自动关联查询
        /// 值等同为 select Name as ProductName from ProductData where BarCode=ModelTest.ModelTest
        /// </summary>
        [CRL.Attribute.Field(ConstraintType = typeof(ProductData), ConstraintField = "$BarCode=BarCode", ConstraintResultField = "Name")]
        public string ProductName
        {
            get;
            set;
        }
        [CRL.Attribute.Field(ColumnType = "decimal(18,4)")]//强制指定字段类型,如果不指定则默认为decimal(18,2)
        public decimal Price
        {

            get;
            set;
        }
        /// <summary>
        /// 虚拟字段,等同于 year(addtime) as Year
        /// </summary>
        [CRL.Attribute.Field(VirtualField="year(addtime)")]
        public int Year
        {
            get;
            set;
        }
    }
}