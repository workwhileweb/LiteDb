using System.Collections.Generic;
using System.Linq;

namespace LiteDbExplorer.Core
{
    public abstract class ReferenceNodeChangeEventArgs<T> where T : IReferenceNode
    {
        protected ReferenceNodeChangeEventArgs()
        {
            Action = ReferenceNodeChangeAction.None;
            Items = new T[0];
        }

        protected ReferenceNodeChangeEventArgs(ReferenceNodeChangeAction action, IReadOnlyCollection<T> items)
        {
            Action = action;
            Items = items;
        }

        protected ReferenceNodeChangeEventArgs(ReferenceNodeChangeAction action, T item) 
            : this(action, new []{ item })
        {

        }
        
        public virtual ReferenceNodeChangeAction Action { get; }

        public virtual IReadOnlyCollection<T> Items { get; }

        public string PostAction { get; set; }

        public virtual bool ContainsReference(T item)
        {
            return Items != null && Items.Any(p => p.InstanceId.Equals(item.InstanceId));
        }
    }
}