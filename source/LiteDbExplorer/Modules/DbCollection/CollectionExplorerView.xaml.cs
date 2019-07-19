using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbCollection
{
    /// <summary>
    /// Interaction logic for CollectionExplorerView.xaml
    /// </summary>
    public partial class CollectionExplorerView : UserControl, ICollectionReferenceListView
    {
        public CollectionExplorerView()
        {
            InitializeComponent();

            SplitContainerSelectionController.Attach(splitContainer, splitOrientationSelector);

            DockSearch.IsVisibleChanged += (sender, args) =>
            {
                if (DockSearch.Visibility == Visibility.Visible)
                {
                    Dispatcher.Invoke(async () =>
                    {
                        await Task.Delay(100);
                        TextSearch.Focus();
                        TextSearch.SelectAll();
                    });
                }
            };

            CollectionListView.Loaded += CollectionListViewOnLoaded;
        }

        public Action CollectionLoadedAction { get; set; }

        private void CollectionListViewOnLoaded(object sender, RoutedEventArgs e)
        {
            if (CollectionLoadedAction != null)
            {
                Dispatcher.Invoke(CollectionLoadedAction);
            }
        }

        public void ScrollIntoItem(object item)
        {
            CollectionListView.ScrollIntoItem(item);
        }

        public void ScrollIntoSelectedItem()
        {
            CollectionListView.ScrollIntoSelectedItem();
        }

        public void UpdateView(CollectionReference collectionReference)
        {
            CollectionListView.UpdateGridColumns(collectionReference);
        }

        public void UpdateView(DocumentReference documentReference)
        {
            CollectionListView.UpdateGridColumns();
        }

        public void Find(string text, bool matchCase)
        {
            CollectionListView.Find(text, matchCase);
        }

        public void FindPrevious(string text, bool matchCase)
        {
            CollectionListView.FindPrevious(text, matchCase);
        }

        public void FocusListView()
        {
            CollectionListView.ListCollectionData.Focus();
        }

    }
}
