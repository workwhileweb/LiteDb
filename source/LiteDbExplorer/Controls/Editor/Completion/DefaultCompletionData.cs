using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Serilog;

namespace LiteDbExplorer.Controls.Editor.Completion
{
    public class DefaultCompletionData : ICompletionData
    {
        private object _content;

        public DefaultCompletionData(string text, string value, double priority = 0, object description = null)
        {
            Text = text;
            Value = value;
            Description = description;
            Priority = priority;
        }

        public string Text { get; set; }

        public string Value { get; set; }

        public object Content
        {
            get => _content ?? Value;
            set => _content = value;
        }

        public object Description { get; set; }
        public double Priority { get; set; }

        [JsonIgnore]
        public ImageSource Image { get; set; }

        [JsonIgnore]
        public PackIconKind? PackIconKind { get; set; }

        public DefaultCompletionData SetPriority(double priority = 0)
        {
            Priority = priority;
            return this;
        }

        public DefaultCompletionData SetIcon(ImageSource imageSource)
        {
            PackIconKind = null;
            Image = imageSource;
            return this;
        }

        public DefaultCompletionData SetIcon(PackIconKind packIconKind)
        {
            PackIconKind = packIconKind;
            Image = null;
            return this;
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            var lead = textArea.Document.GetText(line);

            var text = Text;

            if (completionSegment.Offset == completionSegment.EndOffset)
            {
                text = text.Substring(completionSegment.EndOffset);  //Value;
            }

            Log.Debug("[Complete] segment: {completionSegment} lead: {lead}, rawText: {rawText}, currentText: {currentText}", completionSegment, lead, Text, text);

            textArea.Document.Replace(completionSegment, text);

        }

        public static int Compare(ICompletionData a, ICompletionData b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            return string.Compare(a.Text, b.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}