using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LiteDbExplorer.Wpf.Behaviors
{
    // See: https://github.com/kthsu/HighlightableTextBlock
    public class HighlightTextBlock
    {
        #region Bold

        public static bool GetBold(DependencyObject obj)
        {
            return (bool)obj.GetValue(BoldProperty);
        }

        public static void SetBold(DependencyObject obj, bool value)
        {
            obj.SetValue(BoldProperty, value);
        }

        // Using a DependencyProperty as the backing store for Bold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoldProperty =
            DependencyProperty.RegisterAttached("Bold", typeof(bool), typeof(HighlightTextBlock), new PropertyMetadata(false, Refresh));

        #endregion

        #region Italic

        public static bool GetItalic(DependencyObject obj)
        {
            return (bool)obj.GetValue(ItalicProperty);
        }

        public static void SetItalic(DependencyObject obj, bool value)
        {
            obj.SetValue(ItalicProperty, value);
        }

        // Using a DependencyProperty as the backing store for Italic.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItalicProperty =
            DependencyProperty.RegisterAttached("Italic", typeof(bool), typeof(HighlightTextBlock), new PropertyMetadata(false, Refresh));

        #endregion

        #region Underline

        public static bool GetUnderline(DependencyObject obj)
        {
            return (bool)obj.GetValue(UnderlineProperty);
        }

        public static void SetUnderline(DependencyObject obj, bool value)
        {
            obj.SetValue(UnderlineProperty, value);
        }

        // Using a DependencyProperty as the backing store for Underline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnderlineProperty =
            DependencyProperty.RegisterAttached("Underline", typeof(bool), typeof(HighlightTextBlock), new PropertyMetadata(false, Refresh));

        #endregion

        #region HighlightTextBrush

        public static Brush GetHighlightTextBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightTextBrushProperty);
        }

        public static void SetHighlightTextBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightTextBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for HighlightTextBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightTextBrushProperty =
            DependencyProperty.RegisterAttached("HighlightTextBrush", typeof(Brush), typeof(HighlightTextBlock), new PropertyMetadata(SystemColors.HighlightTextBrush, Refresh));

        #endregion

        #region HighlightBrush

        public static Brush GetHighlightBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightBrushProperty);
        }

        public static void SetHighlightBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for HighlightBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.RegisterAttached("HighlightBrush", typeof(Brush), typeof(HighlightTextBlock), new PropertyMetadata(SystemColors.HighlightBrush, Refresh));

        #endregion

        #region HighlightText

        public static string GetHightlightText(DependencyObject obj)
        {
            return (string)obj.GetValue(HightlightTextProperty);
        }

        public static void SetHightlightText(DependencyObject obj, string value)
        {
            obj.SetValue(HightlightTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HightlightText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HightlightTextProperty =
            DependencyProperty.RegisterAttached("HightlightText", typeof(string), typeof(HighlightTextBlock), new PropertyMetadata(string.Empty, Refresh));

        #endregion

        #region InternalText

        protected static string GetInternalText(DependencyObject obj)
        {
            return (string)obj.GetValue(InternalTextProperty);
        }

        protected static void SetInternalText(DependencyObject obj, string value)
        {
            obj.SetValue(InternalTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalText.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty InternalTextProperty =
            DependencyProperty.RegisterAttached("InternalText", typeof(string),
                typeof(HighlightTextBlock), new PropertyMetadata(string.Empty, OnInternalTextChanged));

        private static void OnInternalTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.Text = e.NewValue as string;
                Highlight(textBlock);
            }
        }

        #endregion

        #region  IsBusy 

        private static bool GetIsBusy(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsBusyProperty);
        }

        private static void SetIsBusy(DependencyObject obj, bool value)
        {
            obj.SetValue(IsBusyProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsBusy.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.RegisterAttached("IsBusy", typeof(bool), typeof(HighlightTextBlock), new PropertyMetadata(false));

        #endregion

        #region Methods

        private static void Refresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Highlight(d as TextBlock);
        }

        private static void Highlight(TextBlock textBlock)
        {
            if (textBlock == null) return;

            string text = textBlock.Text;

            if (textBlock.GetBindingExpression(HighlightTextBlock.InternalTextProperty) == null)
            {
                var textBinding = textBlock.GetBindingExpression(TextBlock.TextProperty);

                if (textBinding != null)
                {
                    textBlock.SetBinding(HighlightTextBlock.InternalTextProperty, textBinding.ParentBindingBase);

                    var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

                    propertyDescriptor.RemoveValueChanged(textBlock, OnTextChanged);
                }
                else
                {
                    var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

                    propertyDescriptor.AddValueChanged(textBlock, OnTextChanged);

                    textBlock.Unloaded -= Textblock_Unloaded;
                    textBlock.Unloaded += Textblock_Unloaded;
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                SetIsBusy(textBlock, true);

                var toHighlight = GetHightlightText(textBlock);

                if (!string.IsNullOrEmpty(toHighlight))
                {
                    var matches = Regex.Split(text, $"({Regex.Escape(toHighlight)})", RegexOptions.IgnoreCase);

                    textBlock.Inlines.Clear();

                    var highlightBrush = GetHighlightBrush(textBlock);
                    var highlightTextBrush = GetHighlightTextBrush(textBlock);

                    foreach (var subString in matches)
                    {
                        if (string.Compare(subString, toHighlight, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            var formattedText = new Run(subString)
                            {
                                Background = highlightBrush,
                                Foreground = highlightTextBrush,
                            };

                            if (GetBold(textBlock))
                            {
                                formattedText.FontWeight = FontWeights.Bold;
                            }

                            if (GetItalic(textBlock))
                            {
                                formattedText.FontStyle = FontStyles.Italic;
                            }

                            if (GetUnderline(textBlock))
                            {
                                formattedText.TextDecorations.Add(TextDecorations.Underline);
                            }

                            textBlock.Inlines.Add(formattedText);
                        }
                        else
                        {
                            textBlock.Inlines.Add(subString);
                        }
                    }
                }
                else
                {
                    textBlock.Inlines.Clear();
                    textBlock.SetCurrentValue(TextBlock.TextProperty, text);
                }

                SetIsBusy(textBlock, false);
            }
        }

        private static void Textblock_Unloaded(object sender, RoutedEventArgs e)
        {
            var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

            propertyDescriptor.RemoveValueChanged(sender as TextBlock, OnTextChanged);
        }

        private static void OnTextChanged(object sender, EventArgs e)
        {
            if (sender is TextBlock textBlock && !GetIsBusy(textBlock))
            {
                Highlight(textBlock);
            }
        }

        #endregion
    }
}