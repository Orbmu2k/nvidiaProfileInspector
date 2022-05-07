namespace nspector.Common.Import;

class ImportExportUitl
{
    public static bool AreDrsSettingEqualToProfileSetting(nspector.Native.NVAPI2.NVDRS_SETTING drsSetting,
        ProfileSetting                                                                         profileSetting)
    {
        var profileSettingCompare=ImportExportUitl.ConvertDrsSettingToProfileSetting(drsSetting);
        return profileSetting.SettingValue.Equals(profileSettingCompare.SettingValue);
    }

    public static ProfileSetting ConvertDrsSettingToProfileSetting(nspector.Native.NVAPI2.NVDRS_SETTING setting)
        =>new ProfileSetting
        {
            SettingId   =setting.settingId,SettingNameInfo=setting.settingName,
            SettingValue=ImportExportUitl.ConvertSettingValueToString(setting),
            ValueType   =ImportExportUitl.MapValueType(setting.settingType),
        };

    static string ConvertSettingValueToString(nspector.Native.NVAPI2.NVDRS_SETTING setting)
    {
        var settingUnion=setting.currentValue;
        if(setting.isCurrentPredefined==1)
        {
            settingUnion=setting.predefinedValue;
        }

        switch(setting.settingType)
        {
            case nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE:
                return settingUnion.dwordValue.ToString();
            case nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE:
                return settingUnion.ansiStringValue;
            case nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE:
                return settingUnion.stringValue;
            case nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE:
                return System.Convert.ToBase64String(settingUnion.binaryValue);
            default:
                throw new System.Exception("invalid setting type");
        }
    }

    static SettingValueType MapValueType(nspector.Native.NVAPI2.NVDRS_SETTING_TYPE input)
    {
        switch(input)
        {
            case nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE:
                return SettingValueType.Binary;
            case nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE:
                return SettingValueType.AnsiString;
            case nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE:
                return SettingValueType.String;
            default:
                return SettingValueType.Dword;
        }
    }

    public static nspector.Native.NVAPI2.NVDRS_SETTING ConvertProfileSettingToDrsSetting(ProfileSetting setting)
    {
        var newSetting=new nspector.Native.NVAPI2.NVDRS_SETTING
        {
            version        =nspector.Native.NVAPI2.NvapiDrsWrapper.NVDRS_SETTING_VER,settingId=setting.SettingId,
            settingType    =ImportExportUitl.MapValueType(setting.ValueType),
            settingLocation=nspector.Native.NVAPI2.NVDRS_SETTING_LOCATION.NVDRS_CURRENT_PROFILE_LOCATION,
            currentValue   =ImportExportUitl.ConvertStringToSettingUnion(setting.ValueType,setting.SettingValue),
        };
        return newSetting;
    }

    static nspector.Native.NVAPI2.NVDRS_SETTING_UNION ConvertStringToSettingUnion(SettingValueType valueType,
        string                                                                                     valueString)
    {
        var union=new nspector.Native.NVAPI2.NVDRS_SETTING_UNION();
        switch(valueType)
        {
            case SettingValueType.Dword:
                union.dwordValue=uint.Parse(valueString);
                break;
            case SettingValueType.String:
                union.stringValue=valueString;
                break;
            case SettingValueType.AnsiString:
                union.ansiStringValue=valueString;
                break;
            case SettingValueType.Binary:
                union.binaryValue=System.Convert.FromBase64String(valueString);
                break;
            default:
                throw new System.Exception("invalid value type");
        }

        return union;
    }

    static nspector.Native.NVAPI2.NVDRS_SETTING_TYPE MapValueType(SettingValueType input)
    {
        switch(input)
        {
            case SettingValueType.Binary:
                return nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE;
            case SettingValueType.AnsiString:
                return nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE;
            case SettingValueType.String:
                return nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE;
            default:
                return nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE;
        }
    }
}