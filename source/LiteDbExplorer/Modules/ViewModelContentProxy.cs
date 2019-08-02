using Caliburn.Micro;

namespace LiteDbExplorer.Modules
{
    public class ViewModelContentProxy
    {
        public ViewModelContentProxy()
        {
        }

        public ViewModelContentProxy(IScreen content)
        {
            Content = content;
        }

        public IScreen Content { get; set; }
    }
}