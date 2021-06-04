using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LiteDbExplorer.Core
{
    public sealed class Store : INotifyPropertyChanged
    {
        private static readonly Lazy<Store> _current = new Lazy<Store>(() => new Store());

        private Store()
        {
            Databases = new ObservableCollection<IDatabaseReference>();
        }

        public static Store Current => _current.Value;

        public ObservableCollection<IDatabaseReference> Databases { get; private set; }

        public void AddDatabase(IDatabaseReference databaseReference)
        {
            Databases.Add(databaseReference);
        }

        public void CloseDatabase(IDatabaseReference databaseReference)
        {
            if(databaseReference == null)
            {
                return;
            }

            databaseReference?.BeforeDispose();
            
            Databases.Remove(databaseReference);

            databaseReference?.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void CloseDatabases()
        {
            foreach (var db in Databases)
            {
                db.Dispose();
            }

            Databases = new ObservableCollection<IDatabaseReference>();
        }

        public bool IsDatabaseOpen(string path)
        {
            return Databases.FirstOrDefault(a => a.Location == path) != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}