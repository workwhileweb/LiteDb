﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using LiteDbExplorer.Presentation.Converters;
using LiteDbExplorer.Windows;
using LiteDB;
using Xceed.Wpf.Toolkit;

namespace LiteDbExplorer.Controls
{
    public enum OpenEditorMode
    {
        Inline,
        Window
    }

    public class BsonValueEditorContext
    {
        public BsonValueEditorContext(OpenEditorMode openMode, string bindingPath, BsonValue bindingValue, object bindingSource, bool readOnly, string keyName)
        {
            OpenMode = openMode;
            BindingPath = bindingPath;
            BindingValue = bindingValue;
            BindingSource = bindingSource;
            ReadOnly = readOnly;
            KeyName = keyName;
        }

        public OpenEditorMode OpenMode { get; private set; }
        public string BindingPath { get; private set; }
        public BsonValue BindingValue { get; private set; }
        public object BindingSource { get; private set; }
        public bool ReadOnly { get; private set; }
        public string KeyName { get; private set; }
    }

    public class BsonValueEditor
    {
        private static double DefaultWindowHeight =>
            Math.Min(Math.Max(636, SystemParameters.VirtualScreenHeight / 1.61), SystemParameters.VirtualScreenHeight);

        public static FrameworkElement GetBsonValueEditor(BsonValueEditorContext editorContext)
        {
            var binding = new Binding
            {
                Path = new PropertyPath(editorContext.BindingPath),
                Source = editorContext.BindingSource,
                Mode = BindingMode.TwoWay,
                Converter = new BsonValueToNetValueConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            if (editorContext.BindingValue.IsArray)
            {
                var arrayValue = editorContext.BindingValue as BsonArray;

                if (editorContext.OpenMode == OpenEditorMode.Window)
                {
                    var button = new Button
                    {
                        Content = $"[Array] {arrayValue?.Count} {editorContext.KeyName}",
                        Style = StyleKit.MaterialDesignEntryButtonStyle
                    };

                    button.Click += (s, a) =>
                    {
                        arrayValue = editorContext.BindingValue as BsonArray;

                        var windowController = new WindowController {Title = "Array Editor"};
                        var control = new ArrayEntryControl(arrayValue, editorContext.ReadOnly, windowController);
                        var window = new DialogWindow(control, windowController)
                        {
                            Owner = Application.Current.MainWindow,
                            Height = DefaultWindowHeight,
                            MinWidth = 400,
                            MinHeight = 400,
                        };

                        if (window.ShowDialog() == true)
                        {
                            arrayValue?.Clear();
                            arrayValue?.AddRange(control.EditedItems);
                            
                            button.Content = $"[Array] {arrayValue?.Count} {editorContext.KeyName}";
                        }
                    };

                    return button;
                }

                var contentView = new ContentExpander
                {
                    LoadButton =
                    {
                        Content = $"[Array] {arrayValue?.Count} {editorContext.KeyName}"
                    }
                };

                contentView.LoadButton.Click += (s, a) =>
                {
                    if (contentView.ContentLoaded) return;

                    arrayValue = editorContext.BindingValue as BsonArray;
                    var control = new ArrayEntryControl(arrayValue, editorContext.ReadOnly);
                    control.CloseRequested += (sender, args) => { contentView.Content = null; };
                    contentView.Content = control;
                };

                return contentView;
            }

            if (editorContext.BindingValue.IsDocument)
            {
                var expandLabel = "[Document]";
                if (editorContext.OpenMode == OpenEditorMode.Window)
                {
                    var button = new Button
                    {
                        Content = expandLabel,
                        Style = StyleKit.MaterialDesignEntryButtonStyle
                    };

                    button.Click += (s, a) =>
                    {
                        var windowController = new WindowController {Title = "Document Editor"};
                        var bsonDocument = editorContext.BindingValue as BsonDocument;
                        var control = new DocumentEntryControl(bsonDocument, editorContext.ReadOnly, windowController);
                        var window = new DialogWindow(control, windowController)
                        {
                            Owner = Application.Current.MainWindow,
                            Height = DefaultWindowHeight,
                            MinWidth = 400,
                            MinHeight = 400,
                        };

                        window.ShowDialog();
                    };

                    return button;
                }

                var contentView = new ContentExpander
                {
                    LoadButton =
                    {
                        Content = expandLabel
                    }
                };

                contentView.LoadButton.Click += (s, a) =>
                {
                    if (contentView.ContentLoaded)
                    {
                        return;
                    }

                    var bsonDocument = editorContext.BindingValue as BsonDocument;
                    var control = new DocumentEntryControl(bsonDocument, editorContext.ReadOnly);
                    control.CloseRequested += (sender, args) => { contentView.Content = null; };

                    contentView.Content = control;
                };

                return contentView;
            }

            if (editorContext.BindingValue.IsBoolean)
            {
                var check = new CheckBox
                {
                    IsEnabled = !editorContext.ReadOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                check.SetBinding(ToggleButton.IsCheckedProperty, binding);
                return check;
            }

            if (editorContext.BindingValue.IsDateTime)
            {
                var datePicker = new DateTimePicker
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = editorContext.ReadOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0),
                };

                datePicker.SetBinding(DateTimePicker.ValueProperty, binding);

                return datePicker;
            }

            if (editorContext.BindingValue.IsDouble)
            {
                var numberEditor = new DoubleUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = editorContext.ReadOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(DoubleUpDown.ValueProperty, binding);
                return numberEditor;
            }

            if (editorContext.BindingValue.IsDecimal)
            {
                var numberEditor = new DecimalUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = editorContext.ReadOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(DecimalUpDown.ValueProperty, binding);
                return numberEditor;
            }

            if (editorContext.BindingValue.IsInt32)
            {
                var numberEditor = new IntegerUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = editorContext.ReadOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(IntegerUpDown.ValueProperty, binding);

                return numberEditor;
            }

            if (editorContext.BindingValue.IsInt64)
            {
                var numberEditor = new LongUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = editorContext.ReadOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(LongUpDown.ValueProperty, binding);
                return numberEditor;
            }

            if (editorContext.BindingValue.IsGuid || editorContext.BindingValue.Type == BsonType.Guid)
            {
                var guidEditor = new MaskedTextBox
                {
                    IsReadOnly = editorContext.ReadOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Mask = @"AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA",
                    ValueDataType = typeof(Guid)
                };

                guidEditor.SetBinding(MaskedTextBox.ValueProperty, binding);
                return guidEditor;
            }

            if (editorContext.BindingValue.IsString)
            {
                var stringEditor = new TextBox
                {
                    IsReadOnly = editorContext.ReadOnly,
                    AcceptsReturn = true,
                    VerticalAlignment = VerticalAlignment.Center,
                    MaxHeight = 200,
                    MaxLength = 1024,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                };

                stringEditor.SetBinding(TextBox.TextProperty, binding);
                return stringEditor;
            }

            if (editorContext.BindingValue.IsBinary)
            {
                var text = new TextBlock
                {
                    Text = "[Binary Data]",
                    VerticalAlignment = VerticalAlignment.Center,
                };

                return text;
            }

            if (editorContext.BindingValue.IsObjectId)
            {
                var text = new TextBox
                {
                    Text = editorContext.BindingValue.AsString,
                    IsReadOnly = true,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                return text;
            }

            var defaultEditor = new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                MaxHeight = 200,
                MaxLength = 1024,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            defaultEditor.SetBinding(TextBox.TextProperty, binding);
            return defaultEditor;
        }
    }
}