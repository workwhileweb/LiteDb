using System.Collections.Generic;

namespace LiteDbExplorer.Framework.Shell
{
    public class DisplayOrderComparer : IComparer<IHaveDisplayOrder>
    {
        public static readonly DisplayOrderComparer Default = new DisplayOrderComparer();

        public int Compare(IHaveDisplayOrder x, IHaveDisplayOrder y)
        {
            int result;

            // first check to see if lhs is null.
            if (x == null)
            {
                // if lhs null, check rhs to decide on return value.
                if (y == null)
                {
                    result = 0;
                }
                else
                {
                    result = -1;
                }
            }
            else
            {
                result = x.DisplayOrder.CompareTo(y?.DisplayOrder);
            }

            return result;
        }
    }
}