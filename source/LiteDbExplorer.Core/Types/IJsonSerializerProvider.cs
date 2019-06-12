using System.IO;

namespace LiteDbExplorer.Core
{
    public interface IJsonSerializerProvider
    {
        string Serialize(bool pretty = false, bool decoded = true);
        void Serialize(TextWriter writer, bool pretty = false);
    }
}