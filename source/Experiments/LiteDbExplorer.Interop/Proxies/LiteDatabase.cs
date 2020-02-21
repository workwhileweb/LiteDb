extern alias v4;
extern alias v5;
using System.Collections.Generic;
using System.IO;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public interface ILiteDatabaseProxy
    {
        LiteDBv5.ILiteCollection<T> GetCollection<T>(string name);
        LiteDBv5.ILiteCollection<T> GetCollection<T>();
        LiteDBv5.ILiteCollection<T> GetCollection<T>(LiteDBv5.BsonAutoId autoId);
        LiteDBv5.ILiteCollection<LiteDBv5.BsonDocument> GetCollection(string name, LiteDBv5.BsonAutoId autoId);
        bool BeginTrans();
        bool Commit();
        bool Rollback();
        LiteDBv5.ILiteStorage<TFileId> GetStorage<TFileId>(string filesCollection, string chunksCollection);
        IEnumerable<string> GetCollectionNames();
        bool CollectionExists(string name);
        bool DropCollection(string name);
        bool RenameCollection(string oldName, string newName);
        LiteDBv5.IBsonDataReader Execute(TextReader commandReader, LiteDBv5.BsonDocument parameters);
        LiteDBv5.IBsonDataReader Execute(string command, LiteDBv5.BsonDocument parameters);
        LiteDBv5.IBsonDataReader Execute(string command, params LiteDBv5.BsonValue[] args);
        void Checkpoint();
        long Rebuild(LiteDBv5.Engine.RebuildOptions options);
        // BsonValue Pragma(string name);
        // BsonValue Pragma(string name, BsonValue value);
        void Dispose();
        // BsonMapper Mapper { get; }
        LiteDBv5.ILiteStorage<string> FileStorage { get; }
        int UserVersion { get; set; }
    }

    public class LiteDatabaseProxyV5 : LiteDBv5.LiteDatabase, ILiteDatabaseProxy
    {
        public LiteDatabaseProxyV5(string connectionString, LiteDBv5.BsonMapper mapper = null) : base(connectionString, mapper)
        {
        }

        public LiteDatabaseProxyV5(LiteDBv5.ConnectionString connectionString, LiteDBv5.BsonMapper mapper = null) : base(connectionString, mapper)
        {
        }

        public LiteDatabaseProxyV5(Stream stream, LiteDBv5.BsonMapper mapper = null) : base(stream, mapper)
        {
        }
    }
}