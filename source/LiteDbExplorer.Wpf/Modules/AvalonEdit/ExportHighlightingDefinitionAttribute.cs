using System;
using System.ComponentModel.Composition;
using JetBrains.Annotations;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class ExportHighlightingDefinitionAttribute : ExportAttribute, IHighlightingProviderMetadata
    {
        public ExportHighlightingDefinitionAttribute([NotNull] string name, int order, string fileExtension = null)
            : base(typeof(IHighlightingProvider))
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Order = order;
            FileExtension = fileExtension;
        }

        public string Name { get; }
        public int Order { get; }
        public string FileExtension { get; }
    }
}