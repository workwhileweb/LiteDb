using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public class UserDefinedCultureFormatDataGridBehavior : Behavior<DataGrid>
    {
        public UserDefinedCultureFormat CultureFormat { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();

            CultureFormat = UserDefinedCultureFormat.Default;

            AssociatedObject.AutoGeneratingColumn += OnAutoGeneratingColumn;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.AutoGeneratingColumn -= OnAutoGeneratingColumn;

            CultureFormat = null;

            base.OnDetaching();
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (CultureFormat == null)
            {
                return;
            }

            if (e.PropertyType == typeof(System.DateTime) && e.Column is DataGridTextColumn dataGridTextColumn)
            {
                // dataGridTextColumn.Binding.StringFormat = CultureFormat.DateTimeFormat;
                dataGridTextColumn.Binding = new Binding(e.PropertyName) {StringFormat = CultureFormat.DateTimeFormat, ConverterCulture = CultureFormat.Culture};
            }

        }
    }
}