using System.Threading.Tasks;
using System.Windows.Controls;
using CSharpFunctionalExtensions;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for InputDialogView.xaml
    /// </summary>
    public partial class InputDialogView : UserControl
    {
        public InputDialogView()
        {
            InitializeComponent();

            DataContext = this;
        }

        public string Hint { get; private set; } = string.Empty;

        public string Message { get; private set; } = string.Empty;

        public InputDialogView(string message, string caption, string predefined = "") : this()
        {
            Hint = caption ?? string.Empty;
            Message = message ?? string.Empty;
            ValueTextBox.Text = predefined ?? string.Empty;
        }

        public static async Task<Maybe<string>> Show(string dialogIdentifier, string message, string caption, string predefined = "")
        {
            var dialogView = new InputDialogView(message, caption, predefined);
            var result = await DialogHost.Show(dialogView, dialogIdentifier);

            if ((bool)result)
            {
                return Maybe<string>.From(dialogView.ValueTextBox.Text);
            }

            return Maybe<string>.None;
        }
    }
}
