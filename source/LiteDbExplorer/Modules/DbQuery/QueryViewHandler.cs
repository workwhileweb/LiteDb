using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbQuery
{
    public interface IQueryViewHandler
    {
        Task<IList<IScreen>> RunQuery(DatabaseReference databaseReference, IQueryHistoryProvider queryHistoryProvider, string query);
    }

    public interface IQueryViewHandlerMetadata
    {
        string Name { get; }
        string DisplayName { get; }
        int DisplayOrder { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class ExportQueryViewHandlerAttribute : ExportAttribute, IQueryViewHandlerMetadata
    {
        public ExportQueryViewHandlerAttribute([NotNull] string name, string displayName = null, int displayOrder = 0)
            : base(typeof(IQueryViewHandler))
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? name;
            DisplayOrder = displayOrder;
        }

        public string Name { get; }
        public string DisplayName { get; }
        public int DisplayOrder { get; }
    }
}