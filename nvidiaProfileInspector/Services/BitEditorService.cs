namespace nvidiaProfileInspector.Services
{
    public interface IBitEditorService
    {
        void ShowBitEditor(uint settingId, uint value, string settingName, System.Action<uint> onValueChanged);
    }

    public class BitEditorService : IBitEditorService
    {
        public void ShowBitEditor(uint settingId, uint value, string settingName, System.Action<uint> onValueChanged)
        {
            var dialog = new UI.Views.Dialogs.BitEditorDialog(settingId, value, settingName);
            dialog.OnValueChanged = onValueChanged;
            dialog.ShowDialog();
        }
    }
}
