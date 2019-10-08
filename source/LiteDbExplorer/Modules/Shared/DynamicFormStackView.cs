using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using Forge.Forms.Controls;
using LiteDbExplorer.Wpf.Behaviors;

namespace LiteDbExplorer.Modules.Shared
{
    public class DynamicFormStackView : StackPanel
    {
        public DynamicFormStackView()
        {
        }

        public DynamicFormStackView(params object[] sources) : this()
        {
            ItemsSource = sources;
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable<object>), 
            typeof(DynamicFormStackView), 
            new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DynamicFormStackView view)
            {
                view.UpdateItems();
            }
        }

        public IEnumerable<object> ItemsSource
        {
            get => (IEnumerable<object>) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private void UpdateItems()
        {
            Children.Clear();
            if (ItemsSource == null)
            {
                return;
            }

            for (var i = 0; i < ItemsSource.Count(); i++)
            {
                var dynamicForm = new DynamicForm();
                dynamicForm.SetBinding(DynamicForm.ModelProperty, new Binding
                {
                    Path = new PropertyPath($"ItemsSource[{i}]"),
                    Source = this
                });
                Interaction.GetBehaviors(dynamicForm).Add(new BubbleScrollEvent());
                Children.Add(dynamicForm);
            }
        }
    }
}