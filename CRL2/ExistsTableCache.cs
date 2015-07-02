using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL
{
    [Serializable]
    internal class Table
    {
        public override string ToString()
        {
            return Name;
        }
        public string Name
        {
            get;
            set;
        }
        List<string> fields = new List<string>();
        public List<string> Fields
        {
            get { return fields; }
            set { fields = value; }
        }
        /// <summary>
        /// 是否需要检查字段
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        [NonSerializedAttribute]
        public bool ColumnChecked;
    }
    [Serializable]
    internal class ExistsTableCache
    {
        #region 属性
        /// <summary>
        /// 数据库,表
        /// </summary>
        Dictionary<string, Dictionary<string, Table>> tables = new Dictionary<string, Dictionary<string, Table>>();

        public Dictionary<string, Dictionary<string, Table>> Tables
        {
            get { return tables; }
            set { tables = value; }
        }
        #endregion
        static object lockObj = new object();
        /// <summary>
        /// 初始所有表
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tables"></param>
        public void InitTable(string dbName, List<string> tables)
        {
            var list = new Dictionary<string, Table>();
            foreach(var item in tables)
            {
                list.Add(item.ToUpper(), new Table() { Name = item.ToUpper() });
            }
            Tables[dbName] = list;
            Save();
        }
        /// <summary>
        /// 获取一个表
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Table GetTable(string dbName, Attribute.TableAttribute table)
        {
            var name = table.TableName.ToUpper();
            if (Tables[dbName].ContainsKey(name))
            {
                return Tables[dbName][name];
            }
            return null;
        }
        /// <summary>
        /// 保存表字段
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="table"></param>
        public void SaveTable(string dbName, Attribute.TableAttribute table)
        {
            var tableName = table.TableName.ToUpper();
            var fields = table.Fields;
            lock (lockObj)
            {
                var fields2 = new List<string>();
                fields.ForEach(b =>
                {
                    fields2.Add(b.Name.ToUpper());
                });
                var tb = new Table() { Name = tableName, Fields = fields2 };
                Tables[dbName].Remove(tableName);
                Tables[dbName].Add(tableName, tb);
            }
            Save();
        }
        /// <summary>
        /// 检查字段
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public List<Attribute.FieldAttribute> CheckFieldExists(string dbName, Attribute.TableAttribute table)
        {
            var fields = table.Fields;
            var tableName = table.TableName.ToUpper();
            var returns = new List<Attribute.FieldAttribute>();
            var tb = Tables[dbName][tableName];
            if (tb.ColumnChecked)
            {
                return returns;
            }
            if (tb.Fields.Count==0)//首次不检查
            {
                SaveTable(dbName, table);
                return returns;
            }
            //检查字段是否一致
            foreach (var item in fields)
            {
                if (item.FieldType != Attribute.FieldType.数据库字段)
                    continue;
                if (!tb.Fields.Contains(item.Name.ToUpper()))
                {
                    returns.Add(item);
                }
            }
            tb.ColumnChecked = true;
            if (returns.Count > 0)
            {
                var fields2 = new List<string>();
                fields.ForEach(b =>
                {
                    fields2.Add(b.Name.ToUpper());
                });
                tb.Fields = fields2;
                Save();
            }
            return returns;
        }
            
        
        private static ExistsTableCache instance;
        /// <summary>
        /// 实例
        /// </summary>
        internal static ExistsTableCache Instance
        {
            get
            {
                if (instance == null)
                    instance = FromFile();
                return instance;
            }
            set
            {
                instance = value;
            }
        }
        const string confgiFile = @"\TableCache.config";
        public static ExistsTableCache FromFile()
        {
            var file = CoreHelper.RequestHelper.GetFilePath(confgiFile);
            ExistsTableCache cache = null;
            if (System.IO.File.Exists(file))
            {
                try
                {
                    cache = CoreHelper.SerializeHelper.BinaryDeserialize<ExistsTableCache>(file);
                    CoreHelper.EventLog.Log("读取CoreConfig");
                }
                catch { }
            }
            if (cache == null)
                cache = new ExistsTableCache();
            return cache;
        }
        public void Save()
        {
            var file = CoreHelper.RequestHelper.GetFilePath(confgiFile);
            lock (lockObj)
            {
                CoreHelper.SerializeHelper.BinarySerialize(this, file);
                //CoreHelper.EventLog.Log("保存CoreConfig");
            }
        }
    }
}
