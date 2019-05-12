using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private readonly WindowController _windowController;

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

        public ObservableCollection<ArrayUIItem> Items
        {
            get; set;
        }

        public BsonArray EditedItems;

        public bool IsReadOnly { get; } = false;

        public bool DialogResult { get; set; }

        public event EventHandler CloseRequested;

        public ArrayEntryControl(BsonArray array, bool readOnly, WindowController windowController = null)
        {
            _windowController = windowController;

            InitializeComponent();

            IsReadOnly = readOnly;

            Items = new ObservableCollection<ArrayUIItem>();

            AddNewFieldCommand = new RelayCommand<BsonType?>(async type => await AddNewFieldHandler(type));

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

            var index = 0;
            foreach (BsonValue item in array)
            {
                Items.Add(NewItem(item, index));
                index++;
            }

            ItemsItems.ItemsSource = Items;

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
        }

        public RelayCommand<BsonType?> AddNewFieldCommand { get; set; }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            var value = (sender as Control).Tag as BsonValue;
            Items.Remove(Items.First(a => a.Value == value));
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
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
                        if (binding.IsDirty)
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
                var valueEdit = BsonValueEditor.GetBsonValueEditor(
                    openMode: expandMode, 
                    bindingPath: @"Value", 
                    bindingValue: value, 
                    bindingSource: arrayItem,
                    readOnly: IsReadOnly, 
                    keyName: keyName);

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

        private Task AddNewFieldHandler(BsonType? bsonType)
        {
            ButtonAddItem.IsPopupOpen = false;

            if (!bsonType.HasValue)
            {
                return Task.CompletedTask;
            }

            var newValue = GetFieldDefaultValue(bsonType.Value);

            var newItem = NewItem(newValue, Items.Count);
            Items.Add(newItem);
            newItem.Control.Focus();
            newItem.Control.BringIntoView();

            return Task.CompletedTask;
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
