using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using PropertyChanging;

namespace LiteDbExplorer.Core
{
    [ImplementPropertyChanging]
    public abstract class ReferenceNode<T> : INotifyPropertyChanging, INotifyPropertyChanged, IReferenceNode, IDisposable
    {
        protected ReferenceNode()
        {
            InstanceId = Guid.NewGuid().ToString();

            // Log.Debug("Ctor. {ReferenceType}, InstanceId {InstanceId}", GetType(), InstanceId);
        }

        public virtual string InstanceId { get; }

        public virtual bool ReferenceEquals(ReferenceNode<T> reference)
        {
            return InstanceId.Equals(reference?.InstanceId);
        }

        public void Dispose()
        {
            // Log.Debug("Dispose {ReferenceType}, InstanceId {InstanceId}", GetType(), InstanceId);

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        public event EventHandler<ReferenceChangedEventArgs<T>> ReferenceChanged;
        
        internal virtual void OnReferenceChanged(ReferenceNodeChangeAction action, T item)
        {
            OnReferenceChanged(new ReferenceChangedEventArgs<T>(action, item));
            
            if (action == ReferenceNodeChangeAction.Dispose)
            {
                Dispose();
            }
        }

        protected virtual void OnReferenceChanged(ReferenceChangedEventArgs<T> e)
        {
            ReferenceChanged?.Invoke(this, e);
        }

        [UsedImplicitly]
        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;
        
        protected virtual void OnPropertyChanging([CallerMemberName] string name = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        }

    }
}