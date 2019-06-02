using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Modules
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class UpdateSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public UpdateSettingsViewModel()
        {
            CheckUpdateOnStartup = Properties.Settings.Default.UpdateManager_CheckUpdateOnStartup;
        }

        public string SettingsPageName => Properties.Resources.SettingsPageGeneral;

        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;

        public int EditorDisplayOrder => 50;

        public string GroupDisplayName => "Auto update";

        public object AutoGenContext => this;

        [DisplayName("Check for update at startup")]
        public bool CheckUpdateOnStartup { get; set; }

        public void ApplyChanges()
        {
            Properties.Settings.Default.UpdateManager_CheckUpdateOnStartup = CheckUpdateOnStartup;
            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
    }
}