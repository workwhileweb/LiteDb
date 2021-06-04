using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CSharpFunctionalExtensions;

namespace LiteDbExplorer.Modules.DbQuery
{
    public interface IQueryViewsProvider
    {
        Maybe<Lazy<IQueryViewHandler>> FindHandler(string name);
        IEnumerable<IQueryViewHandlerMetadata> ListMetadata();
    }

    [Export(typeof(IQueryViewsProvider))]
    public class QueryViewsProvider : IQueryViewsProvider
    {
        [ImportMany(typeof(IQueryViewHandler), AllowRecomposition = true)]
        private IEnumerable<Lazy<IQueryViewHandler, IQueryViewHandlerMetadata>> HandlersParts { get; set; }

        public Maybe<Lazy<IQueryViewHandler>> FindHandler(string name)
        {
            var lazyHandler = HandlersParts.FirstOrDefault(p => p.Metadata.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return lazyHandler;
        }

        public IEnumerable<IQueryViewHandlerMetadata> ListMetadata()
        {
            return HandlersParts
                .Select(p => p.Metadata)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.DisplayName);
        }
    }
}