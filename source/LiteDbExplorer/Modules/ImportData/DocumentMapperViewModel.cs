using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDB;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.ImportData
{
    [Export(typeof(DocumentMapperViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DocumentMapperViewModel : Screen, IStepsScreen
    {
        private HashSet<string> _targetFields;
        private HashSet<string> _sourceFields;
        private Dictionary<string, BsonType> _targetFieldsWithTypes;
        private IObservableCollection<DocumentToDocumentMap> _mappingSet;

        public DocumentMapperViewModel()
        {
            DisplayName = "Import fields";

            ValidBsonTypes = Enum.GetValues(typeof(BsonType))
                .Cast<BsonType>()
                .Except(new[] {BsonType.MinValue, BsonType.MaxValue, BsonType.Document, BsonType.Array});

            SourceFields = new BindableCollection<string>();

            TargetFields = new BindableCollection<string>();
        }

        public bool CanContentScroll => true;

        public IEnumerable<BsonType> ValidBsonTypes { get; private set; }

        public IObservableCollection<string> SourceFields { get; private set; }

        public IObservableCollection<string> TargetFields { get; private set; }

        public IObservableCollection<DocumentToDocumentMap> MappingSet
        {
            get => _mappingSet;
            private set
            {
                if (_mappingSet != null)
                {
                    foreach(var item in _mappingSet)
                    {
                        item.PropertyChanged -= OnDocumentMapItemPropertyChanged;
                    }
                    _mappingSet.CollectionChanged -= OnMappingSetCollectionChanged;
                }

                _mappingSet = value;

                foreach(var item in _mappingSet)
                {
                    item.PropertyChanged += OnDocumentMapItemPropertyChanged;
                }

                _mappingSet.CollectionChanged += OnMappingSetCollectionChanged;
            }
        }

        public void Init(IEnumerable<string> sourceFields, CollectionReference targetCollection)
        {
            _sourceFields = sourceFields?.ToHashSet() ?? new HashSet<string>();

            if (targetCollection != null)
            {
                _targetFieldsWithTypes = targetCollection
                    .Items
                    .SelectMany(p => p.LiteDocument.RawValue)
                    .GroupBy(p => p.Key)
                    .Select(p => new {p.First().Key, Value = p.First().Value.Type})
                    // .Where(p => ValidBsonTypes.Contains(p.Value))
                    .ToDictionary(p => p.Key, p => p.Value);

                _targetFields = _targetFieldsWithTypes.Select(p => p.Key).ToHashSet();
            }
            else
            {
                _targetFields = new HashSet<string>();
                _targetFieldsWithTypes = new Dictionary<string, BsonType>();                
            }

            SourceFields.Clear();
            SourceFields.AddRange(_sourceFields);
            foreach (var sourceField in _sourceFields.Select(p => p.RemoveSpaces().WithoutDiacritics()))
            {
                if (_targetFields.Any(p => p.TrimStart('_').Equals(sourceField.TrimStart('_'), StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                _targetFields.Add(sourceField);
            }

            TargetFields.Clear();
            if (_targetFields != null)
            {
                TargetFields.AddRange(_targetFields);
            }

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
                var documentMap = mappingSet.FirstOrDefault(
                    p => p.SourceKey != null && p.SourceKey.TrimStart('_').Equals(targetField.TrimStart('_'), StringComparison.OrdinalIgnoreCase)
                );

                BsonType? fieldType = null;
                if (_targetFieldsWithTypes.TryGetValue(targetField, out var bsonType))
                {
                    fieldType = ValidBsonTypes.Contains(bsonType) ? bsonType : BsonType.Null;
                }

                if (documentMap != null)
                {
                    documentMap.TargetKey = targetField;
                    documentMap.FieldType = fieldType;
                    documentMap.TargetExists = fieldType.HasValue;
                }
                else
                {
                    documentMap = new DocumentToDocumentMap
                    {
                        TargetKey = targetField,
                        FieldType = fieldType,
                        TargetExists = fieldType.HasValue
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
        

        public Task<object> Next()
        {
            return null;
        }

        public bool Validate()
        {
            return true;
        }

        private void OnMappingSetCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach(DocumentToDocumentMap item in e.NewItems)
                {
                    item.PropertyChanged += OnDocumentMapItemPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach(DocumentToDocumentMap item in e.OldItems)
                {
                    item.PropertyChanged -= OnDocumentMapItemPropertyChanged;
                }
            }
        }

        private void OnDocumentMapItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is DocumentToDocumentMap documentMap))
            {
                return;
            }

            if (e.PropertyName.Equals(nameof(DocumentToDocumentMap.TargetKey)))
            {
                if (!string.IsNullOrEmpty(documentMap.TargetKey) && _targetFieldsWithTypes.TryGetValue(documentMap.TargetKey, out var bsonType))
                {
                    documentMap.FieldType = ValidBsonTypes.Contains(bsonType) ? bsonType : BsonType.Null;
                    documentMap.TargetExists = true;
                }
                else
                {
                    documentMap.FieldType = null;
                    documentMap.TargetExists = false;
                }
            }

            documentMap.Active = documentMap.HasRequiredValues;
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

        public bool TargetExists { get; set; }

        public bool SourceIsReadonly { get; set; }

        public bool SourceIsEditable => !SourceIsReadonly;

        public bool HasRequiredValues => !string.IsNullOrEmpty(SourceKey) && !string.IsNullOrEmpty(TargetKey) && FieldType.HasValue;

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}