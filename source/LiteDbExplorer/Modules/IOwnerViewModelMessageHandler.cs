namespace LiteDbExplorer.Modules
{
    public interface IOwnerViewModelMessageHandler
    {
        void Handle(string message, object payload = null);
    }
}