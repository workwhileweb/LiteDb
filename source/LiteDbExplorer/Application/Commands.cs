using System.Windows.Input;

namespace LiteDbExplorer
{
    public static class Commands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand
        (
            "Exit",
            nameof(Exit),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );

        public static readonly RoutedUICommand Add = new RoutedUICommand
        (
            "Add...",
            "Add",
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.Insert)
            }
        );

        public static readonly RoutedUICommand Edit = new RoutedUICommand
        (
            "Edit...",
            nameof(Edit),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F2)
            }
        );

        public static readonly RoutedUICommand Remove = new RoutedUICommand
        (
            "Remove",
            nameof(Remove),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.Delete)
            }
        );

        public static readonly RoutedUICommand DropCollection = new RoutedUICommand
        (
            "Drop Collection",
            nameof(DropCollection),
            typeof(Commands)
        );

        public static readonly RoutedUICommand AddCollection = new RoutedUICommand
        (
            "Add Collection...",
            nameof(AddCollection),
            typeof(Commands)
        );

        public static readonly RoutedUICommand RenameCollection = new RoutedUICommand
        (
            "Rename Collection...",
            nameof(RenameCollection),
            typeof(Commands)
        );

        public static readonly RoutedUICommand Export = new RoutedUICommand
        (
            "Export...",
            nameof(Export),
            typeof(Commands)
        );

        public static readonly RoutedUICommand ExportCollection = new RoutedUICommand
        (
            "Export Collection...",
            nameof(ExportCollection),
            typeof(Commands)
        );

        public static readonly RoutedUICommand EditDbProperties = new RoutedUICommand
        (
            "Database Properties...",
            "EditDb",
            typeof(Commands)
        );

        public static readonly RoutedUICommand AddFile = new RoutedUICommand
        (
            "Add File...",
            nameof(AddFile),
            typeof(Commands)
        );

        public static readonly RoutedUICommand SaveCopyAs = new RoutedUICommand
        (
            "Save Copy As...",
            nameof(SaveCopyAs),
            typeof(Commands)
        );

        /*public static readonly RoutedUICommand Find = new RoutedUICommand
        (
            "Find...",
            nameof(Find),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F, ModifierKeys.Control)
            }
        );*/

        public static readonly RoutedUICommand FindNext = new RoutedUICommand
        (
            "Find Next",
            nameof(FindNext),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F3)
            }
        );

        public static readonly RoutedUICommand FindPrevious = new RoutedUICommand
        (
            "Find Previous",
            nameof(FindPrevious),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F3, ModifierKeys.Shift)
            }
        );
        
        public static readonly RoutedUICommand RefreshCollection = new RoutedUICommand
        (
            "Refresh Collection",
            nameof(RefreshCollection),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F5)
            }
        );

        public static readonly RoutedUICommand RefreshDatabase = new RoutedUICommand
        (
            "Refresh Database",
            nameof(RefreshDatabase),
            typeof(Commands)
        );

        public static readonly RoutedUICommand RevealInExplorer = new RoutedUICommand
        (
            "Reveal in Explorer",
            nameof(RevealInExplorer),
            typeof(Commands)
        );

        public static readonly RoutedCommand FileDropped = new RoutedCommand
        (
            "File Dropped",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Run = new RoutedUICommand
        (
            "Run",
            nameof(Run),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F5)
            }
        );

        public static readonly RoutedUICommand RunSelected = new RoutedUICommand
        (
            "Run Selected",
            nameof(Run),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.F5, ModifierKeys.Shift)
            }
        );

        public static readonly RoutedUICommand Import = new RoutedUICommand
        (
            "Import...",
            nameof(Import),
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.I, ModifierKeys.Control)
            }
        );
    }
}
