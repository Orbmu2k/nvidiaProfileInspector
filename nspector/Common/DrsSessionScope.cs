#region

using nvw=nspector.Native.NVAPI2.NvapiDrsWrapper;

#endregion

namespace nspector.Common;

public class DrsSessionScope
{
    public static volatile System.IntPtr GlobalSession;

    public static volatile bool HoldSession=true;

    static readonly object _Sync=new object();


    public static T DrsSession<T>(System.Func<System.IntPtr,T> action,bool forceNonGlobalSession=false,
        bool                                                   preventLoadSettings=false)
    {
        lock(DrsSessionScope._Sync)
        {
            if(!DrsSessionScope.HoldSession||forceNonGlobalSession)
            {
                return DrsSessionScope.NonGlobalDrsSession(action,preventLoadSettings);
            }


            if(DrsSessionScope.GlobalSession==System.IntPtr.Zero)
            {
            #pragma warning disable CS0420
                var csRes=nvw.DRS_CreateSession(ref DrsSessionScope.GlobalSession);
            #pragma warning restore CS0420

                if(csRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
                {
                    throw new NvapiException("DRS_CreateSession",csRes);
                }

                if(!preventLoadSettings)
                {
                    var nvRes=nvw.DRS_LoadSettings(DrsSessionScope.GlobalSession);
                    if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
                    {
                        throw new NvapiException("DRS_LoadSettings",nvRes);
                    }
                }
            }
        }

        if(DrsSessionScope.GlobalSession!=System.IntPtr.Zero)
        {
            return action(DrsSessionScope.GlobalSession);
        }

        throw new System.Exception(nameof(DrsSessionScope.GlobalSession)+" is Zero!");
    }

    public static void DestroyGlobalSession()
    {
        lock(DrsSessionScope._Sync)
        {
            if(DrsSessionScope.GlobalSession!=System.IntPtr.Zero)
            {
                var csRes=nvw.DRS_DestroySession(DrsSessionScope.GlobalSession);
                DrsSessionScope.GlobalSession=System.IntPtr.Zero;
            }
        }
    }

    static T NonGlobalDrsSession<T>(System.Func<System.IntPtr,T> action,bool preventLoadSettings=false)
    {
        var hSession=System.IntPtr.Zero;
        var csRes   =nvw.DRS_CreateSession(ref hSession);
        if(csRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_CreateSession",csRes);
        }

        try
        {
            if(!preventLoadSettings)
            {
                var nvRes=nvw.DRS_LoadSettings(hSession);
                if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
                {
                    throw new NvapiException("DRS_LoadSettings",nvRes);
                }
            }

            return action(hSession);
        }
        finally
        {
            var nvRes=nvw.DRS_DestroySession(hSession);
            if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
            {
                throw new NvapiException("DRS_DestroySession",nvRes);
            }
        }
    }
}