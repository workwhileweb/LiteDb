using System;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Controls;

namespace LiteDbExplorer.Modules.DbQuery
{
    /// <summary>
    /// Interaction logic for QueryView.xaml
    /// </summary>
    public partial class QueryView : UserControl, IOwnerViewModelMessageHandler
    {
        public QueryView()
        {
            InitializeComponent();
        }

        public void Handle(string message, object payload = null)
        {
            if (message.Equals("QueryEditorFocus", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(queryEditor.Text) && payload is string option)
                {
                    switch (option.ToLower())
                    {
                        case "end":
                            queryEditor.Select(queryEditor.Text.Length, 0);
                            break;
                        case "start":
                            queryEditor.Select(0, 0);
                            break;
                        case "all":
                            queryEditor.Select(0, queryEditor.Text.Length);
                            break;
                    }
                }

                queryEditor.Focus();
            }
        }
    }
}
