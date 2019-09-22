using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Principal;
using System.Windows.Input;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Wpf.Framework.FileAssociation;
using LiteDbExplorer.Wpf.Modules.Settings;

namespace LiteDbExplorer.Modules.WinInterop
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FileAssociationSettingsViewModel : PropertyChangedBase, ISettingsEditor, ILazyInitialize
    {
        private readonly Lazy<IApplicationRegistrationService> _applicationRegistrationService;
        private readonly Lazy<IFileAssociationService> _fileAssociationService;

        [ImportingConstructor]
        public FileAssociationSettingsViewModel(
            Lazy<IApplicationRegistrationService> applicationRegistrationService,
            Lazy<IFileAssociationService> fileAssociationService)
        {
            _applicationRegistrationService = applicationRegistrationService;
            _fileAssociationService = fileAssociationService;

            ApplicationInfo = new ApplicationInfo(System.Reflection.Assembly.GetEntryAssembly());

            FileAssociations = "db;";

            RegisterApplicationCommand = new RelayCommand(o => RegisterApplication(), o => RegisterApplicationCanExecute());

            UnregisterApplicationCommand = new RelayCommand(o => UnregisterApplication(), o => UnregisterApplicationCanExecute());
        }

        public string SettingsPageName => Properties.Resources.SettingsPageIntegration;

        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;

        public int EditorDisplayOrder => 60;
        
        [UsedImplicitly]
        public string GroupDisplayName => "Application Association";

        public ICommand RegisterApplicationCommand { get; }

        public ICommand UnregisterApplicationCommand { get; }

        public ApplicationInfo ApplicationInfo { get; }

        public string FileAssociations { get; set; }

        public bool IsApplicationRegistered { get; private set; }

        public bool IsUserAdministrator { get; private set; }

        public bool IsInitialized { get; set; }

        public void Init()
        {
            UpdateState();
            IsUserAdministrator = CurrentUserHasAdminRights();
        }

        public void ApplyChanges()
        {
            // Ignore
        }

        public void DiscardChanges()
        {
            // Ignore
        }

        private void RegisterApplication()
        {
            var applicationInfo = ApplicationInfo;

            applicationInfo.SupportedExtensions.Clear();
            foreach (var extension in FileAssociations.Split(new[] {",", ";"}, StringSplitOptions.RemoveEmptyEntries))
            {
                applicationInfo.SupportedExtensions.Add(extension);
            }

            _applicationRegistrationService.Value.RegisterApplication(ApplicationInfo);

            UpdateState();

            if (IsApplicationRegistered)
            {
                _fileAssociationService.Value.AssociateFilesWithApplication(ApplicationInfo.Name);
            }
        }

        private bool RegisterApplicationCanExecute()
        {
            return IsUserAdministrator && !IsApplicationRegistered;
        }

        private void UnregisterApplication()
        {
            _applicationRegistrationService.Value.UnregisterApplication(ApplicationInfo);

            UpdateState();
        }

        private bool UnregisterApplicationCanExecute()
        {
            return IsUserAdministrator && IsApplicationRegistered;
        }

        public bool CurrentUserHasAdminRights()
        {
            bool isAdmin = false;
            using (var user = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(user);
                // Check for token claim with well-known Administrators group SID
                const string LOCAL_ADMININSTRATORS_GROUP_SID = @"S-1-5-32-544";
                if (principal.Claims.FirstOrDefault(x => x.Value == LOCAL_ADMININSTRATORS_GROUP_SID) != null)
                {
                    isAdmin = true;
                }
            }

            return isAdmin;
        }

        private void UpdateState()
        {
            IsApplicationRegistered = _applicationRegistrationService.Value.IsApplicationRegistered(ApplicationInfo);
        }

        
    }
}