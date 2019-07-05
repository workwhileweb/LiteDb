using System;
using System.Windows;
using System.Windows.Controls;
using CSharpFunctionalExtensions;
using MahApps.Metro.Controls;

namespace LiteDbExplorer.Windows
{
    /// <summary>
    /// Interaction logic for InputBoxWindow.xaml
    /// </summary>
    public partial class InputBoxWindow : MetroWindow
    {
        private Func<string, Result> _validateCallback;
        
        public string Text => ValueTextBox.Text;

        public string ValidationErrorText
        {
            get => ErrorTextBlock.Text;
            protected set
            {
                ErrorTextBlock.Text = value;
                ErrorTextBlock.Visibility = string.IsNullOrEmpty(ErrorTextBlock.Text) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public InputBoxWindow()
        {
            InitializeComponent();

            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        public static bool? ShowDialog(string message, string caption, string predefined, Func<string, Result> validationFunc, out string input)
        {
            return ShowDialog(message, caption, predefined, validationFunc, null, out input);
        }

        public static bool? ShowDialog(string message, string caption, string predefined, Func<string, Result> validationFunc, Window owner, out string input)
        {
            var window = new InputBoxWindow
            {
                Owner = owner,
                TextMessage = {Text = message},
                Title = caption,
                ValueTextBox = {Text = predefined},
                _validateCallback = validationFunc
            };


            var result = window.ShowDialog();
            input = window.Text;
            return result;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ValidationErrorText = string.Empty;
            if (_validateCallback != null)
            {
                var validationResult = _validateCallback(Text);
                if (validationResult.IsFailure)
                {
                    ValidationErrorText = validationResult.Error;
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ValueTextBox.Focus();
            ValueTextBox.SelectAll();
        }
    }
}
