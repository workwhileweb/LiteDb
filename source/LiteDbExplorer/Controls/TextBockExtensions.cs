using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LiteDbExplorer.Controls
{
    public static class TextBockExtensions
    {
        public static TextBlock SetDefinitionList(this TextBlock textBlock, IDictionary<string, string> dictionary, Orientation orientation = Orientation.Horizontal)
        {
            foreach (var info in dictionary)
            {
                textBlock.Inlines.Add(new Run(info.Key + ": "){ FontWeight = FontWeights.Bold});
                if (orientation == Orientation.Vertical)
                {
                    textBlock.Inlines.Add(new LineBreak());
                }
                textBlock.Inlines.Add(info.Value);
                textBlock.Inlines.Add(new LineBreak());
            }
            textBlock.Inlines.Remove(textBlock.Inlines.LastInline);

            return textBlock;
        }
    }
}