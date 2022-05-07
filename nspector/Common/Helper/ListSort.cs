namespace nspector.Common.Helper;

class ListSort:System.Collections.IComparer
{
    public int Compare(object x,object y)
    {
        try
        {
            return string.CompareOrdinal(x.ToString(),y.ToString());
        }
        catch
        {
            return 0;
        }
    }
}