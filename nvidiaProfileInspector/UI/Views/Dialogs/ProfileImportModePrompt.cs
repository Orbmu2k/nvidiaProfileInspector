namespace nvidiaProfileInspector.UI.Views.Dialogs
{
    using nvidiaProfileInspector.Common;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Asks the user how an interactively triggered .nip import should be applied
    /// (drag &amp; drop or file-association double-click). Returns null when cancelled.
    /// </summary>
    public static class ProfileImportModePrompt
    {
        public static ProfileImportMode? Ask(Window owner, int fileCount)
        {
            var noun = fileCount == 1 ? "profile" : "profiles";
            var items = new List<ActionCardsDialog.ActionCardItem>
            {
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Replace",
                    Description = $"Reset each target {noun} and write only the imported apps and settings.",
                    IconPath = Application.Current.Resources["IconImport"] as Geometry,
                    IconFill = Application.Current.Resources["TextSecondaryBrush"] as Brush,
                },
                new ActionCardsDialog.ActionCardItem
                {
                    Title = "Merge",
                    Description = $"Keep the existing values in each target {noun} and overwrite only the settings contained in the import.",
                    IconPath = Application.Current.Resources["IconUser"] as Geometry,
                    IconFill = Application.Current.Resources["TextSecondaryBrush"] as Brush,
                },
            };

            var subtitle = fileCount == 1
                ? "Choose how this .nip file should be imported:"
                : $"Choose how these {fileCount} .nip files should be imported:";

            var dialog = new ActionCardsDialog("Import Profiles", items, subtitle);
            if (owner != null && owner.IsLoaded)
                dialog.Owner = owner;
            else
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (dialog.ShowDialog() != true || dialog.SelectedItem == null)
                return null;

            return dialog.SelectedItem.Title == "Merge"
                ? ProfileImportMode.Merge
                : ProfileImportMode.Replace;
        }
    }
}
