using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Highlighting;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    [Export(typeof(ISyntaxHighlightingServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SyntaxHighlightingServices : ISyntaxHighlightingServices
    {
        public static ISyntaxHighlightingServices Current = IoC.Get<ISyntaxHighlightingServices>();

        public IHighlightingDefinition LoadDefinition(string name, string theme)
        {
            var highlightingDefinition = HighlightingProviderParts
                .Where(p => p.Metadata.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Value)
                .FirstOrDefault();

            return highlightingDefinition?.LoadDefinition(theme);
        }

        [ImportMany(typeof(IHighlightingProvider), AllowRecomposition = true)]
        private IEnumerable<Lazy<IHighlightingProvider, IHighlightingProviderMetadata>> HighlightingProviderParts { get; set; }
    }
}