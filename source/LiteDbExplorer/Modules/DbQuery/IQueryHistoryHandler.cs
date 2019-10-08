using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace LiteDbExplorer.Modules.DbQuery
{
    public interface IQueryHistoryHandler
    {
        Task<Result> InsertQuery(RawQueryHistory item);
    }
}