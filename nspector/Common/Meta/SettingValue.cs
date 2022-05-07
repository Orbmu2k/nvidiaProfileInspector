namespace nspector.Common.Meta;

class SettingValue<T>
{
    public SettingMetaSource ValueSource;

    public SettingValue(SettingMetaSource source)=>this.ValueSource=source;

    public int ValuePos
    {
        get;
        set;
    }

    public string ValueName
    {
        get;
        set;
    }

    public T Value
    {
        get;
        set;
    }

    public override string ToString()
    {
        if(typeof(T)==typeof(uint))
        {
            return string.Format("Value=0x{0:X8}; ValueName={1}; Source={2};",this.Value,this.ValueName,
                this.ValueSource);
        }

        return string.Format("Value={0}; ValueName={1};",this.Value,this.ValueName);
    }
}