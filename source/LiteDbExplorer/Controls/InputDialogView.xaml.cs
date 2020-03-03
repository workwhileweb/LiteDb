using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for InputDialogView.xaml
    /// </summary>
    public partial class InputDialogView : UserControl, INotifyPropertyChanged
    {
        private readonly Func<string, Result> _validateCallback;

        public InputDialogView()
        {
            InitializeComponent();

            DataContext = this;

            ValueTextBox.SetBinding(TextBox.TextProperty, new Binding
            {
                Path = new PropertyPath(nameof(Text)),
                Mode = BindingMode.TwoWay,
                ValidatesOnExceptions = true,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = this
            });
        }

        public string Text { get; set; }

        public string Hint { get; private set; } = string.Empty;

        public string Message { get; private set; } = string.Empty;

        // public string ValidationErrorText { get; private set; }

        public InputDialogView(string message, string caption, string predefined = "", Func<string, Result> validationFunc = null) : this()
        {
            Hint = caption ?? string.Empty;
            Message = message ?? string.Empty;
            Text = predefined ?? string.Empty;
            _validateCallback = validationFunc;
        }

        public bool Validate()
        {
            // ValidationErrorText = string.Empty;
            
            if (_validateCallback != null)
            {
                var result = _validateCallback(Text);
                if (result.IsFailure)
                {
                    SetSearchInlineError(true, result.Error);
                }
                else
                {
                    SetSearchInlineError(false);
                }
                // ValidationErrorText = result.Error;
                return result.IsSuccess;
            }

            return true;
        }

        private void SetSearchInlineError(bool isInvalid, string message = "")
        {
            var bindingExpressionBase = ValueTextBox?.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpressionBase == null)
            {
                return;
            }

            if (isInvalid)
            {
                var validationError =
                    new ValidationError(new ExceptionValidationRule(), bindingExpressionBase)
                    {
                        ErrorContent = message
                    };


                Validation.MarkInvalid(bindingExpressionBase, validationError);
            }
            else
            {
                Validation.ClearInvalid(bindingExpressionBase);
            }

        }

        public static async Task<Maybe<string>> Show(string dialogIdentifier, string message, string caption, string predefined = "", Func<string, Result> validationFunc = null)
        {
            var dialogView = new InputDialogView(message, caption, predefined, validationFunc);
            var result = await DialogHost.Show(dialogView, dialogIdentifier,
                delegate(object sender, DialogClosingEventArgs args)
                {
                    if ((bool)args.Parameter && !dialogView.Validate())
                    {
                        args.Cancel();
                        args.Handled = true;
                    }
                });

            if ((bool)result)
            {
                return Maybe<string>.From(dialogView.Text);
            }

            return Maybe<string>.None;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
