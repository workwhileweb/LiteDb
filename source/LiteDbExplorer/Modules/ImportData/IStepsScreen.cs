using System.ComponentModel;
using System.Threading.Tasks;

namespace LiteDbExplorer.Modules.ImportData
{
    public interface IStepsScreen : INotifyPropertyChanged
    {
        bool HasNext { get; }
        bool CanContentScroll { get; }
        Task<object> Next();
        bool Validate();
    }
}