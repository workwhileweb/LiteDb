namespace LiteDbExplorer.Wpf.Framework.FileAssociation
{
    public interface IApplicationRegistrationService
    {
        bool IsApplicationRegistered(ApplicationInfo applicationInfo);
        void RegisterApplication(ApplicationInfo applicationInfo);
        void UnregisterApplication(ApplicationInfo applicationInfo);
    }
}