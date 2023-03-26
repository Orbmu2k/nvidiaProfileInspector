using nspector.Common.Helper;
using nspector.Native.NVAPI2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace nspector.Common
{
    internal class DrsDecrypterService : DrsSettingsServiceBase
    {

        private static readonly byte[] _InternalSettingsKey = new byte[] {
            0x2f, 0x7c, 0x4f, 0x8b, 0x20, 0x24, 0x52, 0x8d, 0x26, 0x3c, 0x94, 0x77, 0xf3, 0x7c, 0x98, 0xa5,
            0xfa, 0x71, 0xb6, 0x80, 0xdd, 0x35, 0x84, 0xba, 0xfd, 0xb6, 0xa6, 0x1b, 0x39, 0xc4, 0xcc, 0xb0,
            0x7e, 0x95, 0xd9, 0xee, 0x18, 0x4b, 0x9c, 0xf5, 0x2d, 0x4e, 0xd0, 0xc1, 0x55, 0x17, 0xdf, 0x18,
            0x1e, 0x0b, 0x18, 0x8b, 0x88, 0x58, 0x86, 0x5a, 0x1e, 0x03, 0xed, 0x56, 0xfb, 0x16, 0xfe, 0x8a,
            0x01, 0x32, 0x9c, 0x8d, 0xf2, 0xe8, 0x4a, 0xe6, 0x90, 0x8e, 0x15, 0x68, 0xe8, 0x2d, 0xf4, 0x40,
            0x37, 0x9a, 0x72, 0xc7, 0x02, 0x0c, 0xd1, 0xd3, 0x58, 0xea, 0x62, 0xd1, 0x98, 0x36, 0x2b, 0xb2,
            0x16, 0xd5, 0xde, 0x93, 0xf1, 0xba, 0x74, 0xe3, 0x32, 0xc4, 0x9f, 0xf6, 0x12, 0xfe, 0x18, 0xc0,
            0xbb, 0x35, 0x79, 0x9c, 0x6b, 0x7a, 0x23, 0x7f, 0x2b, 0x15, 0x9b, 0x42, 0x07, 0x1a, 0xff, 0x69,
            0xfb, 0x9c, 0xbd, 0x23, 0x97, 0xa8, 0x22, 0x63, 0x8f, 0x32, 0xc8, 0xe9, 0x9b, 0x63, 0x1c, 0xee,
            0x2c, 0xd9, 0xed, 0x8d, 0x3a, 0x35, 0x9c, 0xb1, 0x60, 0xae, 0x5e, 0xf5, 0x97, 0x6b, 0x9f, 0x20,
            0x8c, 0xf7, 0x98, 0x2c, 0x43, 0x79, 0x95, 0x1d, 0xcd, 0x46, 0x36, 0x6c, 0xd9, 0x67, 0x20, 0xab,
            0x41, 0x22, 0x21, 0xe5, 0x55, 0x82, 0xf5, 0x27, 0x20, 0xf5, 0x08, 0x07, 0x3f, 0x6d, 0x69, 0xd9,
            0x1c, 0x4b, 0xf8, 0x26, 0x03, 0x6e, 0xb2, 0x3f, 0x1e, 0xe6, 0xca, 0x3d, 0x61, 0x44, 0xb0, 0x92,
            0xaf, 0xf0, 0x88, 0xca, 0xe0, 0x5f, 0x5d, 0xf4, 0xdf, 0xc6, 0x4c, 0xa4, 0xe0, 0xca, 0xb0, 0x20,
            0x5d, 0xc0, 0xfa, 0xdd, 0x9a, 0x34, 0x8f, 0x50, 0x79, 0x5a, 0x5f, 0x7c, 0x19, 0x9e, 0x40, 0x70,
            0x71, 0xb5, 0x45, 0x19, 0xb8, 0x53, 0xfc, 0xdf, 0x24, 0xbe, 0x22, 0x1c, 0x79, 0xbf, 0x42, 0x89 };

        public DrsDecrypterService(DrsSettingsMetaService metaService) : base(metaService)
        {
            try
            {
                CreateInternalSettingMap();
            }
            catch { }
        }

        private uint GetDwordFromKey(uint offset)
        {
            var bytes = new byte[4];
            bytes[0] = _InternalSettingsKey[(offset + 0) % 256];
            bytes[1] = _InternalSettingsKey[(offset + 1) % 256];
            bytes[2] = _InternalSettingsKey[(offset + 2) % 256];
            bytes[3] = _InternalSettingsKey[(offset + 3) % 256];
            return BitConverter.ToUInt32(bytes, 0);
        }

        public uint DecryptDwordValue(uint orgValue, uint settingId)
        {
            var keyOffset = (settingId << 1);
            var key = GetDwordFromKey(keyOffset);
            return orgValue ^ key;
        }

        public string DecryptStringValue(byte[] rawData, uint settingId)
        {
            var keyOffset = (settingId << 1);
            for (uint i = 0; i < (uint)rawData.Length; i++)
            {
                rawData[i] ^= _InternalSettingsKey[(keyOffset + i) % 256];
            }
            return Encoding.Unicode.GetString(rawData).Trim('\0');
        }

        public void DecryptSettingIfNeeded(string profileName, ref NVDRS_SETTING setting)
        {
            if (setting.isPredefinedValid == 1)
            {
                if (IsInternalSetting(profileName, setting.settingId))
                {
                    if (setting.settingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
                    {
                        setting.predefinedValue.stringValue = DecryptStringValue(setting.predefinedValue.rawData, setting.settingId);
                        if (setting.isCurrentPredefined == 1)
                            setting.currentValue.stringValue = DecryptStringValue(setting.currentValue.rawData, setting.settingId);
                    }
                    else if (setting.settingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
                    {
                        setting.predefinedValue.dwordValue = DecryptDwordValue(setting.predefinedValue.dwordValue, setting.settingId);
                        if (setting.isCurrentPredefined == 1)
                            setting.currentValue.dwordValue = DecryptDwordValue(setting.currentValue.dwordValue, setting.settingId);
                    }
                }
            }
        }

        private string FormatInternalSettingKey(string profileName, uint settingId)
        {
            return profileName + settingId.ToString("X8").ToLowerInvariant();
        }

        public bool IsInternalSetting(string profileName, uint settingId)
        {
            return _InternalSettings.Contains(FormatInternalSettingKey(profileName, settingId));
        }

        private HashSet<string> _InternalSettings = new HashSet<string>();

        private void CreateInternalSettingMap()
        {
            string tmpfile = TempFile.GetTempFileName();

            try
            {
                DrsSession((hSession) =>
                {
                    SaveSettingsFileEx(hSession, tmpfile);
                });

                if (File.Exists(tmpfile))
                {
                    var lines = File.ReadAllLines(tmpfile);

                    _InternalSettings = new HashSet<string>();

                    var paProfile = "Profile\\s\\\"(?<profileName>.*?)\\\"";
                    var rxProfile = new Regex(paProfile, RegexOptions.Compiled);

                    var paSetting = "ID_0x(?<sid>[0-9a-fA-F]+)\\s\\=.*?InternalSettingFlag\\=V0";
                    var rxSetting = new Regex(paSetting, RegexOptions.Compiled);

                    var currentProfileName = "";
                    for (int i = 0; i < lines.Length; i++)
                    {
                        foreach (Match ms in rxProfile.Matches(lines[i]))
                        {
                            currentProfileName = ms.Result("${profileName}");
                        }
                        foreach (Match ms in rxSetting.Matches(lines[i]))
                        {
                            _InternalSettings.Add(currentProfileName + ms.Result("${sid}"));
                        }
                    }
                }
            }
            finally
            {
                if (File.Exists(tmpfile))
                    File.Delete(tmpfile);
            }
        }
    }
}