using System;
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

    public class NvapiAddApplicationException : NvapiException
    {
        public readonly string ApplicationName;

        public NvapiAddApplicationException(string applicationName)
            : base("DRS_CreateApplication", NvAPI_Status.NVAPI_EXECUTABLE_ALREADY_IN_USE)
        {
            ApplicationName = applicationName;
        }

    }
}
