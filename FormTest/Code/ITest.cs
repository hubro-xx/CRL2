using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormTest.Code
{
    public abstract class ITest : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }
        public int Data { get; set; }
        public abstract void Do(int data);
        public long TotalTime
        {
            get;
            set;
        }
    }
}
