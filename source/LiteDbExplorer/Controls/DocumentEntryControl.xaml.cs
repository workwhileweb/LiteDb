using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using JetBrains.Annotations;
using LiteDB;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Wpf.Behaviors;

namespace LiteDbExplorer.Controls
{
    public class DocumentFieldData : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public FrameworkElement EditControl { get; set; }

        public DocumentFieldData(string name, FrameworkElement editControl)
        {
            Name = name;
            EditControl = editControl;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    ///     Interaction logic for DocumentViewerControl.xaml
    /// </summary>
    public partial class DocumentEntryControl : UserControl
    {
        public static readonly RoutedUICommand PreviousItem = new RoutedUICommand
        (
            "Previous Item",
            "PreviousItem",
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.PageUp)
            }
        );

        public static readonly RoutedUICommand NextItem = new RoutedUICommand
        (
            "Next Item",
            "NextItem",
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.PageDown)
            }
        );

        public static readonly Dictionary<BsonType, Func<BsonValue>> FieldTypesMap = new Dictionary<BsonType, Func<BsonValue>>
        {
            {BsonType.String, () => new BsonValue(string.Empty)},
            {BsonType.Boolean, () => new BsonValue(false)},
            {BsonType.Double, () => new BsonValue((double) 0)},
            {BsonType.Decimal, () => new BsonValue((decimal) 0.0m)},
            {BsonType.Int32, () => new BsonValue(0)},
            {BsonType.Int64, () => new BsonValue((long) 0)},
            {BsonType.DateTime, () => new BsonValue(DateTime.Now)},
            {BsonType.Guid, () => new BsonValue(Guid.Empty)},
            {BsonType.Array, () => new BsonArray()},
            {BsonType.Document, () => new BsonDocument()},
        };

        private BsonDocument _currentDocument;
        private ObservableCollection<DocumentFieldData> _entryControls;
        private DocumentReference _documentReference;

        private bool _loaded = false;
        private bool _invalidatingSize;
        private readonly WindowController _windowController;

        public DocumentEntryControl()
        {
            InitializeComponent();

            ListItems.Loaded += (sender, args) =>
            {
                if (_loaded)
                {
                    return;
                }

                InvalidateItemsSize();

                _loaded = true;
            };

            AddNewFieldCommand = new RelayCommand<BsonType?>(async type => await AddNewFieldHandler(type));

            AddExistingFieldCommand = new RelayCommand<KeyValuePair<string, BsonType>?>(async pair => await AddExistingFieldHandler(pair));

            foreach (var item in FieldTypesMap)
            {
                var menuItem = new MenuItem
                {
                    Header = item.Key,
                    Command = AddNewFieldCommand,
                    CommandParameter = item.Key
                };
                AddFieldsTypesPanel.Children.Add(menuItem);
            }

            AddExistingFieldsButton.IsEnabled = false;

            Interaction.GetBehaviors(AddExistingFieldsButton).Add(new ButtonClickOpenMenuBehavior());
        }

        public RelayCommand<BsonType?> AddNewFieldCommand { get; }

        public RelayCommand<KeyValuePair<string, BsonType>?> AddExistingFieldCommand { get; }

        private DocumentEntryControl(WindowController windowController) : this()
        {
            _windowController = windowController;
        }

        public DocumentEntryControl(DocumentReference document, WindowController windowController = null) : this(
            windowController)
        {
            LoadDocument(document);
        }

        public DocumentEntryControl(BsonDocument document, bool readOnly, WindowController windowController = null) :
            this(windowController)
        {
            IsReadOnly = readOnly;

            _currentDocument = document;
            _entryControls = new ObservableCollection<DocumentFieldData>();

            for (var i = 0; i < document.Keys.Count; i++)
            {
                var key = document.Keys.ElementAt(i);
                _entryControls.Add(NewField(key, readOnly));
            }

            ListItems.ItemsSource = _entryControls;

            ButtonNext.Visibility = Visibility.Collapsed;
            ButtonPrev.Visibility = Visibility.Collapsed;
            AddExistingFieldsButton.Visibility = Visibility.Collapsed;

            if (readOnly)
            {
                ButtonClose.Visibility = Visibility.Visible;
                ButtonOK.Visibility = Visibility.Collapsed;
                ButtonCancel.Visibility = Visibility.Collapsed;
                DropNewField.Visibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty DocumentReferenceProperty = DependencyProperty.Register(
            nameof(DocumentReference), typeof(DocumentReference), typeof(DocumentEntryControl),
            new PropertyMetadata(default(DocumentReference), OnDocumentReferenceChanged));

        private static void OnDocumentReferenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DocumentEntryControl;
            var documentReference = e.NewValue as DocumentReference;
            control?.LoadDocument(documentReference);
        }

        public DocumentReference DocumentReference
        {
            get => (DocumentReference) GetValue(DocumentReferenceProperty);
            set => SetValue(DocumentReferenceProperty, value);
        }

        public bool IsReadOnly { get; }

        public bool DialogResult { get; set; }

        public void LoadDocument(DocumentReference document)
        {
            if (document.Collection is FileCollectionReference reference)
            {
                var fileInfo = reference.GetFileObject(document);
                GroupFile.Visibility = Visibility.Visible;
                FileView.LoadFile(fileInfo);
            }

            _currentDocument = document.Collection.LiteCollection.FindById(document.LiteDocument["_id"]);
            _documentReference = document;
            _entryControls = new ObservableCollection<DocumentFieldData>();

            for (var i = 0; i < document.LiteDocument.Keys.Count; i++)
            {
                var key = document.LiteDocument.Keys.ElementAt(i);
                _entryControls.Add(NewField(key, IsReadOnly));
            }

            ListItems.ItemsSource = _entryControls;

            LoadExistingFieldsPicker();
        }

        private void LoadExistingFieldsPicker()
        {
            AddExistingFieldsButton.IsEnabled = false;
            AddExistingFieldsButton.ContextMenu = null;

            if (_documentReference == null || _currentDocument == null)
            {
                return;
            }

            var currentDocumentKeys = _currentDocument.Keys;

            // Get distinct fields from all entries
            var allFieldsWithTypes = _documentReference.Collection
                .Items
                .SelectMany(p => p.LiteDocument.RawValue)
                .GroupBy(p => p.Key)
                .Select(p => new {p.First().Key, Value = p.First().Value.Type})
                .Where(
                    p => !currentDocumentKeys.Contains(p.Key) && 
                         (p.Value == BsonType.Null || FieldTypesMap.ContainsKey(p.Value))
                 )
                .ToDictionary(p => p.Key, p => p.Value);

            if (allFieldsWithTypes.Any())
            {
                var existingFieldsPickerMenu = new ContextMenu();
                foreach (var item in allFieldsWithTypes)
                {
                    var menuItem = new MenuItem
                    {
                        Header = $"{item.Key.Replace("_", "__")}",
                        InputGestureText = $"{item.Value}",
                        Command = AddExistingFieldCommand,
                        CommandParameter = item
                    };
                    existingFieldsPickerMenu.Items.Add(menuItem);
                }

                AddExistingFieldsButton.IsEnabled = true;
                AddExistingFieldsButton.ContextMenu = existingFieldsPickerMenu;
            }
        }

        private DocumentFieldData NewField(string key, bool readOnly)
        {
            var expandMode = OpenEditorMode.Inline;
            if (_windowController != null)
            {
                expandMode = OpenEditorMode.Window;
            }

            if (_currentDocument[key].IsNull)
            {
                var docTypePicker = GetNullValueEditor(key, readOnly, expandMode);
                return new DocumentFieldData(key, docTypePicker);
            }

            var valueEdit =
                BsonValueEditor.GetBsonValueEditor(
                    openMode: expandMode,
                    bindingPath: $"[{key}]",
                    bindingValue: _currentDocument[key],
                    bindingSource: _currentDocument,
                    readOnly: readOnly,
                    keyName: key);

            return new DocumentFieldData(key, valueEdit);
        }

        private FrameworkElement GetNullValueEditor(string key, bool readOnly, OpenEditorMode expandMode)
        {
            var docTypePicker = new Button
            {
                Content = "[Null]",
                ToolTip = "Select null field type",
                Style = StyleKit.MaterialDesignEntryButtonStyle
            };

            if (readOnly)
            {
                return docTypePicker;
            }

            var fieldToggleMenuItems = GetFieldToggleMenuItems(key, keyValuePair =>
            {
                if (keyValuePair.HasValue)
                {
                    var documentFieldData = _entryControls.First(p => p.Name == keyValuePair.Value.Key);
                    var fieldDefaultValue = GetFieldDefaultValue(keyValuePair.Value.Value);

                    _currentDocument[key] = fieldDefaultValue;
                    documentFieldData.EditControl =
                        BsonValueEditor.GetBsonValueEditor(
                            openMode: expandMode,
                            bindingPath: $"[{key}]",
                            bindingValue: _currentDocument[key],
                            bindingSource: _currentDocument,
                            readOnly: readOnly,
                            keyName: key);
                }
            });

            var typeMenuPicker = new ContextMenu();
            foreach (var menuItem in fieldToggleMenuItems)
            {
                typeMenuPicker.Items.Add(menuItem);
            }

            docTypePicker.ContextMenu = typeMenuPicker;
            Interaction.GetBehaviors(docTypePicker).Add(new ButtonClickOpenMenuBehavior());

            return docTypePicker;
        }

        private IEnumerable<MenuItem> GetFieldToggleMenuItems(string name, Action<KeyValuePair<string, BsonType>?> clickAction)
        {
            var result = new List<MenuItem>();
            foreach (var bsonType in FieldTypesMap.Keys)
            {
                var menuItem = new MenuItem
                {
                    Header = bsonType.ToString(),
                    Command = new RelayCommand<KeyValuePair<string, BsonType>?>(clickAction),
                    CommandParameter = new KeyValuePair<string, BsonType>(name, bsonType),
                };
                result.Add(menuItem);
            }

            return result;
        }

        private BsonValue GetFieldDefaultValue(BsonType bsonType)
        {
            if (bsonType == BsonType.Null)
            {
                return new BsonValue();
            }

            if (!FieldTypesMap.ContainsKey(bsonType))
            {
                throw new Exception("Uknown value type.");
            }

            return FieldTypesMap[bsonType]();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string key)
            {
                RemoveField(key);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event EventHandler CloseRequested;

        private void Close()
        {
            OnCloseRequested();

            _windowController?.Close(DialogResult);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            // TODO make array and document types use this as well
            foreach (var ctrl in _entryControls)
            {
                var control = ctrl.EditControl;
                var values = control.GetLocalValueEnumerator();
                while (values.MoveNext())
                {
                    var current = values.Current;
                    if (BindingOperations.IsDataBound(control, current.Property))
                    {
                        var binding = control.GetBindingExpression(current.Property);
                        if (binding?.IsDirty == true)
                        {
                            binding.UpdateSource();
                        }
                    }
                }
            }

            if (_documentReference != null)
            {
                _documentReference.LiteDocument = _currentDocument;
                _documentReference.Collection.UpdateItem(_documentReference);
            }

            DialogResult = true;

            Close();
        }

        private async Task AddNewFieldHandler(BsonType? bsonType)
        {
            if (!bsonType.HasValue)
            {
                return;
            }

            var maybeFieldName = await InputDialogView.Show("DocumentEntryDialogHost", "Enter name of new field.", "New field name");

            if (maybeFieldName.HasNoValue)
            {
                return;
            }

            var fieldName = maybeFieldName.Value.Trim();
            if (fieldName.Any(Char.IsWhiteSpace))
            {
                MessageBox.Show("Field name can not contain white spaces.", "", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (_currentDocument.Keys.Contains(fieldName))
            {
                MessageBox.Show($"Field \"{fieldName}\" already exists!", "", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            AddNewField(fieldName, bsonType.Value);
        }

        private Task AddExistingFieldHandler(KeyValuePair<string, BsonType>? keyValuePair)
        {
            if (keyValuePair.HasValue)
            {
                AddNewField(keyValuePair.Value.Key, keyValuePair.Value.Value);
            }

            return Task.CompletedTask;
        }

        private void AddNewField(string fieldName, BsonType bsonType)
        {
            var newValue = GetFieldDefaultValue(bsonType);
            
            _currentDocument.Add(fieldName, newValue);

            var newField = NewField(fieldName, false);

            _entryControls.Add(newField);

            ItemsField_SizeChanged(ListItems, null);
            ListItems.ScrollIntoView(newField);

            newField.EditControl.Focus();

            LoadExistingFieldsPicker();
        }

        private void RemoveField(string name)
        {
            var item = _entryControls.First(a => a.Name == name);
            _entryControls.Remove(item);
            _currentDocument.Remove(name);

            LoadExistingFieldsPicker();
        }

        private async void ItemsField_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_invalidatingSize)
            {
                return;
            }

            _invalidatingSize = true;

            var listView = sender as ListView;
            var grid = listView.View as GridView;
            var newWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10 -
                           grid.Columns[0].ActualWidth - grid.Columns[2].ActualWidth;

            if (newWidth > 0)
            {
                grid.Columns[1].Width = Math.Max(140, newWidth);
            }

            if (_loaded)
            {
                await Task.Delay(50);
            }

            _invalidatingSize = false;
        }

        private void InvalidateItemsSize()
        {
            ItemsField_SizeChanged(ListItems, null);
        }

        private void NextItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_documentReference == null)
            {
                e.CanExecute = false;
            }
            else
            {
                var index = _documentReference.Collection.Items.IndexOf(_documentReference);
                e.CanExecute = index + 1 < _documentReference.Collection.Items.Count;
            }
        }

        private void NextItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = _documentReference.Collection.Items.IndexOf(_documentReference);
            if (index + 1 < _documentReference.Collection.Items.Count)
            {
                var newDocument = _documentReference.Collection.Items[index + 1];
                LoadDocument(newDocument);
            }
        }

        private void PreviousItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_documentReference == null)
            {
                e.CanExecute = false;
            }
            else
            {
                var index = _documentReference.Collection.Items.IndexOf(_documentReference);
                e.CanExecute = index > 0;
            }
        }

        private void PreviousItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = _documentReference.Collection.Items.IndexOf(_documentReference);

            if (index > 0)
            {
                var newDocument = _documentReference.Collection.Items[index - 1];
                LoadDocument(newDocument);
            }
        }

        protected virtual void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}