using System;

namespace LiteDbExplorer.Modules.DbQuery
{
    public class RawQueryHistory
    {
        public string GroupKey { get; set; }

        public string DatabaseLocation { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? LastRunAt { get; set; }

        public string RawQuery { get; set; }
    }
}