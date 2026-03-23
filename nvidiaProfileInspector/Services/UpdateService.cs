namespace nvidiaProfileInspector.Services
{
    using System.Threading.Tasks;

    public interface IUpdateService
    {
        Task<bool> CheckForUpdatesAsync();
    }

    public class UpdateService : IUpdateService
    {
        public async Task<bool> CheckForUpdatesAsync()
        {
            return await Common.Helper.GithubVersionHelper.IsUpdateAvailableAsync();
        }
    }
}
