namespace nvidiaProfileInspector.Common.Updates
{
    public sealed class UpdateCheckResult
    {
        private UpdateCheckResult(bool succeeded, bool isUpdateAvailable, UpdateRelease latestRelease, string statusMessage, string errorMessage)
        {
            Succeeded = succeeded;
            IsUpdateAvailable = isUpdateAvailable;
            LatestRelease = latestRelease;
            StatusMessage = statusMessage;
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; }
        public bool IsUpdateAvailable { get; }
        public UpdateRelease LatestRelease { get; }
        public string StatusMessage { get; }
        public string ErrorMessage { get; }
        public bool CanInstall => IsUpdateAvailable && LatestRelease?.IsInstallable == true;

        public static UpdateCheckResult Available(UpdateRelease release)
        {
            var message = release?.IsInstallable == true
                ? "A newer version is ready to install."
                : "A newer version is available, but it does not contain a supported update package.";
            return new UpdateCheckResult(true, true, release, message, null);
        }

        public static UpdateCheckResult UpToDate(UpdateRelease release)
        {
            return new UpdateCheckResult(true, false, release, "You are already on the latest version for this channel.", null);
        }

        public static UpdateCheckResult Unavailable(string message)
        {
            return new UpdateCheckResult(false, false, null, message, message);
        }
    }
}
