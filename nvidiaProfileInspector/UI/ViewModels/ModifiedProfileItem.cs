namespace nvidiaProfileInspector.UI.ViewModels
{
    public class ModifiedProfileItem
    {
        public ModifiedProfileItem(string profileName, bool isUserDefined)
        {
            ProfileName = profileName;
            IsUserDefined = isUserDefined;
        }

        public string ProfileName { get; }

        public bool IsUserDefined { get; }

        public string ProfileTypeLabel => IsUserDefined ? "User-created profile" : "NVIDIA predefined profile";

        public override string ToString()
        {
            return ProfileName;
        }
    }
}
