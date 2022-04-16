using System;
using System.Collections.Generic;

namespace nspector.Common.Import;

[Serializable]
public class Profile
{
    public List<string> Executeables = new();
    public string ProfileName = "";
    public List<ProfileSetting> Settings = new();
}