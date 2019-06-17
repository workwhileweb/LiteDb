using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Onova.Services;

namespace LiteDbExplorer.Modules
{
    /// <summary>
    /// Extracts files from zip-archived packages.
    /// </summary>
    public class LocalZipPackageExtractor : IPackageExtractor
    {
        /// <inheritdoc />
        public async Task ExtractAsync([NotNull] string sourceFilePath, [NotNull] string destDirPath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sourceFilePath == null)
            {
                throw new ArgumentNullException(nameof(sourceFilePath));
            }

            if (destDirPath == null)
            {
                throw new ArgumentNullException(nameof(destDirPath));
            }


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

                    // Get destination paths
                    var entryDestFilePath = Path.Combine(destDirPath, entry.FullName);
                    var entryDestDirPath = Path.GetDirectoryName(entryDestFilePath);

                    // Create directory
                    Directory.CreateDirectory(entryDestDirPath);

                    // Extract entry
                    using (var input = entry.Open())
                    using (var output = File.Create(entryDestFilePath))
                    {
                        int bytesCopied;
                        do
                        {
                            // Copy
                            bytesCopied = await CopyChunkToAsync(input, output, cancellationToken)
                                .ConfigureAwait(false);

                            // Report progress
                            totalBytesCopied += bytesCopied;
                            progress?.Report(1.0 * totalBytesCopied / totalBytes);
                        } while (bytesCopied > 0);
                    }
                }
            }
        }

        public static async Task<int> CopyChunkToAsync(Stream source, Stream destination,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new byte[81920];

            // Read
            var bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

            // Write
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken).ConfigureAwait(false);

            return bytesCopied;
        }
    }
}