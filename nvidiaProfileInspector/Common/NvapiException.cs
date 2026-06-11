using nvidiaProfileInspector.Native.NVAPI2;
using System;
using nvidiaProfileInspector.Localization;

namespace nvidiaProfileInspector.Common
{
    public class NvapiException : Exception
    {
        public readonly NvAPI_Status Status;

        public NvapiException(string function, NvAPI_Status status)
            : base(string.Format(UIStrings.NvapiFunctionFailed, function, status))
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
