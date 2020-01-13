using System.Security.Principal;

namespace nspector.Common.Helper
{
    public static class AdminHelper
    {
        private static bool isAdmin = false;
        static AdminHelper()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsAdmin
        {
            get { return isAdmin; }
        }
    }
}
