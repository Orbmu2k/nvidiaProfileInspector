#region

using nvw=nspector.Native.NVAPI2.NvapiDrsWrapper;

#endregion

namespace nspector.Common.Meta;

class DriverSettingMetaService:ISettingMetaService
{
    readonly System.Collections.Generic.List<uint> _settingIds;

    readonly System.Collections.Generic.Dictionary<uint,SettingMeta> _settingMetaCache
        =new System.Collections.Generic.Dictionary<uint,SettingMeta>();

    public DriverSettingMetaService()=>this._settingIds=this.InitSettingIds();

    public string GetSettingName(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.SettingName;
        }

        return null;
    }

    public uint? GetDwordDefaultValue(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.DefaultDwordValue;
        }

        return null;
    }

    public string GetStringDefaultValue(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.DefaultStringValue;
        }

        return null;
    }

    public System.Collections.Generic.List<SettingValue<string>> GetStringValues(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.StringValues;
        }

        return null;
    }

    public System.Collections.Generic.List<SettingValue<uint>> GetDwordValues(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.DwordValues;
        }

        return null;
    }

    public System.Collections.Generic.List<uint> GetSettingIds()=>this._settingIds;

    public nspector.Native.NVAPI2.NVDRS_SETTING_TYPE? GetSettingValueType(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.SettingType;
        }

        return null;
    }

    public string GetGroupName(uint settingId)=>null;

    public byte[] GetBinaryDefaultValue(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.DefaultBinaryValue;
        }

        return null;
    }

    public System.Collections.Generic.List<SettingValue<byte[]>> GetBinaryValues(uint settingId)
    {
        var settingMeta=this.GetSettingsMeta(settingId);
        if(settingMeta!=null)
        {
            return settingMeta.BinaryValues;
        }

        return null;
    }

    public SettingMetaSource Source
    {
        get
        {
            return SettingMetaSource.DriverSettings;
        }
    }

    System.Collections.Generic.List<uint> InitSettingIds()
    {
        var settingIds=new System.Collections.Generic.List<uint>();

        var nvRes=nvw.DRS_EnumAvailableSettingIds(out settingIds,512);
        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_EnumAvailableSettingIds",nvRes);
        }

        return settingIds;
    }

    SettingMeta GetDriverSettingMetaInternal(uint settingId)
    {
        var values=new nspector.Native.NVAPI2.NVDRS_SETTING_VALUES();
        values.version=nvw.NVDRS_SETTING_VALUES_VER;
        uint valueCount=255;

        var nvRes=nvw.DRS_EnumAvailableSettingValues(settingId,ref valueCount,ref values);

        if(nvRes==nspector.Native.NVAPI2.NvAPI_Status.NVAPI_SETTING_NOT_FOUND)
        {
            return null;
        }

        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_EnumAvailableSettingValues",nvRes);
        }


        var sbSettingName=new System.Text.StringBuilder((int)nvw.NVAPI_UNICODE_STRING_MAX);
        nvRes=nvw.DRS_GetSettingNameFromId(settingId,sbSettingName);
        if(nvRes!=nspector.Native.NVAPI2.NvAPI_Status.NVAPI_OK)
        {
            throw new NvapiException("DRS_GetSettingNameFromId",nvRes);
        }

        var settingName=sbSettingName.ToString();
        if(string.IsNullOrWhiteSpace(settingName))
        {
            settingName=DrsUtil.GetDwordString(settingId);
        }

        var result=new SettingMeta
        {
            SettingType=values.settingType,SettingName=settingName,
        };


        if(values.settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
        {
            result.DefaultDwordValue=values.defaultValue.dwordValue;
            result.DwordValues      =new System.Collections.Generic.List<SettingValue<uint>>();
            for(var i=0;i<values.numSettingValues;i++)
            {
                result.DwordValues.Add(
                    new SettingValue<uint>(this.Source)
                    {
                        Value    =values.settingValues[i].dwordValue,
                        ValueName=DrsUtil.GetDwordString(values.settingValues[i].dwordValue),
                    });
            }
        }

        if(values.settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
        {
            result.DefaultStringValue=values.defaultValue.stringValue;
            result.StringValues      =new System.Collections.Generic.List<SettingValue<string>>();
            for(var i=0;i<values.numSettingValues;i++)
            {
                var strValue=values.settingValues[i].stringValue;
                if(strValue!=null)
                {
                    result.StringValues.Add(
                        new SettingValue<string>(this.Source)
                        {
                            Value=strValue,ValueName=strValue,
                        });
                }
            }
        }

        if(values.settingType==nspector.Native.NVAPI2.NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
        {
            result.DefaultBinaryValue=values.defaultValue.binaryValue;
            result.BinaryValues      =new System.Collections.Generic.List<SettingValue<byte[]>>();
            for(var i=0;i<values.numSettingValues;i++)
            {
                var binValue=values.settingValues[i].binaryValue;
                if(binValue!=null)
                {
                    result.BinaryValues.Add(
                        new SettingValue<byte[]>(this.Source)
                        {
                            Value=binValue,ValueName=DrsUtil.GetBinaryString(binValue),
                        });
                }
            }
        }

        return result;
    }

    SettingMeta GetSettingsMeta(uint settingId)
    {
        if(this._settingMetaCache.ContainsKey(settingId))
        {
            return this._settingMetaCache[settingId];
        }

        var settingMeta=this.GetDriverSettingMetaInternal(settingId);
        if(settingMeta!=null)
        {
            this._settingMetaCache.Add(settingId,settingMeta);
            return settingMeta;
        }

        return null;
    }
}