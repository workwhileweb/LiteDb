using System;
using LiteDbExplorer.Core;
using LiteDbExplorer.Wpf.Framework;

namespace LiteDbExplorer.Modules.DbQuery
{
    public class RunQueryContext : IReferenceId
    {
        private RunQueryContext()
        {
            InstanceId = Guid.NewGuid().ToString("D");
        }

        public RunQueryContext(DatabaseReference databaseReference = null, QueryReference queryReference = null) : this()
        {
            DatabaseReference = databaseReference;
            QueryReference = queryReference;
        }

        public string InstanceId { get; }

        public DatabaseReference DatabaseReference { get; }

        public QueryReference QueryReference { get; }

        public bool RunOnStart { get; set; }
    }
}