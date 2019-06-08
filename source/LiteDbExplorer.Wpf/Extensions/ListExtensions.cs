using System.Collections.Generic;

namespace LiteDbExplorer.Presentation
{
    public static class ListExtensions
    {
        public static void AddSorted<T>(this IList<T> list, T item, IComparer<T> comparer = null)
        {
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
            }

            int i = 0;
            while (i < list.Count && comparer.Compare(list[i], item) < 0)
            {
                i++;
            }

            list.Insert(i, item);
        }
    }
}