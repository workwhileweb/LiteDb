using System;
using System.Windows.Input;
using System.Windows.Markup;

namespace LiteDbExplorer.Wpf.Behaviors
{
    public class KeyGestureExtension : MarkupExtension
    {
        public Key Key { get; set; }
        public ModifierKeys ModifierKeys { get; set; }
        public string Caption { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new KeyGesture(Key, ModifierKeys, Caption);
        }
    }
}