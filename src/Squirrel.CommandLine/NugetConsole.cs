﻿using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using NuGet.Commands;
using Squirrel.CommandLine.Commands;
using Squirrel.SimpleSplat;
using NG = NuGet.Common;

namespace Squirrel.CommandLine
{
    internal class NugetConsole : NG.ILogger, IEnableLogger
    {
        public static string CreateNuspec(
            string packId, string packTitle, string packAuthors,
            string packVersion, string releaseNotes, bool includePdb, string libFolderName)
        {
            var releaseNotesText = String.IsNullOrEmpty(releaseNotes)
                ? "" // no releaseNotes
                : $"<releaseNotes>{SecurityElement.Escape(File.ReadAllText(releaseNotes))}</releaseNotes>";

            string nuspec = $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<package>
  <metadata>
    <id>{packId}</id>
    <title>{packTitle ?? packId}</title>
    <description>{packTitle ?? packId}</description>
    <authors>{packAuthors ?? packId}</authors>
    <version>{packVersion}</version>
    {releaseNotesText}
  </metadata>
  <files>
    <file src=""**"" target=""lib\{libFolderName}\"" exclude=""{(includePdb ? "" : "*.pdb;")}*.nupkg;*.vshost.*;**\createdump.exe""/>
  </files>
</package>
".Trim();

            return nuspec;
        }

        public static string CreatePackageFromNuspecPath(string tempDir, string packDir, string nuspecPath)
        {
            var nup = Path.Combine(tempDir, "squirreltemp.nuspec");
            File.Copy(nuspecPath, nup);

            new NugetConsole().Pack(nup, packDir, tempDir);

            var nupkgPath = Directory.EnumerateFiles(tempDir).Where(f => f.EndsWith(".nupkg")).FirstOrDefault();
            if (nupkgPath == null)
                throw new Exception($"Failed to generate nupkg, unspecified error");

            return nupkgPath;
        }

        public static string CreatePackageFromOptions(string tempDir, INugetPackCommand command, string libFolderName)
        {
            return CreatePackageFromMetadata(tempDir, command.PackDirectory.FullName, command.PackId, command.PackTitle,
                command.PackAuthors, command.PackVersion, command.ReleaseNotes?.FullName ?? "", command.IncludePdb, libFolderName);
        }

        public static string CreatePackageFromMetadata(
            string tempDir, string packDir, string packId, string packTitle, string packAuthors,
            string packVersion, string releaseNotes, bool includePdb, string libFolderName)
        {
            string nuspec = CreateNuspec(packId, packTitle, packAuthors, packVersion, releaseNotes, includePdb, libFolderName);
            var nuspecPath = Path.Combine(tempDir, packId + ".nuspec");
            File.WriteAllText(nuspecPath, nuspec);
            return CreatePackageFromNuspecPath(tempDir, packDir, nuspecPath);
        }

        public void Pack(string nuspecPath, string baseDirectory, string outputDirectory)
        {
            this.Log().Info($"Starting to package '{nuspecPath}'");
            var args = new PackArgs() {
                Deterministic = true,
                BasePath = baseDirectory,
                OutputDirectory = outputDirectory,
                Path = nuspecPath,
                Exclude = Enumerable.Empty<string>(),
                Arguments = Enumerable.Empty<string>(),
                Logger = this,
                ExcludeEmptyDirectories = true,
                NoDefaultExcludes = true,
                NoPackageAnalysis = true,
            };

            var c = new PackCommandRunner(args, null);
            if (!c.RunPackageBuild())
                throw new Exception("Error creating nuget package.");
        }

        #region NuGet.Common.ILogger

        public void Log(NG.LogLevel level, string data)
        {
            this.Log().Info(data);
        }

        public void Log(NG.ILogMessage message)
        {
            this.Log().Info(message.Message);
        }

        public Task LogAsync(NG.LogLevel level, string data)
        {
            this.Log().Info(data);
            return Task.CompletedTask;
        }

        public Task LogAsync(NG.ILogMessage message)
        {
            this.Log().Info(message.Message);
            return Task.CompletedTask;
        }

        public void LogDebug(string data)
        {
            this.Log().Debug(data);
        }

        public void LogError(string data)
        {
            this.Log().Error(data);
        }

        public void LogInformation(string data)
        {
            this.Log().Info(data);
        }

        public void LogInformationSummary(string data)
        {
            this.Log().Info(data);
        }

        public void LogMinimal(string data)
        {
            this.Log().Info(data);
        }

        public void LogVerbose(string data)
        {
            this.Log().Debug(data);
        }

        public void LogWarning(string data)
        {
            this.Log().Warn(data);
        }

        #endregion NuGet.Common.ILogger
    }
}