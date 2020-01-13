namespace nspector.Common.Meta
{
    internal class SettingValue<T>
    {

        public SettingMetaSource ValueSource;

        public SettingValue(SettingMetaSource source)
        {
            ValueSource = source;
        }

        public int ValuePos { get; set; }
        public string ValueName { get; set; }
        public T Value { get; set; }

        public override string ToString()
        {
            if (typeof(T) == typeof(uint))
                return string.Format("Value=0x{0:X8}; ValueName={1}; Source={2};", Value, ValueName, ValueSource);

            return string.Format("Value={0}; ValueName={1};", Value, ValueName);
        }

    }
}
