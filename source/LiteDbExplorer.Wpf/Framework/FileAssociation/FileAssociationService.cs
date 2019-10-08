using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Serilog;

namespace LiteDbExplorer.Wpf.Framework.FileAssociation
{
    [Export(typeof(IFileAssociationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FileAssociationService : IFileAssociationService
    {
        // private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static readonly ILogger Logger = Log.ForContext<ApplicationRegistrationService>();

        public void AssociateFilesWithApplication([NotNull] string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(applicationName));
            }

            // applicationName = applicationName.GetApplicationName();

            Logger.Information("Associating files with '{0}'", applicationName);

            var applicationAssociationRegistrationUi = (IApplicationAssociationRegistrationUI)new ApplicationAssociationRegistrationUI();
            var hr = applicationAssociationRegistrationUi.LaunchAdvancedAssociationUI(applicationName);
            var exception = Marshal.GetExceptionForHR(hr);
            if (exception != null)
            {
                Log.Error(exception, "Failed to associate the files with application '{0}'", applicationName);
                throw exception;
            }

            Logger.Information("Associated files with '{0}'", applicationName);
        }

        [Guid("1f76a169-f994-40ac-8fc8-0959e8874710")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IApplicationAssociationRegistrationUI
        {
            [PreserveSig]
            int LaunchAdvancedAssociationUI([MarshalAs(UnmanagedType.LPWStr)] string pszAppRegName);
        }

        [ComImport]
        [Guid("1968106d-f3b5-44cf-890e-116fcb9ecef1")]
        public class ApplicationAssociationRegistrationUI
        {
        }
    }
}