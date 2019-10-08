using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace LiteDbExplorer.Modules.DbQuery
{
    public class RawQueryHistory : INotifyPropertyChanged
    {
        private string _databaseLocation;

        public string GroupKey { get; set; }

        public string DatabaseLocation
        {
            get => _databaseLocation;
            set
            {
                _databaseLocation = value;
                Name = System.IO.Path.GetFileName(_databaseLocation);
            }
        }

        public DateTime? CreatedAt { get; set; }

        public DateTime? LastRunAt { get; set; }

        public string RawQuery { get; set; }

        [JsonIgnore]
        public string Name { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class RawSourceEqualityComparer : IEqualityComparer<RawQueryHistory>
        {
            public bool Equals(RawQueryHistory x, RawQueryHistory y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return string.Equals(x.DatabaseLocation, y.DatabaseLocation, StringComparison.OrdinalIgnoreCase) && string.Equals(x.RawQuery, y.RawQuery, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(RawQueryHistory obj)
            {
                unchecked
                {
                    return ((obj.DatabaseLocation != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.DatabaseLocation) : 0) * 397) ^ (obj.RawQuery != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.RawQuery) : 0);
                }
            }
        }

        public static IEqualityComparer<RawQueryHistory> RawSourceComparer { get; } = new RawSourceEqualityComparer();
    }
}