using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public class EscapeAccessKeyColumnHeaderBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AutoGeneratingColumn += OnAutoGeneratingColumn;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.AutoGeneratingColumn -= OnAutoGeneratingColumn;
            base.OnDetaching();
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header != null)
            {
                var header = e.Column.Header.ToString();

                // Replace all underscores with two underscores, to prevent AccessKey handling
                e.Column.Header = header.Replace("_", "__");
            }
        }
    }
}