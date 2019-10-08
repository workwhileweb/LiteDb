using System;
using JetBrains.Annotations;

namespace LiteDbExplorer.Wpf.Framework.FileAssociation
{
    public static class ApplicationRegistrationServiceExtensions
    {
        public static void UpdateRegistration([NotNull] this IApplicationRegistrationService applicationRegistrationService,
            [NotNull] ApplicationInfo applicationInfo)
        {
            if (applicationRegistrationService == null)
            {
                throw new ArgumentNullException(nameof(applicationRegistrationService));
            }

            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            // Just a forward call to the register application, maybe in the future we will uninstall / install
            applicationRegistrationService.RegisterApplication(applicationInfo);
        }
    }
}