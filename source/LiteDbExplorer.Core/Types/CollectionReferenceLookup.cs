using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LiteDbExplorer.Core
{
    public class CollectionReferenceLookup : IReferenceNode, INotifyPropertyChanged
    {
        public CollectionReferenceLookup(string name) : 
            this(name == @"_files" ? CollectionHandlerType.Files : CollectionHandlerType.Documents, name)
        {
            Name = name;
        }

        public CollectionReferenceLookup(CollectionHandlerType type, string name)
        {
            InstanceId = Guid.NewGuid().ToString("D");
            Type = type;
            Name = name;
        }

        public string InstanceId { get; }

        public CollectionHandlerType Type { get; }

        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}