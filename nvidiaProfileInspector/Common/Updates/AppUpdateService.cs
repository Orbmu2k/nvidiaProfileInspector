using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace nvidiaProfileInspector.Common.Updates
{
    public sealed class AppUpdateService
    {
        private readonly GitHubReleaseClient _releaseClient;
        private readonly IUpdateInstaller _installer;

        public AppUpdateService()
            : this(new GitHubReleaseClient(GetCurrentVersion()), new InplaceUpdateInstaller())
        {
        }

        public AppUpdateService(GitHubReleaseClient releaseClient, IUpdateInstaller installer)
        {
            _releaseClient = releaseClient;
            _installer = installer;
        }

        public async Task<UpdateCheckResult> CheckAsync(UpdateChannel channel)
        {
            try
            {
                var releases = await _releaseClient.GetReleasesAsync();
                var latestRelease = releases
                    .Where(release => channel == UpdateChannel.Prerelease || !release.IsPreRelease)
                    .OrderByDescending(release => release.Version)
                    .FirstOrDefault();

                if (latestRelease == null)
                    return UpdateCheckResult.Unavailable("No release information is available.");

                return latestRelease.Version > GetCurrentVersion()
                    ? UpdateCheckResult.Available(latestRelease)
                    : UpdateCheckResult.UpToDate(latestRelease);
            }
            catch
            {
                return UpdateCheckResult.Unavailable("Could not read release information.");
            }
        }

        public Task PrepareInstallAsync(UpdateRelease release)
        {
            if (release == null || !release.IsInstallable)
                throw new InvalidOperationException("The selected release does not contain a downloadable update package.");

            return _installer.PrepareAndRunAsync(release);
        }

        public static Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
