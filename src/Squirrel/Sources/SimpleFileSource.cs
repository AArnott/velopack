﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Squirrel.Sources
{
    /// <summary>
    /// Retrieves available updates from a local or network-attached disk. The directory
    /// must contain one or more valid packages, as well as a 'RELEASES' index file.
    /// </summary>
    public class SimpleFileSource : SourceBase
    {
        /// <summary> The local directory containing packages to update to. </summary>
        public virtual DirectoryInfo BaseDirectory { get; }

        /// <inheritdoc cref="SimpleFileSource" />
        public SimpleFileSource(DirectoryInfo baseDirectory, string channel = null, ILogger logger = null)
            : base(channel, logger)
        {
            BaseDirectory = baseDirectory;
        }

        /// <inheritdoc />
        public override Task<ReleaseEntry[]> GetReleaseFeed(Guid? stagingId = null, ReleaseEntry latestLocalRelease = null)
        {
            if (!BaseDirectory.Exists)
                throw new Exception($"The local update directory '{BaseDirectory.FullName}' does not exist.");

            var releasesPath = Path.Combine(BaseDirectory.FullName, "RELEASES");
            Log.Info($"Reading RELEASES from '{releasesPath}'");
            var fi = new FileInfo(releasesPath);

            if (fi.Exists) {
                var txt = File.ReadAllText(fi.FullName, encoding: Encoding.UTF8);
                return Task.FromResult(ReleaseEntry.ParseReleaseFileAndApplyStaging(txt, stagingId).ToArray());
            } else {
                var packages = BaseDirectory.EnumerateFiles("*.nupkg");
                if (packages.Any()) {
                    Log.Warn($"The file '{releasesPath}' does not exist but directory contains packages. " +
                        $"This is not valid but attempting to proceed anyway by writing new file.");
                    return Task.FromResult(ReleaseEntry.BuildReleasesFile(BaseDirectory.FullName).ToArray());
                } else {
                    throw new Exception($"The file '{releasesPath}' does not exist. Cannot update from invalid source.");
                }
            }
        }

        /// <inheritdoc />
        public override Task DownloadReleaseEntry(ReleaseEntry releaseEntry, string localFile, Action<int> progress)
        {
            var releasePath = Path.Combine(BaseDirectory.FullName, releaseEntry.OriginalFilename);
            if (!File.Exists(releasePath))
                throw new Exception($"The file '{releasePath}' does not exist. The packages directory is invalid.");

            File.Copy(releasePath, localFile, true);
            progress?.Invoke(100);
            return Task.CompletedTask;
        }
    }
}
