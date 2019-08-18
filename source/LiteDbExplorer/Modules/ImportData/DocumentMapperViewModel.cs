using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDB;

namespace LiteDbExplorer.Modules.ImportData
{
    [Export(typeof(DocumentMapperViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DocumentMapperViewModel : Screen, IStepsScreen
    {
        public DocumentMapperViewModel()
        {
            DisplayName = "Import fields";

            BsonTypes = Enum.GetValues(typeof(BsonType))
                .Cast<BsonType>()
                .Except(new[]{ BsonType.MinValue, BsonType.MaxValue });

            Init();
        }

        public IEnumerable<BsonType> BsonTypes { get; private set; }

        public IObservableCollection<string> SourceFields { get; private set; }

        public IObservableCollection<string> TargetFields { get; private set; }

        public IObservableCollection<DocumentToDocumentMap> MappingSet { get; private set; }

        private void Init()
        {
            SourceFields = new BindableCollection<string>
            {
                "_id",
                "Name",
                "Age",
                "IsEnable",
            };

            TargetFields = new BindableCollection<string>
            {
                "_id",
                "Name",
                "IsEnable",
            };

            var mappingSet = new BindableCollection<DocumentToDocumentMap>();
            // Fill from source
            foreach (var sourceField in SourceFields)
            {
                var documentMap = new DocumentToDocumentMap
                {
                    SourceKey = sourceField,
                    SourceIsReadonly = true
                };

                mappingSet.Add(documentMap);
            }

            // Fill from target
            foreach (var targetField in TargetFields)
            {
                var documentMap = mappingSet.FirstOrDefault(p => p.SourceKey.Equals(targetField, StringComparison.OrdinalIgnoreCase));
                if (documentMap != null)
                {
                    documentMap.TargetKey = targetField;
                }
                else
                {
                    documentMap = new DocumentToDocumentMap
                    {
                        TargetKey = targetField
                    };
                    mappingSet.Add(documentMap);
                }
            }

            foreach (var documentMap in mappingSet)
            {
                if (documentMap.SourceKey != null && documentMap.TargetKey != null)
                {
                    documentMap.Active = true;
                }
            }

            MappingSet = mappingSet;
        }

        public bool HasNext => false;

        public object Next()
        {
            return null;
        }

        public bool Validate()
        {
            return true;
        }
    }

    public class DocumentKeyInfo
    {
        public DocumentKeyInfo()
        {
        }

        public DocumentKeyInfo(string name, BsonType bsonType, bool exists)
        {
            Name = name;
            BsonType = bsonType;
            Exists = exists;
        }

        public string Name { get; set; }

        public BsonType BsonType { get; set; }

        public bool Exists { get; set; }
    }

    public class DocumentToDocumentMap : INotifyPropertyChanged
    {
        public bool Active { get; set; }

        public string SourceKey { get; set; }

        public string TargetKey { get; set; }

        public BsonType? FieldType { get; set; }

        public bool SourceIsReadonly { get; set; }

        public bool SourceIsEditable => !SourceIsReadonly;

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}