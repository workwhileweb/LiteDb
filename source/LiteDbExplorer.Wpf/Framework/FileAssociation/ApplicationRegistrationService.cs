// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationRegistrationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using JetBrains.Annotations;
using Microsoft.Win32;
using Serilog;

namespace LiteDbExplorer.Wpf.Framework.FileAssociation
{
    [Export(typeof(IApplicationRegistrationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ApplicationRegistrationService : IApplicationRegistrationService
    {
        private const string ClassesRegistryKeyName = "Software\\Classes";
        private const string RegisteredApplicationRegistryKeyName = "Software\\RegisteredApplications";

        private static readonly ILogger Logger = Log.ForContext<ApplicationRegistrationService>();

        public virtual bool IsApplicationRegistered([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Checking if application '{0}' is registered", applicationInfo.Name);

            if (!IsApplicationAddedToClassesRoot(applicationInfo))
            {
                Logger.Debug("Application not added to classes root");
                return false;
            }

            if (!IsFileAssociationCapabilitiesAdded(applicationInfo))
            {
                Logger.Debug("Application not added to file association capabilities");
                return false;
            }

            if (!IsAppAddedToRegisteredApps(applicationInfo))
            {
                Logger.Debug("Application not added to registered apps");
                return false;
            }

            Logger.Debug("Application '{0}' is registered", applicationInfo.Name);

            return true;
        }

        public virtual void RegisterApplication([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Registering application '{0}'", applicationInfo.Name);

            // Step 1: Create app in the classes root
            AddApplicationToClassesRoot(applicationInfo);

            // Step 2: Create app in registry with file association capabilities
            AddFileAssociationCapabilities(applicationInfo);

            // Step 3: Add registered app
            AddAppToRegisteredApps(applicationInfo);

            Logger.Debug("Registered application '{0}'", applicationInfo.Name);
        }

        public virtual void UnregisterApplication([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Unregistering application '{0}'", applicationInfo.Name);

            RemoveApplicationFromClassesRoot(applicationInfo);
            RemoveFileAssociationCapabilities(applicationInfo);
            RemoveAppFromRegisteredApps(applicationInfo);

            Logger.Debug("Unregistered application '{0}'", applicationInfo.Name);
        }

        protected virtual bool IsApplicationAddedToClassesRoot([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            var registryHive = RegistryHive.CurrentUser;

            var registryKey = $"{ClassesRegistryKeyName}\\{applicationInfo.Name}";
            var keyExists = registryHive.IsRegistryKeyAvailable(registryKey);
            return keyExists;
        }

        protected virtual void AddApplicationToClassesRoot([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Adding application '{0}' to classes root", applicationInfo.Name);

            var registryHive = RegistryHive.CurrentUser;

            //[HKEY_CURRENT_USER\Software\Classes\MyAppHTML]
            //@="MyApp HTML Document"
            registryHive.SetRegistryValue($"{ClassesRegistryKeyName}\\{applicationInfo.Name}", "", applicationInfo.Title);

            //[HKEY_CURRENT_USER\Software\Classes\MyAppHTML\Application]
            //"ApplicationCompany"="Fictional Software Inc."
            registryHive.SetRegistryValue($"{ClassesRegistryKeyName}\\{applicationInfo.Name}\\Application", "ApplicationCompany", applicationInfo.Company);

            //[HKEY_CURRENT_USER\Software\Classes\MyAppHTML\shell]
            //@="open"
            registryHive.SetRegistryValue($"{ClassesRegistryKeyName}\\{applicationInfo.Name}\\shell", "", "open");

            //[HKEY_CURRENT_USER\Software\Classes\MyAppHTML\shell\open\command]
            //@="\"C:\\the app path\\testassoc.exe\""
            registryHive.SetRegistryValue($"{ClassesRegistryKeyName}\\{applicationInfo.Name}\\shell\\open\\command", "",
                $"\"{applicationInfo.Location}\" \"%1\"");
        }

        protected virtual void RemoveApplicationFromClassesRoot([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Removing application '{0}' from classes root", applicationInfo.Name);

            var registryHive = RegistryHive.CurrentUser;

            //[HKEY_CURRENT_USER\Software\Classes]
            registryHive.RemoveRegistryKey($"{ClassesRegistryKeyName}\\{applicationInfo.Name}");
        }

        protected virtual bool IsFileAssociationCapabilitiesAdded([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            var registryHive = RegistryHive.CurrentUser;

            var softwareKey = GetCurrentUserSoftwareKeyName(applicationInfo);
            var keyExists = registryHive.IsRegistryKeyAvailable(softwareKey);
            return keyExists;
        }

        protected virtual void AddFileAssociationCapabilities([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Adding file association capabilities '{0}' to current user", applicationInfo.Name);

            var registryHive = RegistryHive.CurrentUser;

            var softwareKey = GetCurrentUserSoftwareKeyName(applicationInfo);

            //[HKEY_CURRENT_USER\Software\FictionalSoftware\MyApp\Capabilities]
            //"ApplicationDescription" = "My Fictional Application"
            registryHive.SetRegistryValue($"{softwareKey}\\Capabilities", "ApplicationDescription", applicationInfo.Title);

            //[HKEY_CURRENT_USER\Software\FictionalSoftware\MyApp\Capabilities\FileAssociations]
            //".htm" = "MyAppHTML"
            //".html" = "MyAppHTML"
            foreach (var extension in applicationInfo.SupportedExtensions)
            {
                var finalExtension = extension;
                if (!finalExtension.StartsWith("."))
                {
                    finalExtension = "." + finalExtension;
                }

                Logger.Debug("Adding file association capability for extension '{0}'", finalExtension);

                registryHive.SetRegistryValue($"{softwareKey}\\Capabilities\\FileAssociations", finalExtension, applicationInfo.Name);
            }
        }

        protected virtual void RemoveFileAssociationCapabilities([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Removing file association capabilities '{0}' from current user", applicationInfo.Name);

            var registryHive = RegistryHive.CurrentUser;

            //[HKEY_CURRENT_USER\Software\FictionalSoftware\MyApp]
            var softwareKey = GetCurrentUserSoftwareKeyName(applicationInfo);
            registryHive.RemoveRegistryKey(softwareKey);
        }

        protected virtual bool IsAppAddedToRegisteredApps([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            var registryHive = RegistryHive.CurrentUser;

            var keyExists = registryHive.IsRegisteryValueAvailable(RegisteredApplicationRegistryKeyName, applicationInfo.Name);
            return keyExists;
        }

        protected virtual void AddAppToRegisteredApps([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Adding app {0}' to registered apps", applicationInfo.Name);

            var registryHive = RegistryHive.CurrentUser;

            //[HKEY_CURRENT_USER\Software\RegisteredApplications]
            //"MyApp" ="Software\\FictionalSoftware\\MyApp\\Capabilities"
            registryHive.SetRegistryValue(RegisteredApplicationRegistryKeyName, applicationInfo.Name,
                $"{GetCurrentUserSoftwareKeyName(applicationInfo)}\\Capabilities");
        }

        protected virtual void RemoveAppFromRegisteredApps([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            Logger.Debug("Removing app {0}' from registered apps", applicationInfo.Name);

            var registryHive = RegistryHive.CurrentUser;

            registryHive.RemoveRegistryValue(RegisteredApplicationRegistryKeyName, applicationInfo.Name);
        }

        protected virtual string GetCurrentUserSoftwareKeyName([NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            return $"Software\\{applicationInfo.Company}\\{applicationInfo.Name}";
        }
    }
}