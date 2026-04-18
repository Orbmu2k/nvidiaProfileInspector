using System.Threading.Tasks;

namespace nvidiaProfileInspector.Common.Updates
{
    public interface IUpdateInstaller
    {
        Task PrepareAndRunAsync(UpdateRelease release);
    }
}
