namespace nvidiaProfileInspector.Common.Updates
{
    public enum UpdatePackageType
    {
        Zip,
        Exe
    }

    public sealed class UpdateAsset
    {
        public UpdateAsset(string name, string downloadUrl, UpdatePackageType packageType)
        {
            Name = name;
            DownloadUrl = downloadUrl;
            PackageType = packageType;
        }

        public string Name { get; }
        public string DownloadUrl { get; }
        public UpdatePackageType PackageType { get; }
    }
}
