namespace nspector.Common.Helper;

public static class AdminHelper
{
    static AdminHelper()
    {
        var identity =System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal=new System.Security.Principal.WindowsPrincipal(identity);
        AdminHelper.IsAdmin=principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    public static bool IsAdmin
    {
        get;
    }
}