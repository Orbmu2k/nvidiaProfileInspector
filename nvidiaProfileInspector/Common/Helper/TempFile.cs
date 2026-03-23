using System;
using System.IO;

namespace nvidiaProfileInspector.Common.Helper
{
    public static class TempFile
    {
        public static string GetTempFileName()
        {
            while (true)
            {
                var tempFile = GenerateTempFileName();
                if (!File.Exists(tempFile))
                    return tempFile;
            }
        }

        private static string GenerateTempFileName()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
        }

    }
}
