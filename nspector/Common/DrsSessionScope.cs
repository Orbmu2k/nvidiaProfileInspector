using nspector.Native.NVAPI2;
using System;
using nvw = nspector.Native.NVAPI2.NvapiDrsWrapper;

namespace nspector.Common
{
    public class DrsSessionScope
    {

        public static volatile IntPtr GlobalSession;

        public static volatile bool HoldSession = true;

        private static object _Sync = new object();


        public static T DrsSession<T>(Func<IntPtr, T> action, bool forceNonGlobalSession = false, bool preventLoadSettings = false)
        {
            lock (_Sync)
            {
                if (!HoldSession || forceNonGlobalSession)
                    return NonGlobalDrsSession<T>(action, preventLoadSettings);


                if (GlobalSession == IntPtr.Zero)
                {

#pragma warning disable CS0420
                    var csRes = nvw.DRS_CreateSession(ref GlobalSession);
#pragma warning restore CS0420

                    if (csRes != NvAPI_Status.NVAPI_OK)
                        throw new NvapiException("DRS_CreateSession", csRes);

                    if (!preventLoadSettings)
                    {
                        var nvRes = nvw.DRS_LoadSettings(GlobalSession);
                        if (nvRes != NvAPI_Status.NVAPI_OK)
                            throw new NvapiException("DRS_LoadSettings", nvRes);
                    }
                }
            }

            if (GlobalSession != IntPtr.Zero)
            {
                return action(GlobalSession);
            }

            throw new Exception(nameof(GlobalSession) + " is Zero!");
        }

        public static void DestroyGlobalSession()
        {
            lock (_Sync)
            {
                if (GlobalSession != IntPtr.Zero)
                {
                    var csRes = nvw.DRS_DestroySession(GlobalSession);
                    GlobalSession = IntPtr.Zero;
                }
            }
        }

        private static T NonGlobalDrsSession<T>(Func<IntPtr, T> action, bool preventLoadSettings = false)
        {
            IntPtr hSession = IntPtr.Zero;
            var csRes = nvw.DRS_CreateSession(ref hSession);
            if (csRes != NvAPI_Status.NVAPI_OK)
                throw new NvapiException("DRS_CreateSession", csRes);

            try
            {
                if (!preventLoadSettings)
                {
                    var nvRes = nvw.DRS_LoadSettings(hSession);
                    if (nvRes != NvAPI_Status.NVAPI_OK)
                        throw new NvapiException("DRS_LoadSettings", nvRes);
                }

                return action(hSession);
            }
            finally
            {
                var nvRes = nvw.DRS_DestroySession(hSession);
                if (nvRes != NvAPI_Status.NVAPI_OK)
                    throw new NvapiException("DRS_DestroySession", nvRes);
            }

        }


    }
}
