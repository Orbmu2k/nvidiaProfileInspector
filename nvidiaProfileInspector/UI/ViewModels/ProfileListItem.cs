namespace nvidiaProfileInspector.UI.ViewModels
{
    using nvidiaProfileInspector.Common;

    public class ProfileListItem
    {
        public ProfileListItem(string profileName, bool isUserDefined)
        {
            ProfileName = profileName;
            IsGlobal = profileName == DrsSettingsService.GlobalProfileName;
            IsUserDefined = !IsGlobal && isUserDefined;
        }

        public string ProfileName { get; }

        public bool IsGlobal { get; }

        public bool IsUserDefined { get; }

        public string ProfileTypeLabel =>
            IsGlobal ? "Global driver profile" :
            IsUserDefined ? "User-created profile" :
            "NVIDIA predefined profile";

        public override string ToString()
        {
            return ProfileName;
        }
    }
}
