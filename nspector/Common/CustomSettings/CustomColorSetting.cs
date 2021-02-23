using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace nspector.Common.CustomSettings
{
    [Serializable]
    public class CustomColorSetting
    {
        public string BackColor { get; set; }
        public string ForeColor { get; set; }
        public string ExtraText { get; set; }
        public string InactiveFunctionText { get; set; }
        public string ControlImageText { get; set; }
        public string OnChangeText { get; set; }
        public bool IgnoreChangeText { get; set; }
        public string GlobalSettingState3Color { get; set; }
        public string GlobalSettingState2Color { get; set; }
        public string GlobalSettingState0Color { get; set; }

        public Color ColorFromRgbString(string color)
        {
            var rgbList = color.Split(',');
            var rgbResult = rgbList.Select(x =>
            {

                var success = int.TryParse(x.Trim(), out int result);
                if (!success)
                {
                    return 0;
                }

                return result;
            }).ToArray();

            if (rgbResult.Count() == 3)
            {
                return Color.FromArgb(255, rgbResult[0], rgbResult[1], rgbResult[2]);
            }

            return Color.Black;
        }
    }
}
