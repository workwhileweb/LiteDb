using System.Collections;

namespace LiteDbExplorer.Core
{
    public class SortBsonValue : IComparer
    {
        private readonly string _key;
        private readonly bool _reverse;

        public SortBsonValue(string key, bool reverse)
        {
            _key = key;
            _reverse = reverse;
        }

        public int Compare(object x, object y)
        {
            var doc1 = x as DocumentReference;
            var doc2 = y as DocumentReference;

            if(doc1 == null && doc2 == null)
            {
                return 0;
            }

            if(doc1 == null)
            {
                return _reverse ? 1 : -1;
            }

            if(doc2 == null)
            {
                return _reverse ? -1 : 1;
            }

            var bsonValue1 = doc1.LiteDocument[_key];
            var bsonValue2 = doc2.LiteDocument[_key];

            if (_reverse)
            {
                return bsonValue2.CompareTo(bsonValue1);
            }

            return bsonValue1.CompareTo(bsonValue2);
        }
    }
}