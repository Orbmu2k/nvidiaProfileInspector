using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nspector.Common.Meta
{
    public enum SettingMetaSource
    {
        CustomSettings = 10,
        DriverSettings = 20,
        ConstantSettings = 30,
        ReferenceSettings = 40,
        NvD3dUmxSettings = 50,
        ScannedSettings = 60,
    }
}
