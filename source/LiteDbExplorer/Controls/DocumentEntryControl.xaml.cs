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
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Wpf.Behaviors;

namespace LiteDbExplorer.Controls
{
    public class DocumentFieldData : INotifyPropertyChanged
    {
        public DocumentFieldData(string name, FrameworkElement editControl, BsonType bsonType, bool isReadonly = false)
        {
            Name = name;
            EditControl = editControl;
            BsonType = bsonType;
            IsReadOnly = isReadonly;
        }

        public string Name { get; set; }

        public FrameworkElement EditControl { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsEnable => !IsReadOnly;

        public BsonType BsonType { get; set; }

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
            nameof(PreviousItem),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.PageUp)
            }
        );

        public static readonly RoutedUICommand NextItem = new RoutedUICommand
        (
            "Next Item",
            nameof(NextItem),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.PageDown)
            }
        );

        public static readonly RoutedUICommand OkCommand = new RoutedUICommand
        (
            "Save",
            nameof(OkCommand),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand CancelCommand = new RoutedUICommand
        (
            "Cancel",
            nameof(CancelCommand),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.Escape)
            }
        );

        public static readonly RoutedUICommand NewCommand = new RoutedUICommand
        (
            "New Field",
            nameof(NewCommand),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.N, ModifierKeys.Control)
            }
        );

        public static readonly Dictionary<BsonType, Func<BsonValue>> FieldTypesMap =
            new Dictionary<BsonType, Func<BsonValue>>
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

        public static readonly DependencyProperty DocumentReferenceProperty = DependencyProperty.Register(
            nameof(DocumentReference), typeof(DocumentReference), typeof(DocumentEntryControl),
            new PropertyMetadata(default(DocumentReference), OnDocumentReferenceChanged));

        private readonly string _dialogHostIdentifier;
        private readonly WindowController _windowController;
        private BsonDocument _currentDocument;
        private DocumentReference _documentReference;
        private ObservableCollection<DocumentFieldData> _entryControls;
        private bool _loaded = false;
        private Dictionary<string, BsonType> _allFieldsWithTypes;

        public DocumentEntryControl()
        {
            InitializeComponent();

            dialogHost.Identifier = _dialogHostIdentifier = $"DocumentEntryDialogHost_{Guid.NewGuid()}";

            ListItems.Loaded += ListItemsOnLoaded;

            AddNewFieldCommand = new RelayCommand<BsonType?>(async type => await AddNewFieldHandler(type));

            AddExistingFieldCommand = new RelayCommand<KeyValuePair<string, BsonType>?>(async pair => await AddExistingFieldHandler(pair));

            AddAllExistingFieldCommand = new RelayCommand(async _ => await AddAllExistingFieldHandler());

            LoadNewFieldsPicker();

            Interaction.GetBehaviors(DropNewField).Add(new ButtonClickOpenMenuBehavior());

            AddExistingFieldsButton.IsEnabled = false;

            Interaction.GetBehaviors(AddExistingFieldsButton).Add(new ButtonClickOpenMenuBehavior());
        }

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
                _entryControls.Add(NewField(key, IsReadOnly));
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

        public RelayCommand<BsonType?> AddNewFieldCommand { get; }

        public RelayCommand<KeyValuePair<string, BsonType>?> AddExistingFieldCommand { get; }

        public RelayCommand AddAllExistingFieldCommand { get; }

        public DocumentReference DocumentReference
        {
            get => (DocumentReference) GetValue(DocumentReferenceProperty);
            set => SetValue(DocumentReferenceProperty, value);
        }

        public bool IsReadOnly { get; }

        public bool DialogResult { get; set; }

        private void ListItemsOnLoaded(object sender, RoutedEventArgs e)
        {
            if (_loaded)
            {
                return;
            }

            InvalidateItemsSize();

            _loaded = true;

            ListItems.Focus();
        }

        private static void OnDocumentReferenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DocumentEntryControl;
            var documentReference = e.NewValue as DocumentReference;
            control?.LoadDocument(documentReference);
        }

        public void LoadDocument(DocumentReference document)
        {
            if (document.Collection is FileCollectionReference reference)
            {
                IsFileCollection = true;
                var fileInfo = reference.GetFileObject(document);
                GroupFile.Visibility = Visibility.Visible;
                FileView.LoadFile(fileInfo);
            }

            _currentDocument = document.FindFromCollectionRef();
            _documentReference = document;
            _entryControls = new ObservableCollection<DocumentFieldData>();

            for (var i = 0; i < document.LiteDocument.Keys.Count; i++)
            {
                var key = document.LiteDocument.Keys.ElementAt(i);
                var fieldData = NewField(key, IsReadOnly);
                _entryControls.Add(fieldData);
            }

            ListItems.ItemsSource = _entryControls;

            LoadExistingFieldsPicker();
        }

        public bool IsFileCollection { get; private set; }

        private void LoadNewFieldsPicker()
        {
            var addNewFieldContextMenu = new ContextMenu
            {
                MinWidth = 160
            };

            foreach (var item in FieldTypesMap)
            {
                var menuItem = new MenuItem
                {
                    Header = item.Key,
                    Command = AddNewFieldCommand,
                    CommandParameter = item.Key
                };
                addNewFieldContextMenu.Items.Add(menuItem);
            }

            DropNewField.ContextMenu = addNewFieldContextMenu;
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
            _allFieldsWithTypes = _documentReference.Collection
                .Items
                .SelectMany(p => p.LiteDocument.RawValue)
                .GroupBy(p => p.Key)
                .Select(p => new {p.First().Key, Value = p.First().Value.Type})
                .Where(
                    p => !currentDocumentKeys.Contains(p.Key) &&
                         (p.Value == BsonType.Null || FieldTypesMap.ContainsKey(p.Value))
                )
                .ToDictionary(p => p.Key, p => p.Value);

            if (_allFieldsWithTypes.Any())
            {
                var existingFieldsPickerMenu = new ContextMenu
                {
                    MinWidth = 160
                };

                foreach (var item in _allFieldsWithTypes)
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

                existingFieldsPickerMenu.Items.Add(new Separator());
                var allAllMenuItem = new MenuItem
                {
                    Header = "Add all",
                    Command = AddAllExistingFieldCommand
                };
                existingFieldsPickerMenu.Items.Add(allAllMenuItem);

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

            var bsonValue = _currentDocument[key];

            var editorAllowEditId = Properties.Settings.Default.DocumentEditor_AllowEditId;
            var isReadOnly = readOnly || (bsonValue.IsObjectId && !editorAllowEditId) || (IsFileCollection && key.Equals("_id") && !editorAllowEditId);

            if (bsonValue.IsNull)
            {
                var docTypePicker = GetNullValueEditor(key, isReadOnly, expandMode);
                return new DocumentFieldData(key, docTypePicker, bsonValue.Type);
            }

            var valueEdit =
                BsonValueEditor.GetBsonValueEditor(new BsonValueEditorContext(expandMode, $"[{key}]", bsonValue, _currentDocument, isReadOnly, key));

            return new DocumentFieldData(key, valueEdit, bsonValue.Type, isReadOnly);
        }

        private FrameworkElement GetNullValueEditor(string key, bool readOnly, OpenEditorMode expandMode)
        {
            var docTypePicker = new Button
            {
                Content = "(null)",
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
                        BsonValueEditor.GetBsonValueEditor(new BsonValueEditorContext(expandMode, $"[{key}]", _currentDocument[key], _currentDocument, readOnly, key));
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

        private IEnumerable<MenuItem> GetFieldToggleMenuItems(string name,
            Action<KeyValuePair<string, BsonType>?> clickAction)
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

        public event EventHandler CloseRequested;

        private void Close()
        {
            OnCloseRequested();

            _windowController?.Close(DialogResult);
        }

        private void CancelCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (dialogHost.IsOpen)
            {
                dialogHost.CurrentSession?.Close(false);
                e.Handled = true;
                return;    
            }

            DialogResult = false;
            Close();
        }

        private void OkCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (dialogHost.IsOpen)
            {
                dialogHost.CurrentSession?.Close(true);
                e.Handled = true;
                return;
            }

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

            if (_documentReference != null && _currentDocument != null)
            {
                _currentDocument.CopyTo(_documentReference.LiteDocument);
                _documentReference.LiteDocument = _currentDocument;
                _documentReference.Collection.UpdateDocument(_documentReference);

                _documentReference.NotifyDocumentChanged();
            }

            DialogResult = true;

            Close();
        }

        private void ReadOnly_CanNotExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsReadOnly;
        }

        private void NewCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (dialogHost.IsOpen)
            {
                e.Handled = true;
                return;    
            }

            if (DropNewField.ContextMenu != null)
            {
                DropNewField.ContextMenu.IsOpen = true;
                DropNewField.ContextMenu.Focus();
                DropNewField.ContextMenu.Items.OfType<MenuItem>().FirstOrDefault()?.Focus();
            }
        }

        private async Task AddNewFieldHandler(BsonType? bsonType)
        {
            if (!bsonType.HasValue)
            {
                return;
            }

            var maybeFieldName =
                await InputDialogView.Show(_dialogHostIdentifier, "Enter name of new field.", "New field name");

            if (maybeFieldName.HasNoValue)
            {
                return;
            }

            var fieldName = maybeFieldName.Value.Trim();
            if (fieldName.Any(char.IsWhiteSpace))
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

            await AddNewField(fieldName, bsonType.Value);
        }

        private async Task AddExistingFieldHandler(KeyValuePair<string, BsonType>? keyValuePair)
        {
            if (keyValuePair.HasValue)
            {
                await AddNewField(keyValuePair.Value.Key, keyValuePair.Value.Value);
            }
        }

        private async Task AddAllExistingFieldHandler()
        {
            if (_allFieldsWithTypes != null)
            {
                var count = _allFieldsWithTypes.Count;
                foreach (var pair in _allFieldsWithTypes)
                {
                    count--;
                    await AddNewField(pair.Key, pair.Value, count == 0);
                }
            }
        }


        private async Task AddNewField(string fieldName, BsonType bsonType, bool isLast = true)
        {
            var newValue = GetFieldDefaultValue(bsonType);

            _currentDocument.Add(fieldName, newValue);

            var newField = NewField(fieldName, false);

            _entryControls.Add(newField);

            if (isLast)
            {
                ItemsField_SizeChanged(ListItems, null);

                LoadExistingFieldsPicker();

                await Task.Delay(150);

                ListItems.ScrollIntoView(newField);
                newField.EditControl.Focus();
            }
        }

        private void RemoveField(string name)
        {
            var item = _entryControls.First(a => a.Name == name);
            _entryControls.Remove(item);
            _currentDocument.Remove(name);

            LoadExistingFieldsPicker();
        }

        private void ItemsField_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*if (_invalidatingSize)
            {
                return;
            }

            _invalidatingSize = true;

            var listView = sender as ListView;
            var grid = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 50;
            var col1 = 0.34;
            var col2 = 0.66;

            grid.Columns[0].Width = workingWidth*col1;
            grid.Columns[1].Width = workingWidth*col2;

            if (_loaded)
            {
                await Task.Delay(25);
            }

            _invalidatingSize = false;*/
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