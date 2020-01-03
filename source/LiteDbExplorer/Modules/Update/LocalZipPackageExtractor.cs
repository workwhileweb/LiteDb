using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;

namespace LiteDbExplorer.Modules
{

    /// <summary>
    /// Extracts files from zip-archived packages.
    /// </summary>
    public class LocalZipPackageExtractor : IPackageExtractor
    {
        public bool IgnoreZipRootPath { get; set; }

        /// <inheritdoc />
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath,
            IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            // Read the zip
            using (var archive = ZipFile.OpenRead(sourceFilePath))
            {
                // For progress reporting
                var totalBytes = archive.Entries.Sum(e => e.Length);
                var totalBytesCopied = 0L;

                // Loop through all entries
                foreach (var entry in archive.Entries)
                {
                    if (entry.Length == 0)
                    {
                        continue;
                    }

                    var entryFullName = entry.FullName;
                    if (IgnoreZipRootPath)
                    {
                        entryFullName = RemoveRootPath(entryFullName);
                    }

                    // Get destination paths
                    var entryDestFilePath = Path.Combine(destDirPath, entryFullName);
                    var entryDestDirPath = Path.GetDirectoryName(entryDestFilePath);

                    // Create directory
                    if (!string.IsNullOrWhiteSpace(entryDestDirPath))
                    {
                        Directory.CreateDirectory(entryDestDirPath);
                    }

                    // If the entry is a directory - continue
                    if (entryFullName.Last() == Path.DirectorySeparatorChar || entryFullName.Last() == Path.AltDirectorySeparatorChar)
                    {
                        continue;
                    }

                    // Extract entry
                    using (var input = entry.Open())
                    using (var output = File.Create(entryDestFilePath))
                    {
                        var buffer = new byte[81920];
                        int bytesCopied;
                        do
                        {
                            // Copy
                            bytesCopied = await CopyChunkToAsync(input, output, buffer, cancellationToken);

                            // Report progress
                            totalBytesCopied += bytesCopied;
                            progress?.Report(1.0 * totalBytesCopied / totalBytes);
                        } while (bytesCopied > 0);
                    }

                }
            }

        }

        protected static async Task<int> CopyChunkToAsync(Stream source, Stream destination, byte[] buffer,
            CancellationToken cancellationToken = default)
        {
            // Read
            var bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            // Write
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

            return bytesCopied;
        }

        protected static string RemoveRootPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return fullPath;
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), fullPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Skip(1));
        }
    }

}