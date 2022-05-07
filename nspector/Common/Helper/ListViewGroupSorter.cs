namespace nspector.Common.Helper;

public class ListViewGroupHeaderSorter:System.Collections.Generic.IComparer<System.Windows.Forms.ListViewGroup>
{
    readonly bool _ascending=true;

    public ListViewGroupHeaderSorter(bool ascending)=>this._ascending=ascending;

#region IComparer<ListViewGroup> Members

    public int Compare(System.Windows.Forms.ListViewGroup x,System.Windows.Forms.ListViewGroup y)
    {
        if(this._ascending)
        {
            return string.Compare(x.Header,y.Header);
        }

        return string.Compare(y.Header,x.Header);
    }

#endregion
}

public class ListViewGroupSorter
{
    internal System.Windows.Forms.ListView _listview;

    internal ListViewGroupSorter(System.Windows.Forms.ListView listview)=>this._listview=listview;

    public static bool operator==(System.Windows.Forms.ListView listview,ListViewGroupSorter sorter)
        =>listview==sorter._listview;

    public static bool operator!=(System.Windows.Forms.ListView listview,ListViewGroupSorter sorter)
        =>listview!=sorter._listview;

    public static implicit operator System.Windows.Forms.ListView(ListViewGroupSorter sorter)=>sorter._listview;

    public static implicit operator ListViewGroupSorter(System.Windows.Forms.ListView listview)
        =>new ListViewGroupSorter(listview);

    public void SortGroups(bool ascending)
    {
        this._listview.BeginUpdate();
        var lvgs=new System.Collections.Generic.List<System.Windows.Forms.ListViewGroup>();
        foreach(System.Windows.Forms.ListViewGroup lvg in this._listview.Groups)
        {
            lvgs.Add(lvg);
        }

        this._listview.Groups.Clear();
        lvgs.Sort(new ListViewGroupHeaderSorter(ascending));
        this._listview.Groups.AddRange(lvgs.ToArray());
        this._listview.EndUpdate();
    }

#region overridden methods

    public override bool Equals(object obj)=>this._listview.Equals(obj);

    public override int GetHashCode()=>this._listview.GetHashCode();

    public override string ToString()=>this._listview.ToString();

#endregion
}