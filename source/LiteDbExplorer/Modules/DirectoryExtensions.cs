using System;
using System.IO;
using JetBrains.Annotations;

namespace LiteDbExplorer.Modules
{
    public static class DirectoryExtensions
    {
        public static string EnsureUniqueFileName(this FileInfo fileInfo)
        {
            if (fileInfo?.DirectoryName == null)
            {
                throw new DirectoryNotFoundException();
            }

            var fileCount = 0;
            string newFileName;
            do
            {
                fileCount++;
                newFileName =
                    $"{Path.GetFileNameWithoutExtension(fileInfo.Name)} {(fileCount > 0 ? "(" + fileCount + ")" : "")}{Path.GetExtension(fileInfo.Name)}";
            } while (File.Exists(Path.Combine(fileInfo.DirectoryName, newFileName)));

            return newFileName;
        }

        public static void EnsureFileDirectory([NotNull] string fileFullName)
        {
            if (string.IsNullOrEmpty(fileFullName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(fileFullName));
            }

            var dir = Path.GetDirectoryName(fileFullName);

            EnsureDirectory(dir);
        }

        public static void EnsureDirectory([NotNull] string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(directoryPath));
            }
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static string EnsureFileNameExtension(string fileName, string extension)
        {
            if (!Path.HasExtension(fileName) || !fileName.EndsWith(extension))
            {
                fileName = $"{fileName.TrimEnd('.')}.{extension.TrimStart('.')}";
            }

            return fileName;
        }

        public static void PrependTimeStamp(ref string fileName)
        {
            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyy-MM-dd_HH-mmss}_{Path.GetExtension(fileName)}";
        }
    }
}