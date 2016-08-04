using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using nspector.Native.NvApi.DriverSettings;
using nspector.Native.NVAPI2;

namespace nspector.Common.Meta
{
    internal class NvD3dUmxSettingMetaService : ISettingMetaService
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct NvD3dUmxName
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string settingName;

            public uint settingId;
            public uint unknown;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NvD3dUmxNameList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public NvD3dUmxName[] settingNames;
        }

        private static int FindOffset(byte[] bytes, byte[] pattern, int offset = 0, byte? wildcard = null)
        {
            for (int i = offset; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch == true; j++)
                    {
                        if (bytes[i + j] != pattern[j] && ((wildcard.HasValue && wildcard != pattern[j]) || !wildcard.HasValue))
                        {
                            ismatch = false;
                            break;
                        }
                    }
                    if (ismatch)
                        return i;

                }
            }
            return -1;
        }

        private static List<NvD3dUmxName> ParseNvD3dUmxNames(string filename)
        {
            if (!File.Exists(filename)) return null;

            var bytes = File.ReadAllBytes(filename);

            var runtimeNameOffset = FindOffset(bytes, new byte[] { 0x52, 0x75, 0x6E, 0x54, 0x69, 0x6D, 0x65, 0x4E, 0x61, 0x6D, 0x65 });
            if (runtimeNameOffset > -1)
            {
                var _2ddNotesOffset = FindOffset(bytes, new byte[] { 0x32, 0x44, 0x44, 0x5F, 0x4E, 0x6F, 0x74, 0x65, 0x73 });
                if (_2ddNotesOffset > -1)
                {
                    var itemSize = Marshal.SizeOf(typeof(NvD3dUmxName));
                    var startOffset = runtimeNameOffset - itemSize;
                    var endOffset = _2ddNotesOffset + itemSize;
                    var tableLength = endOffset - startOffset;
                    
                    var bufferSize = Marshal.SizeOf(typeof(NvD3dUmxNameList));
                    if (tableLength > bufferSize)
                    {
                        tableLength = bufferSize;
                    }

                    var itemCount = tableLength / itemSize;
                    var buffer = new byte[bufferSize];
                    Buffer.BlockCopy(bytes, startOffset, buffer, 0, tableLength);

                    var poBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        var nvD3dUmxNames = (NvD3dUmxNameList)Marshal.PtrToStructure(
                            poBuffer.AddrOfPinnedObject(),
                            typeof(NvD3dUmxNameList));

                        var result = new List<NvD3dUmxName>();
                        for (int i = 0; i < itemCount; i++)
                        {
                            result.Add(nvD3dUmxNames.settingNames[i]);
                        }
                        return result;

                    }
                    finally
                    {
                        poBuffer.Free();
                    }
                }
            }

            return null;
        }

        private readonly List<NvD3dUmxName> _SettingNames;

        public NvD3dUmxSettingMetaService()
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var nvD3dUmxPath = Path.Combine(systemPath, "nvd3dumx.dll");
            _SettingNames = ParseNvD3dUmxNames(nvD3dUmxPath);
        }

        public Type GetSettingEnumType(uint settingId)
        {
            return null;
        }

        public NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
        {
            return null;
        }

        public string GetSettingName(uint settingId)
        {
            if (_SettingNames != null)
            {
                var setting = _SettingNames.FirstOrDefault(s => s.settingId == settingId);
                return setting.settingId != 0 ? setting.settingName : null;
            }
            return null;
        }

        public uint? GetDwordDefaultValue(uint settingId)
        {
            return null;
        }

        public string GetStringDefaultValue(uint settingId)
        {
            return null;
        }

        public List<SettingValue<string>> GetStringValues(uint settingId)
        {
            return null;
        }
        
        public List<SettingValue<uint>> GetDwordValues(uint settingId)
        {
            return null;
        }

        public List<uint> GetSettingIds()
        {
            if (_SettingNames != null)
            {
                return _SettingNames.Select(s => s.settingId).ToList();
            }
            return null;
        }
        
        public string GetGroupName(uint settingId)
        {
            if (_SettingNames != null)
            {
                var setting = _SettingNames.FirstOrDefault(s => s.settingId == settingId);
                return setting.settingId != 0 ? "7 - Stereo" : null;
            }

            return null;
        }

        public SettingMetaSource Source
        {
            get { return SettingMetaSource.NvD3dUmxSettings; }
        }
    }
}
