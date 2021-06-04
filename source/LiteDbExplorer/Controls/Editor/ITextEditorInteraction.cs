using System.Linq;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Controls.Editor
{
    public interface ITextEditorInteraction
    {
        void SelectEnd(bool focus = true);
        void SelectStart(bool focus = true);
        void SelectAll(bool focus = true);
        void SetDocumentText(string content);
        void InsetDocumentText(string content, DocumentInsetMode documentInsetMode);
        void InsetDocumentText(string content);
    }

    public class TextEditorInteraction : ITextEditorInteraction
    {
        private readonly TextEditor _textEditor;

        public TextEditorInteraction(TextEditor textEditor)
        {
            _textEditor = textEditor;
        }

        public virtual void SelectEnd(bool focus = true)
        {
            _textEditor.Select(_textEditor.Text.Length, 0);

            if (focus)
            {
                _textEditor.Focus();
            }
        }

        public virtual void SelectStart(bool focus = true)
        {
            _textEditor.Select(0, 0);

            if (focus)
            {
                _textEditor.Focus();
            }
        }

        public virtual void SelectAll(bool focus = true)
        {
            _textEditor.Select(0, _textEditor.Text.Length);

            if (focus)
            {
                _textEditor.Focus();
            }
        }

        public virtual void SetDocumentText(string content)
        {
            _textEditor.Document.Text = content;
        }

        public virtual void InsetDocumentText(string content, DocumentInsetMode documentInsetMode)
        {
            var originalContentLength = content.Length;

            switch (documentInsetMode)
            {
                case DocumentInsetMode.Start:
                    if (_textEditor.Document.GetLineByNumber(1).Length > 0)
                    {
                        content += "\n";
                    }
                    _textEditor.Document.Insert(0, content);
                    _textEditor.Select(0, originalContentLength);
                    break;
                case DocumentInsetMode.End:
                    if (_textEditor.Document.GetLineByNumber(_textEditor.Document.LineCount).Length > 0)
                    {
                        content = "\n" + content;
                    }
                    _textEditor.Document.Insert(_textEditor.Text.Length, content);
                    _textEditor.Select(_textEditor.Text.Length - originalContentLength, originalContentLength);
                    break;
                default:
                    _textEditor.Document.Text = content;
                    _textEditor.Select(0, _textEditor.Text.Length);
                    break;
            }

            _textEditor.Focus();
        }

        public virtual void InsetDocumentText(string content)
        {
            var command = new RelayCommand<TextInsertAction>(action =>
            {
                InsetDocumentText(action.Text, action.Mode);
            });

            var insertContextMenu = new ContextMenu
            {
                MinWidth = 160,
                Items =
                {
                    new MenuItem
                    {
                        Header = "Replace All",
                        Command = command,
                        CommandParameter = new TextInsertAction(DocumentInsetMode.Replace, content)
                    },
                    new MenuItem
                    {
                        Header = "Insert in Start",
                        Command = command,
                        CommandParameter = new TextInsertAction(DocumentInsetMode.Start, content)
                    },
                    new MenuItem
                    {
                        Header = "Insert in End",
                        Command = command,
                        CommandParameter = new TextInsertAction(DocumentInsetMode.End, content)
                    }
                },
                IsOpen = true
            };

            insertContextMenu.Focus();
            insertContextMenu.Items.OfType<MenuItem>().FirstOrDefault()?.Focus();
        }

        public class TextInsertAction
        {
            public TextInsertAction(DocumentInsetMode mode, string text)
            {
                Mode = mode;
                Text = text;
            }

            public DocumentInsetMode Mode { get; set; }
            public string Text { get; set; }
        }
    }

    public enum DocumentInsetMode
    {
        Replace,
        Start,
        End
    }
}