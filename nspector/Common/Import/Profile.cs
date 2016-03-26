using System;
using System.Collections.Generic;

namespace nspector.Common.Import
{
    [Serializable]
    public class Profile
    {
        public string ProfileName = "";
        public List<string> Executeables = new List<string>();
        public List<ProfileSetting> Settings = new List<ProfileSetting>();
    }
}