using System;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace LiteDbExplorer.Controls
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        internal bool InDocumentChanging;

        public static readonly DependencyProperty BindingTextProperty =
            DependencyProperty.Register(nameof(BindingText), typeof(string), typeof(AvalonEditBehaviour), 
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, BindingTextPropertyChanged));

        public string BindingText
        {
            get => (string)GetValue(BindingTextProperty);
            set => SetValue(BindingTextProperty, value);
        }


        public static readonly DependencyProperty BindingSelectedTextProperty =
            DependencyProperty.Register(nameof(BindingSelectedText), typeof(string), typeof(AvalonEditBehaviour), 
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, BindingSelectedTextPropertyChanged));

        public string BindingSelectedText
        {
            get => (string)GetValue(BindingSelectedTextProperty);
            set => SetValue(BindingSelectedTextProperty, value);
        }
        
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
                AssociatedObject.TextArea.SelectionChanged += TextAreaOnSelectionChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
                AssociatedObject.TextArea.SelectionChanged -= TextAreaOnSelectionChanged;
            }
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            if (textEditor?.Document != null)
            {
                BindingText = textEditor.Document.Text;
            }
        }

        private static void BindingTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AvalonEditBehaviour;
            var editor = behavior?.AssociatedObject;
            if (editor?.Document != null)
            {
                behavior.InDocumentChanging = true;
                var caretOffset = editor.CaretOffset;
                editor.Document.Text = e.NewValue.ToString();
                editor.CaretOffset = caretOffset;
                behavior.InDocumentChanging = false;
            }
        }

        private void TextAreaOnSelectionChanged(object sender, EventArgs e)
        {
            if (AssociatedObject != null)
            {
                BindingSelectedText = AssociatedObject.SelectedText;
            }
        }


        // TODO: Handle 2 way
        private static void BindingSelectedTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            /*var behavior = d as AvalonEditBehaviour;
            var editor = behavior?.AssociatedObject;
            if (editor != null && !behavior.InDocumentChanging)
            {
                behavior.InDocumentChanging = true;
                editor.SelectedText = e.NewValue.ToString();
                behavior.InDocumentChanging = true;
            }*/
        }
    }
}