using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nspector.Native.NVAPI2;

namespace nspector.Common
{
    public class NvapiException : Exception
    {
        public readonly NvAPI_Status Status;
        
        public NvapiException(string function, NvAPI_Status status)
            : base(function + " failed: " + status)
        {
            Status = status;
        }

    }
}
