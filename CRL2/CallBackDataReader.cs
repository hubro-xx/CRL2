using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL
{
    /// <summary>
    /// DataRead关闭回调方法
    /// </summary>
    /// <returns></returns>
    internal delegate int DataReaderHandler();
    /// <summary>
    /// 可回调取出out参数的DataReader
    /// </summary>
    internal class CallBackDataReader
    {
        System.Data.Common.DbDataReader reader;
        DataReaderHandler handler;
        public CallBackDataReader(System.Data.Common.DbDataReader _reader, DataReaderHandler _handler)
        {
            reader = _reader;
            handler = _handler;
        }
        public List<T> GetData<T>(out int outParame, ParameCollection fieldMapping = null) where T : class,new()
        {
            var data = ObjectConvert.DataReaderToList<T>(reader, false, fieldMapping);
            reader.Close();
            outParame = handler();
            return data;
        }
    }
}
