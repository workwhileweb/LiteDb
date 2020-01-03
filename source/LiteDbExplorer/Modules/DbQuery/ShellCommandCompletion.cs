using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using DynamicData;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using LiteDbExplorer.Controls.Editor.Completion;
using LiteDbExplorer.Core;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.DbQuery
{
    public class ShellCommandCompletion 
    {
        private readonly TextEditor _textEditor;

        private CompletionWindow _completionWindow;
        private readonly HashSet<string> _collections;
        private readonly HashSet<string> _collectionsCommandsKeys;
        private readonly HashSet<string> _queryFilterKeys;
        private readonly HashSet<string> _queryOperatorKeys;
        private DatabaseReference _databaseReference;

        public ShellCommandCompletion(TextEditor textEditor)
        {
            _textEditor = textEditor;

            _textEditor.KeyDown += OnTextEditorKeyDown;
            _textEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            _textEditor.TextArea.TextEntered += OnTextAreaTextEntered;
            _textEditor.Unloaded += OnTextEditorUnloaded;

            _collectionsCommandsKeys = new HashSet<string>
            {
                "find",
                "select",
                "indexes",
                "count"
            };

            _queryFilterKeys = new HashSet<string>
            {
                "unique",
                "using",
                "skip",
                "limit",
                "includes",
                "where",
                "like",
                "contains",
                "in",
                "between"
            };

            _queryOperatorKeys = new HashSet<string>
            {
                "and",
                "or"
            };

            _collections = new HashSet<string>();
        }

        public TextArea TextArea => _textEditor.TextArea;

        public void UpdateCodeCompletion(DatabaseReference databaseReference)
        {
            _databaseReference = databaseReference;

            _collections.Clear();

            if (databaseReference != null)
            {
                foreach (var referenceLookup in databaseReference.CollectionsLookup)
                {
                    _collections.Add(referenceLookup.Name);
                }
            }
        }

        private void OnTextEditorUnloaded(object sender, RoutedEventArgs e)
        {
            _textEditor.KeyDown -= OnTextEditorKeyDown;
            _textEditor.TextArea.TextEntering -= OnTextAreaTextEntering;
            _textEditor.TextArea.TextEntered -= OnTextAreaTextEntered;
            _textEditor.Unloaded -= OnTextEditorUnloaded;
        }

        private void OnTextEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                                   && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                ShowCompletion(null);
                e.Handled = true;
            }
        }

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow?.CompletionList.RequestInsertion(e);
                }

                if (char.IsDigit(e.Text[0]) && _completionWindow?.CompletionList.SelectedItem == null)
                {
                    _completionWindow?.Close();
                }
            }
        }

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            ShowCompletion(e.Text);
        }

        private void ShowCompletion(string text)
        {
            if (_completionWindow != null) return;

            var lineText = GetLineText();
            var dotParts = lineText.Split(new []{'.'}, StringSplitOptions.None);
            var words = lineText.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            // var word = GetWord(TextArea.Caret.Offset - 1);

            var inCollectionNames = lineText.Equals("db.", StringComparison.OrdinalIgnoreCase);

            var inCollectionCommands = dotParts.Length == 3 && string.IsNullOrEmpty(dotParts.Last()) && _collections.Any(col => col.Equals(dotParts[1], StringComparison.OrdinalIgnoreCase));
            if (inCollectionCommands)
            {
                inCollectionNames = false;
            }
            
            var inCollectionFilters = dotParts.Length >= 3 && !string.IsNullOrWhiteSpace(dotParts[2]) && 
                                      TextArea.Document.GetCharAt(TextArea.Caret.Offset -1) == ' ';
            if (inCollectionFilters)
            {
                inCollectionCommands = false;
            }

            var codeCompletionData = new List<ICompletionData>();
            if (string.IsNullOrWhiteSpace(lineText) || lineText.Equals("d") || lineText.Equals("db"))
            {
                var emptyData= new DefaultCompletionData("db", "db", 0).SetIcon(PackIconKind.Database);
                codeCompletionData.Add(emptyData);
            }

            // Complete collection names
            if (inCollectionNames)
            {
                var collectionNamesData= _collections.Select(key => 
                    new DefaultCompletionData($"db.{key}", key, inCollectionNames ? 0 : 1).SetIcon(PackIconKind.Table)
                );
                codeCompletionData.AddRange(collectionNamesData);
            }
            
            // Complete collection commands
            if (inCollectionCommands && dotParts.Length > 0)
            {
                var collectionName = dotParts[1];

                var collectionsCommandsData= _collectionsCommandsKeys.Select(key => 
                    new DefaultCompletionData($"db.{collectionName}.{key}", key, inCollectionCommands ? 0: 2).SetIcon(PackIconKind.AppleKeyboardCommand)
                );
                codeCompletionData.AddRange(collectionsCommandsData);
            }
            
            // Complete collection filters
            if (inCollectionFilters)
            {
                var hasFilterExpression = words.Any() && _queryFilterKeys.Any(filterKey => words.Last().Equals(filterKey, StringComparison.OrdinalIgnoreCase));
                if (hasFilterExpression)
                {
                    var logicalOperators = _queryOperatorKeys.Select(key =>
                
                        new DefaultCompletionData(lineText + key, key, 0).SetIcon(PackIconKind.CodeTags)
                    );
                    codeCompletionData.AddRange(logicalOperators);
                }
                
                var collectionsFiltersData= _queryFilterKeys.Select(key => 
                    new DefaultCompletionData(lineText + key, key, 2).SetIcon(PackIconKind.FilterOutline)
                );
                codeCompletionData.AddRange(collectionsFiltersData);

                var collectionName = dotParts[1];
                var collectionFields = _databaseReference?[collectionName]?.GetDistinctKeys();
                if (collectionFields != null)
                {
                    var fieldsData= collectionFields.Select(key => 
                        new DefaultCompletionData(lineText + key, key, 1).SetIcon(PackIconKind.LabelOutline)
                    );
                    codeCompletionData.AddRange(fieldsData);
                }
            }
            
            if (codeCompletionData.Any())
            {
                ShowCodeCompleteWindow(codeCompletionData);
            }
        }

        private void ShowCodeCompleteWindow(IList<ICompletionData> completionDataSet)
        {
            _completionWindow = new CompletionWindow(_textEditor.TextArea)
            {
                CloseAutomatically = true,
                CloseWhenCaretAtBeginning = true
            };
            
            var data = _completionWindow.CompletionList.CompletionData;
            
            data.AddRange(completionDataSet);

            _completionWindow.Show();
            _completionWindow.Closed += (sender, args) => _completionWindow = null;
        }

        private (int wordStart, string text) GetWord(int position)
        {
            var wordStart = TextUtilities.GetNextCaretPosition(TextArea.Document, position, LogicalDirection.Backward, CaretPositioningMode.WordStart);
            var text = TextArea.Document.GetText(wordStart, position - wordStart);
            return (wordStart, text);
        }

        private string GetLineText()
        {
            try
            {
                var textArea = _textEditor.TextArea;
                var line = _textEditor.Document.GetLineByNumber(textArea.Caret.Position.Line);
                var text = _textEditor.Document.GetText(line);
                
                return text ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static bool IsAllowedLanguageLetter(char character)
        {
            return TextUtilities.GetCharacterClass(character) == CharacterClass.IdentifierPart;
        }
    }
}