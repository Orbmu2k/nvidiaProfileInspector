namespace nvidiaProfileInspector.Common.Meta
{
    public class MetaServiceItem
    {
        public ISettingMetaService Service { get; set; }
        public uint ValueNamePrio { get; set; }

        /// <summary>
        /// Resolution order for the setting value type (lower wins). The live driver is
        /// authoritative; XML types are only fallbacks for settings the driver can't describe.
        /// </summary>
        public uint TypePrio { get; set; }
    }
}