using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Modules.Diagnostics
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DiagnosticsSettings : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public DiagnosticsSettings()
        {
            ShowManagedMemory = Properties.Settings.Default.Diagnostics_ShowManagedMemory;
        }

        public string SettingsPageName => Properties.Resources.SettingsPageAdvanced;

        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;

        public int EditorDisplayOrder => 200;

        public string GroupDisplayName => "Diagnostics";

        public object AutoGenContext => this;

        [DisplayName("Show managed memory usage in status bar")]
        public bool ShowManagedMemory { get; set; }

        public void ApplyChanges()
        {
            Properties.Settings.Default.Diagnostics_ShowManagedMemory = ShowManagedMemory;
            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }   
    }
}