extern alias v4;
extern alias v5;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public interface IBsonArrayProxy : IBsonValueProxy// , IList<IBsonValueProxy>
    {
        // void AddRange(IEnumerable<IBsonValueProxy> items);
    }

    public class BsonArrayProxyV4 : BsonValueProxyV4, IBsonArrayProxy
    {
        public BsonArrayProxyV4(LiteDBv4.BsonValue bsonValue) : base(bsonValue)
        {
        }
    }

    public class BsonArrayProxyV5 : BsonValueProxyV5, IBsonArrayProxy
    {
        public BsonArrayProxyV5(LiteDBv5.BsonValue bsonValue) : base(bsonValue)
        {
        }
    }
}