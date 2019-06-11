using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShellRightMenu))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class ShellRightMenuViewModel : PropertyChangedBase, IShellRightMenu
    {
        
    }
}