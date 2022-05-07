namespace nspector.Common.Helper;

static class TempFile
{
    public static string GetTempFileName()
    {
        while(true)
        {
            var tempFile=TempFile.GenerateTempFileName();
            if(!System.IO.File.Exists(tempFile))
            {
                return tempFile;
            }
        }
    }

    static string GenerateTempFileName()
        =>System.IO.Path.Combine(System.IO.Path.GetTempPath(),System.Guid.NewGuid().ToString().Replace("-",""));
}