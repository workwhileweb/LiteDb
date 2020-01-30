using System.Windows;
using System.Windows.Controls;
using Dragablz;
using LiteDbExplorer.Wpf.Framework.Shell;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;

namespace LiteDbExplorer.Modules.Main
{
    /// <summary>
    /// Interaction logic for DocumentSetView.xaml
    /// </summary>
    public partial class DocumentSetView : UserControl, ITabablzControlHolder
    {
        public DocumentSetView()
        {
            InitializeComponent();


            if (TabablzControl != null)
            {
                TabablzControl.SelectionChanged += (sender, args) =>
                {
                    var screen = args.AddedItems.OfType<Screen>().FirstOrDefault();
                    if (screen?.GetView() is UserControl userControl)
                    {
                        FocusManager.SetFocusedElement(userControl, null);
                        Keyboard.ClearFocus();
                        // CommandManager.InvalidateRequerySuggested();
                    }
                };
            }
        }
        
        public TabablzControl TabsControl => TabablzControl;
    }

    public class DocumentMenuItemContainerTemplateSelector : ItemContainerTemplateSelector
    {
        public DataTemplate DocumentsMenuHeaderTemplate { get; set; }

        public DataTemplate DefaultMenuHeaderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
        {
            if (item is IDocument)
            {
                return DocumentsMenuHeaderTemplate;
            }

            if (DefaultMenuHeaderTemplate == null)
            {
                return base.SelectTemplate(item, parentItemsControl);
            }

            return DefaultMenuHeaderTemplate;
        }
    }
}
