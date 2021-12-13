using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D.Core
{
    public interface IStateUiWriter
    {
        void Clear();
        void WriteState(string state);
        void WriteAdditionalData(string data);
    }
}
