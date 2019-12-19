using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using Forge.Forms.Controls;
using LiteDbExplorer.Wpf.Behaviors;

namespace LiteDbExplorer.Modules.Shared
{
    public class DynamicFormGrid : Grid
    {
        public class Item
        {
            public Item(object source, GridLength rowGridLength, bool directContent = false)
            {
                Source = source;
                RowGridLength = rowGridLength;
                DirectContent = directContent;
            }

            public object Source { get; set; }
            public GridLength RowGridLength { get; set; }
            public bool DirectContent { get; set; }
        }

        public DynamicFormGrid(params Item[] items)
        {
            ItemsSource = items;
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable<Item>), typeof(DynamicFormGrid), 
            new PropertyMetadata(default(IEnumerable<Item>), OnItemsSourcePropertyChanged));

        public IEnumerable<Item> ItemsSource
        {
            get => (IEnumerable<Item>) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DynamicFormGrid view)
            {
                view.UpdateItems();
            }
        }

        private void UpdateItems()
        {
            Children.Clear();
            if (ItemsSource == null)
            {
                return;
            }

            var i = 0;
            foreach (var item in ItemsSource)
            {
                var rowDefinition = new RowDefinition
                {
                    Height = item.RowGridLength
                };

                RowDefinitions.Add(rowDefinition);

                UIElement element = null;
                if (item.DirectContent)
                {
                    var contentControl = new ContentControl();
                    contentControl.SetBinding(ContentControl.ContentProperty, new Binding
                    {
                        Path = new PropertyPath($"ItemsSource[{i}].Source"),
                        Source = this
                    });
                    element = contentControl;
                }
                else
                {
                    var dynamicForm = new DynamicForm();
                    dynamicForm.SetBinding(DynamicForm.ModelProperty, new Binding
                    {
                        Path = new PropertyPath($"ItemsSource[{i}].Source"),
                        Source = this
                    });
                    Interaction.GetBehaviors(dynamicForm).Add(new BubbleScrollEvent());
                    element = dynamicForm;
                }

                SetRow(element, i);
                Children.Add(element);

                i++;
            }
        }

    }
}