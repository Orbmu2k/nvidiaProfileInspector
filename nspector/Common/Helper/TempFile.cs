using System;
using System.IO;

namespace nspector.Common.Helper
{
    internal static class TempFile
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
