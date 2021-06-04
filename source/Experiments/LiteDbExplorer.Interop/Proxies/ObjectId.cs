extern alias v4;
extern alias v5;
using System;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public interface IObjectIdProxy : IComparable<IObjectIdProxy>, IEquatable<IObjectIdProxy>
    {
        int Timestamp { get; }
        int Machine { get; }
        short Pid { get; }
        int Increment { get; }
        DateTime CreationTime { get; }
        void ToByteArray(byte[] bytes, int startIndex);
        byte[] ToByteArray();
    }

    public class ObjectIdProxyV4 : IObjectIdProxy
    {
        private readonly LiteDBv4.ObjectId _objectId;
        
        public ObjectIdProxyV4(LiteDBv4.ObjectId objectId)
        {
            _objectId = objectId;
        }

        public int Timestamp => _objectId.Timestamp;

        public int Machine => _objectId.Machine;

        public short Pid => _objectId.Pid;

        public int Increment => _objectId.Increment;

        public DateTime CreationTime => _objectId.CreationTime;

        public bool Equals(IObjectIdProxy other)
        {
            var proxy = other as ObjectIdProxyV4;
            return _objectId.Equals(proxy?._objectId);
        }

        public int CompareTo(IObjectIdProxy other)
        {
            var proxy = other as ObjectIdProxyV4;
            return _objectId.CompareTo(proxy?._objectId);
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

        public byte[] ToByteArray()
        {
            return _objectId.ToByteArray();
        }

        public override int GetHashCode()
        {
            return _objectId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _objectId.Equals(obj);
        }

        public override string ToString()
        {
            return _objectId.ToString();
        }
    }

    public class ObjectIdProxyV5 : IObjectIdProxy
    {
        private readonly LiteDBv5.ObjectId _objectId;

        public ObjectIdProxyV5(LiteDBv5.ObjectId objectId)
        {
            _objectId = objectId;
        }

        public int Timestamp => _objectId.Timestamp;

        public int Machine => _objectId.Machine;

        public short Pid => _objectId.Pid;

        public int Increment => _objectId.Increment;

        public DateTime CreationTime => _objectId.CreationTime;

        public int CompareTo(IObjectIdProxy other)
        {
            var proxy = other as ObjectIdProxyV5;
            return _objectId.CompareTo(proxy?._objectId);
        }

        public bool Equals(IObjectIdProxy other)
        {
            var proxy = other as ObjectIdProxyV5;
            return _objectId.Equals(proxy?._objectId);
        }

        public void ToByteArray(byte[] bytes, int startIndex)
        {
            _objectId.ToByteArray(bytes, startIndex);
        }

        public byte[] ToByteArray()
        {
            return _objectId.ToByteArray();
        }

        public override int GetHashCode()
        {
            return _objectId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _objectId.Equals(obj);
        }

        public override string ToString()
        {
            return _objectId.ToString();
        }
    }
}