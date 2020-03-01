using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using LiteDB;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Wpf.Behaviors;

namespace LiteDbExplorer.Controls
{
    public class ArrayUIItem
    {
        public string Name { get; set; }

        public FrameworkElement Control { get; set; }

        public BsonValue Value { get; set; }

        public int? Index { get; set; }
    }

    /// <summary>
    /// Interaction logic for ArrayViewerControl.xaml
    /// </summary>
    public partial class ArrayEntryControl : UserControl
    {
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

        private readonly WindowController _windowController;

        public ArrayEntryControl(BsonArray array, bool readOnly, WindowController windowController = null)
        {
            _windowController = windowController;

            InitializeComponent();

            IsReadOnly = readOnly;

            Items = new ObservableCollection<ArrayUIItem>();

            AddNewFieldCommand = new RelayCommand<BsonType?>(async type => await AddNewFieldHandler(type));

            var index = 0;
            foreach (BsonValue item in array)
            {
                Items.Add(NewItem(item, index));
                index++;
            }

            ItemsItems.ItemsSource = Items;

            LoadNewFieldsPicker();

            Interaction.GetBehaviors(ButtonAddItem).Add(new ButtonClickOpenMenuBehavior());

            if (readOnly)
            {
                ButtonClose.Visibility = Visibility.Visible;
                ButtonOK.Visibility = Visibility.Collapsed;
                ButtonCancel.Visibility = Visibility.Collapsed;
                ButtonAddItem.Visibility = Visibility.Collapsed;
            }

            if (_windowController != null)
            {
                ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }

            Loaded += (sender, args) =>
            {
                ItemsItems.Focus();
            };
        }


        public ObservableCollection<ArrayUIItem> Items { get; set; }

        public BsonArray EditedItems { get; set; }

        public bool IsReadOnly { get; } = false;

        public bool DialogResult { get; set; }

        public RelayCommand<BsonType?> AddNewFieldCommand { get; set; }

        public event EventHandler CloseRequested;

        private void LoadNewFieldsPicker()
        {
            var addNewFieldContextMenu = new ContextMenu
            {
                MinWidth = 160
            };

            if (Items != null)
            {
                var topBsonTypes = Items
                    .Where(p => p.Value != null)
                    .Select(p => p.Value.Type)
                    .GroupBy(_ => _)
                    .OrderByDescending(p => p.Count())
                    .Select(p => p.First())
                    .ToList();

                foreach (var topBsonType in topBsonTypes)
                {
                    var menuItem = new MenuItem
                    {
                        Header = topBsonType,
                        Command = AddNewFieldCommand,
                        CommandParameter = topBsonType
                    };
                    addNewFieldContextMenu.Items.Add(menuItem);
                }
            }

            if (addNewFieldContextMenu.Items.Count > 0)
            {
                addNewFieldContextMenu.Items.Add(new Separator());
            }

            foreach (var item in FieldTypesMap)
            {
                if (addNewFieldContextMenu.Items
                    .OfType<MenuItem>()
                    .Any(p => p.Header.ToString() == item.Key.ToString()))
                {
                    continue;
                }

                var menuItem = new MenuItem
                {
                    Header = item.Key,
                    Command = AddNewFieldCommand,
                    CommandParameter = item.Key
                };
                addNewFieldContextMenu.Items.Add(menuItem);
            }

            ButtonAddItem.ContextMenu = addNewFieldContextMenu;
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            var value = (sender as Control).Tag as BsonValue;
            Items.Remove(Items.First(a => a.Value == value));

            LoadNewFieldsPicker();
        }

        private void CancelCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //TODO make array and document types use this as well
            foreach (var control in Items.Select(a => a.Control))
            {
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

            DialogResult = true;
            EditedItems = new BsonArray(Items.Select(a => a.Value));
            Close();
        }

        private void ReadOnly_CanNotExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsReadOnly;
        }

        private void NewCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ButtonAddItem.ContextMenu != null)
            {
                ButtonAddItem.ContextMenu.IsOpen = true;
                ButtonAddItem.ContextMenu.Focus();
                ButtonAddItem.ContextMenu.Items.OfType<MenuItem>().FirstOrDefault()?.Focus();
            }
        }

        private void Close()
        {
            OnCloseRequested();

            _windowController?.Close(DialogResult);
        }

        public ArrayUIItem NewItem(BsonValue value, int? index)
        {
            var keyName = value.Type.ToString();
            var arrayItem = new ArrayUIItem
            {
                Name = $"{index}:{keyName}",
                Value = value,
                Index = index
            };

            var expandMode = OpenEditorMode.Inline;
            if (_windowController != null)
            {
                expandMode = OpenEditorMode.Window;
            }

            if (value.IsNull && index.HasValue)
            {
                arrayItem.Control = GetNullValueEditor(index.Value, IsReadOnly);
            }
            else
            {
                var editorContext = new BsonValueEditorContext(openMode: expandMode, bindingPath: @"Value", bindingValue: value, bindingSource: arrayItem, readOnly: IsReadOnly, keyName: keyName);
                var valueEdit = BsonValueEditor.GetBsonValueEditor(editorContext, UserDefinedCultureFormat.Default);

                arrayItem.Control = valueEdit;
            }

            return arrayItem;
        }

        private FrameworkElement GetNullValueEditor(int index, bool readOnly)
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

            var fieldToggleMenuItems = new List<MenuItem>();
            foreach (var bsonType in FieldTypesMap.Keys)
            {
                void ClickAction(BsonType? type)
                {
                    if (!type.HasValue)
                    {
                        return;
                    }

                    var value = GetFieldDefaultValue(type.Value);
                    Items[index] = NewItem(value, index);
                }

                var menuItem = new MenuItem
                {
                    Header = bsonType.ToString(),
                    Command = new RelayCommand<BsonType?>(ClickAction),
                    CommandParameter = bsonType,
                };
                fieldToggleMenuItems.Add(menuItem);
            }

            var typeMenuPicker = new ContextMenu();
            foreach (var menuItem in fieldToggleMenuItems)
            {
                typeMenuPicker.Items.Add(menuItem);
            }

            docTypePicker.ContextMenu = typeMenuPicker;
            Interaction.GetBehaviors(docTypePicker).Add(new ButtonClickOpenMenuBehavior());

            return docTypePicker;
        }

        private async Task AddNewFieldHandler(BsonType? bsonType)
        {
            if (!bsonType.HasValue)
            {
                return;
            }

            var newValue = GetFieldDefaultValue(bsonType.Value);

            var newItem = NewItem(newValue, Items.Count);
            Items.Add(newItem);

            LoadNewFieldsPicker();

            await Task.Delay(150);

            newItem.Control.Focus();
            newItem.Control.BringIntoView();
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

        protected virtual void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}