namespace nvidiaProfileInspector.Common
{
    /// <summary>
    /// How an imported .nip should be applied to its target profiles.
    /// </summary>
    public enum ProfileImportMode
    {
        /// <summary>Reset each target profile and write only the imported apps/settings.</summary>
        Replace,

        /// <summary>Keep existing values and overwrite only those also present in the import.</summary>
        Merge,
    }
}
