using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using nvidiaProfileInspector.Localization;

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
                    return UpdateCheckResult.Unavailable(UIStrings.NoReleaseInformation);

                return latestRelease.Version > GetCurrentVersion()
                    ? UpdateCheckResult.Available(latestRelease)
                    : UpdateCheckResult.UpToDate(latestRelease);
            }
            catch
            {
                return UpdateCheckResult.Unavailable(UIStrings.CouldNotReadReleaseInformation);
            }
        }

        public Task PrepareInstallAsync(UpdateRelease release)
        {
            if (release == null || !release.IsInstallable)
                throw new InvalidOperationException(UIStrings.ReleaseHasNoDownloadablePackage);

            return _installer.PrepareAndRunAsync(release);
        }

        public static Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
