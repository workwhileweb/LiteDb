using System;
using System.Windows.Controls;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbDocument
{
    /// <summary>
    /// Interaction logic for DocumentPreviewView.xaml
    /// </summary>
    public partial class DocumentPreviewView : UserControl, IDocumentDetailView
    {
        public DocumentPreviewView()
        {
            InitializeComponent();
            
            SplitContainerSelectionController.Attach(splitContainer, splitOrientationSelector);
        }

        public void UpdateView(DocumentReference documentReference)
        {
            documentTreeView.UpdateDocument();
            documentJsonView.UpdateDocument();
        }
    }
}