using System;

namespace nvidiaProfileInspector.Common.CustomSettings
{
    [Serializable]
    public class CustomSettingValue
    {
        public uint SettingValue
        {
            get { return Convert.ToUInt32(HexValue.Trim(), 16); }
        }

        public string UserfriendlyName { get; set; }

        public string HexValue { get; set; }

    }
}