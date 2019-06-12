using Caliburn.Micro;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.Database
{
    public interface IDatabasePropertiesView : IScreen
    {
        void Init(DatabaseReference database);
    }
}