namespace nspector.Common;

public class NvapiException:System.Exception
{
    public readonly nspector.Native.NVAPI2.NvAPI_Status Status;

    public NvapiException(string function,nspector.Native.NVAPI2.NvAPI_Status status)
        :base(function+" failed: "+status)=>this.Status=status;
}

public class NvapiAddApplicationException:NvapiException
{
    public readonly string ApplicationName;

    public NvapiAddApplicationException(string applicationName)
        :base("DRS_CreateApplication",nspector.Native.NVAPI2.NvAPI_Status.NVAPI_EXECUTABLE_ALREADY_IN_USE)
        =>this.ApplicationName=applicationName;
}