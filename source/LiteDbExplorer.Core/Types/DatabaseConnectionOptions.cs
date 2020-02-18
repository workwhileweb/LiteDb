using System.Collections.Generic;
using System.Linq;

namespace LiteDbExplorer.Core
{
    public class DatabaseConnectionOptions
    {
        public DatabaseConnectionOptions(string path, string password = null)
        {
            Path = path;
            Password = password;
        }

        public string Path { get; }

        public string Password { get; set; }

        public bool EnableLog { get; set; } = false;

        public DatabaseFileMode Mode { get; set; } = DatabaseFileMode.Exclusive;

        public string GetConnectionString()
        {
            var connectionMap = new Dictionary<string, string>
            {
                { "Filename", Path },
                { "Password", Password },
                { "Mode", $"{Mode}" }
            };

            var connectionString = connectionMap
                .Where(pair => !string.IsNullOrEmpty(pair.Value))
                .Select(pair => $"{pair.Key}={pair.Value}")
                .Aggregate((p, n) => $"{p};{n}");

            return connectionString;
        }
    }
}