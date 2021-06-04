using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DynamicData;

namespace LiteDbExplorer.Controls
{
    public class ListViewFindController
    {
        private readonly ListView _listView;
        private readonly Func<object, string> _getStringData;

        public ListViewFindController(ListView listView, Func<object, string> getStringData)
        {
            _listView = listView;
            _getStringData = getStringData;
        }

        private IList<object> SelectedItems => _listView.SelectedItems as IList<object>;

        private IEnumerable<object> ItemsSource => _listView.ItemsSource.Cast<object>();

        public bool EnableHighlight { get; set; } = false;

        public void Find(string text, bool matchCase)
        {
            if (string.IsNullOrEmpty(text) || ItemsSource == null)
            {
                return;
            }

            var skipIndex = -1;
            if (SelectedItems.Count > 0)
            {
                skipIndex = ItemsSource.IndexOf(SelectedItems.Last());
            }

            HighlightItems(text, matchCase);

            foreach (var item in ItemsSource.Skip(skipIndex + 1))
            {
                if (ItemMatchesSearch(text, item, matchCase))
                {
                    _listView.SelectedItem = item;
                    ScrollIntoSelectedItem();
                    return;
                }
            }
        }

        public void FindPrevious(string text, bool matchCase)
        {
            if (string.IsNullOrEmpty(text) || ItemsSource == null)
            {
                return;
            }

            var skipIndex = 0;
            if (SelectedItems.Count > 0)
            {
                skipIndex = ItemsSource.Count() - ItemsSource.IndexOf(SelectedItems.Last()) - 1;
            }

            HighlightItems(text, matchCase);

            foreach (var item in ItemsSource.Reverse().Skip(skipIndex + 1))
            {
                if (ItemMatchesSearch(text, item, matchCase))
                {
                    _listView.SelectedItem = item;
                    ScrollIntoSelectedItem();
                    return;
                }
            }
        }

        private void ScrollIntoSelectedItem()
        {
            _listView.ScrollIntoView(_listView.SelectedItem);
        }

        private bool ItemMatchesSearch(string matchTerm, object document, bool matchCase)
        {
            var stringData = _getStringData(document);
            if (string.IsNullOrEmpty(stringData))
            {
                return false;
            }

            if (matchCase)
            {
                return stringData.IndexOf(matchTerm, 0, StringComparison.InvariantCulture) != -1;
            }

            return stringData.IndexOf(matchTerm, 0, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private void HighlightItems(string text, bool matchCase)
        {
            if (!EnableHighlight || ItemsSource == null)
            {
                return;
            }

            foreach (var item in ItemsSource)
            {
                if (_listView.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem listViewItem)
                {
                    HighlightText(listViewItem, text, matchCase);
                }
            }
        }

        private void HighlightText(DependencyObject dependencyObject, string term, bool matchCase)
        {
            if (dependencyObject != null)
            {
                if (dependencyObject is TextBlock tb)
                {
                    var regex = new Regex("(" + term + ")", matchCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                    if (term.Length == 0)
                    {
                        var str = tb.Text;
                        tb.Inlines.Clear();
                        tb.Inlines.Add(str);
                        return;
                    }

                    var substrings = regex.Split(tb.Text);
                    tb.Inlines.Clear();
                    foreach (var item in substrings)
                    {
                        if (regex.Match(item).Success)
                        {
                            var runx = new Run(item)
                            {
                                Background = SystemColors.HighlightBrush
                            };
                            tb.Inlines.Add(runx);
                        }
                        else
                        {
                            tb.Inlines.Add(item);
                        }
                    }
                    return;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    HighlightText(VisualTreeHelper.GetChild(dependencyObject, i), term, matchCase);
                }
            }
        }
    }
}