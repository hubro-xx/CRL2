using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CRL
{
    public partial class DBExtend
    {
        #region insert
        /// <summary>
        /// 批量插入,并指定是否保持自增主键
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="details"></param>
        /// <param name="keepIdentity"></param>
        public void BatchInsert<TItem>(List<TItem> details,bool keepIdentity=false) where TItem : IModel,new()
        {
            CheckTableCreated<TItem>();
            if (details.Count == 0)
                return;
            foreach (TItem item in details)
            {
                CheckData(item);
            }
            _DBAdapter.BatchInsert(helper, details, keepIdentity);
            //var type = typeof(TItem);
            //if (TypeCache.ModelKeyCache.ContainsKey(type))
            //{
            //    string key = TypeCache.ModelKeyCache[type];
            //    foreach (var item in details)
            //    {
            //        MemoryDataCache.UpdateCacheItem(key, item);
            //    }
            //}
            var updateModel = MemoryDataCache.GetCacheTypeKey(typeof(TItem));
            foreach (var item in details)
            {
                foreach (var key in updateModel)
                {
                    MemoryDataCache.UpdateCacheItem(key, item, null);
                }
            }
        }
       
        /// <summary>
        /// 单个插入
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int InsertFromObj<TItem>(TItem obj) where TItem : IModel, new()
        {
            CheckTableCreated<TItem>();
            var primaryKey = TypeCache.GetTable(obj.GetType()).PrimaryKey;
            CheckData(obj);
            var index = _DBAdapter.InsertObject(obj, helper);
            primaryKey.SetValue(obj, index);
            ClearParame();
            var clone = obj.Clone();
            obj.OriginClone = clone as TItem;
            //var type = typeof(TItem);
            //if (TypeCache.ModelKeyCache.ContainsKey(type))
            //{
            //    string key = TypeCache.ModelKeyCache[type];
            //    MemoryDataCache.UpdateCacheItem(key, obj);
            //}
            UpdateCacheItem<TItem>(obj, null);
            return index;
        }
        #endregion
    }
}
