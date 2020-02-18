using System;

namespace LiteDbExplorer.Core
{
    public class ReferenceChangedEventArgs<T> : EventArgs
    {
        public ReferenceChangedEventArgs(ReferenceNodeChangeAction action, T reference)
        {
            Action = action;
            Reference = reference;
        }

        public ReferenceNodeChangeAction Action { get; }
        public T Reference { get; }
    }
}