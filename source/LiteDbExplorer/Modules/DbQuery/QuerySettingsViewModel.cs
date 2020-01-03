using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Controls;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Modules.DbQuery
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class QuerySettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public QuerySettingsViewModel()
        {
            QueryEditor_EnableShellCommandAutocomplete = Properties.Settings.Default.QueryEditor_EnableShellCommandAutocomplete;
        }

        public string SettingsPagePath => Properties.Resources.SettingsPageView;
        
        public string SettingsPageName => "_Query";
        
        public int EditorDisplayOrder => 35;

        public string GroupDisplayName => "Options";

        public object AutoGenContext => this;

        [Category("Query Editor (experimental)")]
        [DisplayName("Enable completion (shell command)")]
        public bool QueryEditor_EnableShellCommandAutocomplete { get; set; }

        public void ApplyChanges()
        {
            Properties.Settings.Default.QueryEditor_EnableShellCommandAutocomplete = QueryEditor_EnableShellCommandAutocomplete;

            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }   
    }
}