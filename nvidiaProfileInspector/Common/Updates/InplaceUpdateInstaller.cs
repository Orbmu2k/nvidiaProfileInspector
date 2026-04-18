using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace nvidiaProfileInspector.Common.Updates
{
    public sealed class InplaceUpdateInstaller : IUpdateInstaller
    {
        public async Task PrepareAndRunAsync(UpdateRelease release)
        {
            if (release == null)
                throw new InvalidOperationException("No update release was selected.");

            if (!release.IsInstallable)
                throw new InvalidOperationException("The selected release does not contain a downloadable update package.");

            var tempRoot = Path.Combine(Path.GetTempPath(), "nvidiaProfileInspector-update-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);

            var assetPath = Path.Combine(tempRoot, SanitizeFileName(release.Asset.Name ?? "update.zip"));
            await DownloadFileAsync(release.Asset.DownloadUrl, assetPath);

            var sourcePath = PrepareUpdateSource(tempRoot, assetPath, release.Asset.PackageType);
            var appDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var executablePath = Assembly.GetEntryAssembly()?.Location ?? Process.GetCurrentProcess().MainModule.FileName;
            var scriptPath = Path.Combine(tempRoot, "apply-update.cmd");

            File.WriteAllText(scriptPath, CreateUpdateScript(Process.GetCurrentProcess().Id, tempRoot, sourcePath, appDirectory, executablePath));

            Process.Start(new ProcessStartInfo
            {
                FileName = scriptPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }

        private static async Task DownloadFileAsync(string url, string targetPath)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "nvidiaProfileInspector/" + AppUpdateService.GetCurrentVersion());

            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var input = await response.Content.ReadAsStreamAsync();
            using var output = File.Create(targetPath);
            await input.CopyToAsync(output);
        }

        private static string PrepareUpdateSource(string tempRoot, string assetPath, UpdatePackageType packageType)
        {
            if (packageType == UpdatePackageType.Zip)
            {
                var extractPath = Path.Combine(tempRoot, "extracted");
                ZipFile.ExtractToDirectory(assetPath, extractPath);
                return FindUpdateRoot(extractPath);
            }

            if (packageType == UpdatePackageType.Exe)
            {
                var sourcePath = Path.Combine(tempRoot, "extracted");
                Directory.CreateDirectory(sourcePath);
                File.Copy(assetPath, Path.Combine(sourcePath, Path.GetFileName(Assembly.GetEntryAssembly()?.Location ?? "nvidiaProfileInspector.exe")));
                return sourcePath;
            }

            throw new InvalidOperationException("The selected update package format is not supported.");
        }

        private static string FindUpdateRoot(string extractPath)
        {
            var executableName = Path.GetFileName(Assembly.GetEntryAssembly()?.Location ?? "nvidiaProfileInspector.exe");
            var executable = Directory
                .GetFiles(extractPath, executableName, SearchOption.AllDirectories)
                .OrderBy(path => path.Length)
                .FirstOrDefault();

            return executable == null ? extractPath : Path.GetDirectoryName(executable);
        }

        private static string CreateUpdateScript(int processId, string tempRoot, string sourcePath, string appDirectory, string executablePath)
        {
            return string.Join(Environment.NewLine, new[]
            {
                "@echo off",
                "setlocal",
                $"set \"PID={processId}\"",
                $"set \"TEMPROOT={tempRoot}\"",
                $"set \"SOURCE={sourcePath}\"",
                $"set \"TARGET={appDirectory}\"",
                $"set \"EXE={executablePath}\"",
                ":wait",
                "tasklist /FI \"PID eq %PID%\" | findstr /R /C:\"^[^ ]* *%PID% \" >nul",
                "if not errorlevel 1 (",
                "  timeout /t 1 /nobreak >nul",
                "  goto wait",
                ")",
                "robocopy \"%SOURCE%\" \"%TARGET%\" /E /IS /IT /R:3 /W:1 >nul",
                "if %ERRORLEVEL% GEQ 8 goto failed",
                "start \"\" \"%EXE%\"",
                "goto cleanup",
                ":failed",
                "start \"\" \"%EXE%\"",
                ":cleanup",
                "rmdir /S /Q \"%TEMPROOT%\" >nul 2>nul",
                "del \"%~f0\" >nul 2>nul"
            });
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidChar, '_');

            return fileName;
        }
    }
}
