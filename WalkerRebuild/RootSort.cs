//using Sandbox.ModAPI;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        class RootSort : Comparer<Root>
        {
            public override int Compare(Root x, Root y)
            {
                if (x != null && y != null)
                    return x.MyIndex.CompareTo(y.MyIndex);
                else
                    return 0;
            }
        }

    }
}
