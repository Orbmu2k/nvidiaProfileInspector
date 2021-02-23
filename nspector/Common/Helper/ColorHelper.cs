using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using nspector.Common.CustomSettings;

namespace nspector.Common.Helper
{
    public static class ColorHelper
    {

        public static CustomColorSetting getCustomColorSetting()
        {
            string filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CustomColors.xml";
            return XMLHelper<CustomColorSetting>.DeserializeFromXMLFile(filename);
        }

        public static void SetControlColors(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                UpdateColorControls(control, GetBackColor(), GetForeColor());
            }
        }

        private static Color GetColorHelper(Func<CustomColorSetting, string> obtainColorFunc)
        {
            CustomColorSetting colorSettings = getCustomColorSetting();
            return colorSettings.ColorFromRgbString(obtainColorFunc(colorSettings));
        }


        
        static void UpdateColorControls(Control myControl, Color? backColor, Color? foreColor)
        {
            backColor ??= Color.DarkGray;
            foreColor ??= Color.Black;

            myControl.BackColor = backColor.Value;
            myControl.ForeColor = foreColor.Value;
            foreach (Control subControl in myControl.Controls)
            {
                UpdateColorControls(subControl, backColor, foreColor);
            }
        }


        public static Color GetTextColor()
        {
            return GetColorHelper(customSetting => customSetting.ExtraText);
        }
        public static Color GetBackColor()
        {
            return GetColorHelper(customSetting => customSetting.BackColor);
        }
        public static Color GetForeColor()
        {
            return GetColorHelper(customSetting => customSetting.ForeColor);
        }
        public static Color GetInactiveFunctionText()
        {
            return GetColorHelper(customSetting => customSetting.InactiveFunctionText);
        }
        public static Color GetControlImageText()
        {
            return GetColorHelper(customSetting => customSetting.ControlImageText);
        }
        public static Color GetOnChangeText()
        {
            return GetColorHelper(customSetting => customSetting.OnChangeText);
        }
        public static Color GetGlobalSettingState3Color()
        {
            return GetColorHelper(customSetting => customSetting.GlobalSettingState3Color);
        }

        public static Color GetGlobalSettingState2Color()
        {
            return GetColorHelper(customSetting => customSetting.GlobalSettingState2Color);
        }

        public static Color GetGlobalSettingState0Color()
        {
            return GetColorHelper(customSetting => customSetting.GlobalSettingState0Color);
        }

        public static bool GetIgnoreChangeText()
        {
            return getCustomColorSetting().IgnoreChangeText;
        }
    }
}

