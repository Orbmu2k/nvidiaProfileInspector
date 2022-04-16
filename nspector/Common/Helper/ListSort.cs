using System.Collections;

namespace nspector.Common.Helper;

internal class ListSort : IComparer
{
    public int Compare(object x, object y)
    {
        try
        {
            return string.CompareOrdinal(x.ToString(), y.ToString());
        }
        catch
        {
            return 0;
        }
    }
}