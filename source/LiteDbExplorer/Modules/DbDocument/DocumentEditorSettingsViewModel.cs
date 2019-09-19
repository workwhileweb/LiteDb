using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Modules.DbDocument
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DocumentEditorSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        private bool _ignorePropertyChange;

        public DocumentEditorSettingsViewModel()
        {
            _ignorePropertyChange = true;
            DocumentEditor_AllowEditId = Properties.Settings.Default.DocumentEditor_AllowEditId;
            _ignorePropertyChange = false;
        }

        public string SettingsPagePath => Properties.Resources.SettingsPageView;
        
        public string SettingsPageName => "_Documents";
        
        public int EditorDisplayOrder => 30;

        public string GroupDisplayName => "Options";

        public object AutoGenContext => this;

        [Category("Document Editor")]
        [DisplayName("Allow edit document __id (not recommended)")]
        [Description("Editing document _id can cause issues. Use this option at your own risk.")]
        public bool DocumentEditor_AllowEditId { get; set; }

        public override void NotifyOfPropertyChange(string propertyName = null)
        {
            base.NotifyOfPropertyChange(propertyName);

            if (_ignorePropertyChange)
            {
                return;
            }

            if (propertyName != null && propertyName.Equals(nameof(DocumentEditor_AllowEditId)) && DocumentEditor_AllowEditId)
            {
                _ignorePropertyChange = true;
                DocumentEditor_AllowEditId = MessageBox.Show(
                                                 "Editing document _id can cause issues.\n\nDo you want to enable this option?", 
                                                 AppConstants.Application.DisplayName,
                                                 MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
                _ignorePropertyChange = false;
            }
        }

        public void ApplyChanges()
        {
            Properties.Settings.Default.DocumentEditor_AllowEditId = DocumentEditor_AllowEditId;

            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
    }
}