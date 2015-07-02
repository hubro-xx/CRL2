using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace CRL.Property
{
    /// <summary>
    /// 属性维护,通过TYPE以区分不同用途
    /// </summary>
    public class PropertyBusiness<TType> : BaseProvider<PropertyName> where TType : class
    {
        public static PropertyBusiness<TType> Instance
        {
            get { return new PropertyBusiness<TType>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        public string CreateTable()
        {
            DBExtend helper = dbHelper;
            PropertyName obj1 = new PropertyName();
            PropertyValue obj2 = new PropertyValue();
            string msg = obj1.CreateTable(helper);
            msg += obj2.CreateTable(helper);
            return msg;
        }
        public void ClearCache()
        {
            nameCache.Clear();
            valueCache.Clear();
        }
        static Dictionary<int, PropertyName> nameCache = new Dictionary<int, PropertyName>();
        static Dictionary<int, PropertyValue> valueCache = new Dictionary<int, PropertyValue>();

        void InItCache()
        {
            if (nameCache.Count == 0)
            {
                #region 初始缓存
                
                DBExtend helper = dbHelper;
                List<PropertyName> list = helper.QueryList<PropertyName>();
                foreach (PropertyName c in list)
                {
                    nameCache.Add(c.Id, c);
                }

                List<PropertyValue> list2 = helper.QueryList<PropertyValue>();
                foreach (PropertyValue c in list2)
                {
                    valueCache.Add(c.Id, c);
                }
                #endregion
            }
        }
        /// <summary>
        /// 添加名称的值
        /// </summary>
        /// <param name="values"></param>
        /// <param name="propertyId"></param>
        public void AddPropertyValue(List<string> values, int propertyId)
        {
            DBExtend helper = dbHelper;
            foreach(string s in values)
            {
                PropertyValue v = new PropertyValue();
                v.PropertyId = propertyId;
                v.Name = s.Trim();
                //helper.Params.Clear();
                int id = helper.InsertFromObj(v);
                v.Id = id;
                valueCache.Add(id, v);
            }
        }
        /// <summary>
        /// 删除一个属性
        /// </summary>
        /// <param name="propertyId"></param>
        public void DeleteProperty(int propertyId)
        {
            DBExtend helper = dbHelper;
            Delete(propertyId);
            helper.Delete<PropertyValue>(b => b.PropertyId == propertyId);
            nameCache.Remove(propertyId);
        }
        /// <summary>
        /// 取得名称对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PropertyName GetName(int id)
        {
            if (!nameCache.ContainsKey(id))
                return null;
            return nameCache[id];
        }
        /// <summary>
        /// 取得值对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PropertyValue GetValue(int id)
        {
            if (!valueCache.ContainsKey(id))
                return null;
            return valueCache[id];
        }
        /// <summary>
        /// 取得分类下的名称
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<PropertyName> GetPropertyNames(string categoryCode, PropertyType type)
        {
            InItCache();
            List<PropertyName> list = new List<PropertyName>();
            foreach (KeyValuePair<int, PropertyName> v in nameCache)
            {
                if (v.Value.CategoryCode == categoryCode && v.Value.Type == type)
                {
                    list.Add(v.Value);
                }
            }
            return list.OrderByDescending(b => b.Sort).ToList();
        }
        /// <summary>
        /// 根据属性ID取所有值
        /// </summary>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        public List<PropertyValue> GetPropertyValues(int propertyId)
        {
            InItCache();
            List<PropertyValue> list = new List<PropertyValue>();
            foreach (KeyValuePair<int, PropertyValue> v in valueCache)
            {
                if (v.Value.PropertyId == propertyId)
                {
                    list.Add(v.Value);
                }
            }
            return list;
        }

        /// <summary>
        /// 键值转为STRING
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public string PropertyToString(Dictionary<int,int> keyValue)
        {
            string str = "";
            foreach (KeyValuePair<int, int> v in keyValue)
            {
                str += string.Format("{0}:{1}|", v.Key, v.Value);
            }
            str = str.Substring(0, str.Length - 1);
            return str;
        }
        
        /// <summary>
        /// 将字符串形式转为对象
        /// like 1:11|2:22
        /// </summary>
        /// <param name="propertyString"></param>
        /// <returns></returns>
        public Dictionary<PropertyName,PropertyValue> PropertyFromString(string propertyString)
        {
            Dictionary<PropertyName, PropertyValue> list = new Dictionary<PropertyName, PropertyValue>();
            if (string.IsNullOrEmpty(propertyString))
                return list;
            string[] arry = propertyString.Split('|');
            foreach (string s in arry)
            {
                if (s == "")
                    continue;
                string[] arry1 = s.Split(':');
                if (arry1.Length < 2)
                    continue;
                int nameId = Convert.ToInt32(arry1[0]);
                int valueId = Convert.ToInt32(arry1[1]);
                PropertyName name;
                if (nameCache.ContainsKey(nameId))
                {
                    name = nameCache[nameId];
                }
                else
                {
                    name = new PropertyName() { Id = nameId, Name = "未知" };
                }
                PropertyValue value ;
                if (valueCache.ContainsKey(valueId))
                {
                    value = valueCache[valueId];
                }
                else
                {
                    value = new PropertyValue() { Id = valueId, Name = "未知", PropertyId = nameId };
                }
                list.Add(name,value);
            }
            return list;
        }

        #region 规格
        /// <summary>
        /// 返回分组的形式,规格选择
        /// </summary>
        /// <param name="skus"></param>
        /// <param name="styles"></param>
        /// <returns></returns>
        public List<TypeItem> GetStylesByGroup(List<SKUItem> skus, out List<StyleItem> styles)
        {
            //按规格种类列表
            List<TypeItem> items = new List<TypeItem>();
            styles = new List<StyleItem>();
            TypeItem item;
            Dictionary<int, List<TypeValue>> all = new Dictionary<int, List<TypeValue>>();
            foreach (SKUItem skuItem in skus)
            {
                string sku = skuItem.SKU;
                int innerStyleId = skuItem.StyleId;
                if (string.IsNullOrEmpty(sku))
                {
                    continue;
                }
                string[] arry = sku.Split('|');
                foreach (string s in arry)
                {
                    string[] a = s.Split(':');
                    int typeId = int.Parse(a[0]);
                    int valueId = int.Parse(a[1]);
                    if (!all.ContainsKey(typeId))
                    {
                        all.Add(typeId, new List<TypeValue>());
                    }

                    bool exists = false;
                    foreach (TypeValue a1 in all[typeId])
                    {
                        if (a1.Code == typeId + ":" + valueId)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        TypeValue v = new TypeValue(typeId, valueId, valueCache[valueId].Name);
                        all[typeId].Insert(0, v);
                    }
                }
                int stock = skuItem.Num;
                styles.Insert(0, new StyleItem() { StyleId = innerStyleId, Code = sku, Num = stock });
            }
            foreach (KeyValuePair<int, List<TypeValue>> entry in all)
            {
                item = new TypeItem();
                item.Name = nameCache[entry.Key].Name;
                item.Id = entry.Key;
                item.Values = entry.Value;
                items.Add(item);
            }
            return items;
        }
        #region object
        /// <summary>
        /// 原始SKU信息
        /// </summary>
        public class SKUItem
        {
            public int StyleId;
            /// <summary>
            /// sku串 1:11|2:22
            /// </summary>
            public string SKU;
            /// <summary>
            /// 数量
            /// </summary>
            public int Num;
        }
        /// <summary>
        /// 种类规格对应的值
        /// </summary>
        public class TypeValue
        {
            public TypeValue()
            {
            }
            public TypeValue(int typeId, int vId, string vName)
            {
                Id = vId;
                Name = vName;
                Code = typeId + ":" + vId;
            }
            public int Id
            {
                get;
                set;
            }
            public string Name
            {
                get;
                set;
            }
            /// <summary>
            /// 当前编码
            /// </summary>
            public string Code
            {
                get;
                set;
            }
        }
        /// <summary>
        /// 每个规格
        /// </summary>
        public class StyleItem
        {
            public int StyleId
            {
                get;
                set;
            }
            /// <summary>
            /// 样式唯一编码
            /// </summary>
            public string Code
            {
                get;
                set;
            }
            /// <summary>
            /// 库存
            /// </summary>
            public int Num
            {
                get;
                set;
            }
        }
        /// <summary>
        /// 每个规格种类对应的可选项
        /// 颜色:红,黄,蓝
        /// </summary>
        public class TypeItem
        {
            public int Id
            {
                get;
                set;
            }

            /// <summary>
            /// 种类名称
            /// </summary>
            public string Name
            {
                get;
                set;
            }
            /// <summary>
            /// 值集合
            /// </summary>
            public List<TypeValue> Values;
        }
        #endregion
        #endregion
    }
}
