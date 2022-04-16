using System.Security.Principal;

namespace nspector.Common.Helper;

public static class AdminHelper
{
    static AdminHelper()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static bool IsAdmin { get; }
}