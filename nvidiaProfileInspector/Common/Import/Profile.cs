using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace nvidiaProfileInspector.Common.Import
{
    [Serializable]
    public class Profile
    {
        public string ProfileName = "";
        public List<string> Executeables = new List<string>();
        public List<ProfileSetting> Settings = new List<ProfileSetting>();

        // Optional per-executable "find file" (NVAPI fileInFolder) mappings. Kept as a
        // separate, additive element so existing .nip files (which only list executable
        // names) still deserialize unchanged.
        public List<ExecutableFindFile> ExecutableFindFiles = new List<ExecutableFindFile>();
    }

    [Serializable]
    public class ExecutableFindFile
    {
        [XmlAttribute]
        public string Executable;

        [XmlAttribute]
        public string FindFile;
    }
}
