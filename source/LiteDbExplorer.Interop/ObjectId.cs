extern alias v4;
extern alias v5;
using System;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public interface IObjectId : IComparable<IObjectId>, IEquatable<IObjectId>
    {
        bool Equals(object other);
        int GetHashCode();
        void ToByteArray(byte[] bytes, int startIndex);
        byte[] ToByteArray();
        string ToString();
        int Timestamp { get; }
        int Machine { get; }
        short Pid { get; }
        int Increment { get; }
        DateTime CreationTime { get; }
    }

    public class ObjectIdV4Adapter : LiteDBv4.ObjectId, IObjectId
    {
        private readonly LiteDBv4.ObjectId _objectId;

        public ObjectIdV4Adapter(LiteDBv4.ObjectId objectId)
        {
            _objectId = objectId;
        }

        public bool Equals(IObjectId other)
        {
            return _objectId.Equals(other);
        }

        public int CompareTo(IObjectId other)
        {
            return _objectId.CompareTo((LiteDBv4.ObjectId) other);
        }

        public void ToByteArray(byte[] bytes, int startIndex)
        {
            bytes[startIndex + 0] = (byte)(_objectId.Timestamp >> 24);
            bytes[startIndex + 1] = (byte)(_objectId.Timestamp >> 16);
            bytes[startIndex + 2] = (byte)(_objectId.Timestamp >> 8);
            bytes[startIndex + 3] = (byte)(_objectId.Timestamp);
            bytes[startIndex + 4] = (byte)(_objectId.Machine >> 16);
            bytes[startIndex + 5] = (byte)(_objectId.Machine >> 8);
            bytes[startIndex + 6] = (byte)(_objectId.Machine);
            bytes[startIndex + 7] = (byte)(_objectId.Pid >> 8);
            bytes[startIndex + 8] = (byte)(_objectId.Pid);
            bytes[startIndex + 9] = (byte)(_objectId.Increment >> 16);
            bytes[startIndex + 10] = (byte)(_objectId.Increment >> 8);
            bytes[startIndex + 11] = (byte)(_objectId.Increment);
        }
    }

    public class ObjectIdV5Adapter : LiteDBv5.ObjectId, IObjectId
    {
        public bool Equals(IObjectId other)
        {
            return base.Equals(other);
        }

        public int CompareTo(IObjectId other)
        {
            return base.CompareTo((LiteDBv5.ObjectId) other);
        }
    }
}