using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Forge.Forms.Controls;

namespace LiteDbExplorer.Modules.Shared
{
    /// <summary>
    /// Interaction logic for DynamicFormView.xaml
    /// </summary>
    public partial class DynamicFormView : UserControl
    {
        public DynamicFormView()
        {
            InitializeComponent();

            dynamicForm.SetBinding(DynamicForm.ModelProperty, new Binding()
            {
                Path = new PropertyPath(nameof(FormModelContext)),
                Source = this
            });
        }

        public DynamicFormView(object formModelContext) : this()
        {
            FormModelContext = formModelContext;
        }

        public static readonly DependencyProperty FormModelContextProperty = DependencyProperty.Register(
            nameof(FormModelContext), typeof(object), typeof(DynamicFormView), new PropertyMetadata(default(object)));

        public object FormModelContext
        {
            get => (object) GetValue(FormModelContextProperty);
            set => SetValue(FormModelContextProperty, value);
        }
    }
}
