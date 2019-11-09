using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Core;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Modules.Database
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DatabaseSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public DatabaseSettingsViewModel()
        {
            DatabaseConnectionFileMode = Properties.Settings.Default.Database_ConnectionFileMode;
        }

        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;
        
        public string SettingsPageName => "_Database";
        
        public int EditorDisplayOrder => 15;

        public string GroupDisplayName => "Options";

        public object AutoGenContext => this;

        [Category("Connection")]
        [DisplayName("File mode")]
        [Description("Exclusive file mode is recommended to avoid issues")]
        public DatabaseFileMode DatabaseConnectionFileMode { get; set; }

        public void ApplyChanges()
        {
            Properties.Settings.Default.Database_ConnectionFileMode = DatabaseConnectionFileMode;

            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }

        
    }
}