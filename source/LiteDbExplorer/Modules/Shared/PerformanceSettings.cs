using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PerformanceSettings: PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public PerformanceSettings()
        {
            DeferredScrolling = Properties.Settings.Default.DeferredScrolling;
        }

        public string SettingsPageName => Properties.Resources.SettingsPageAdvanced;

        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;

        public int EditorDisplayOrder => 100;

        public string GroupDisplayName => "Performance";

        public object AutoGenContext => this;

        [DisplayName("Use Deferred Scrolling")]
        public bool DeferredScrolling { get; set; }

        public void ApplyChanges()
        {
            Properties.Settings.Default.DeferredScrolling = DeferredScrolling;
            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
    }
}