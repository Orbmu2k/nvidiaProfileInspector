namespace nvidiaProfileInspector.UI.ViewModels
{
    //public class ItemsChangeObservableCollection<T> :
    //       ObservableCollection<T> where T : INotifyPropertyChanged
    //{
    //    public ItemsChangeObservableCollection(List<T> collection) : base(collection) { }

    //    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    //    {
    //        if (e.Action == NotifyCollectionChangedAction.Add)
    //        {
    //            RegisterPropertyChanged(e.NewItems);
    //        }
    //        else if (e.Action == NotifyCollectionChangedAction.Remove)
    //        {
    //            UnRegisterPropertyChanged(e.OldItems);
    //        }
    //        else if (e.Action == NotifyCollectionChangedAction.Replace)
    //        {
    //            UnRegisterPropertyChanged(e.OldItems);
    //            RegisterPropertyChanged(e.NewItems);
    //        }

    //        base.OnCollectionChanged(e);
    //    }

    //    protected override void ClearItems()
    //    {
    //        UnRegisterPropertyChanged(this);
    //        base.ClearItems();
    //    }

    //    private void RegisterPropertyChanged(IList items)
    //    {
    //        foreach (INotifyPropertyChanged item in items)
    //        {
    //            if (item != null)
    //            {
    //                item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
    //            }
    //        }
    //    }

    //    private void UnRegisterPropertyChanged(IList items)
    //    {
    //        foreach (INotifyPropertyChanged item in items)
    //        {
    //            if (item != null)
    //            {
    //                item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
    //            }
    //        }
    //    }

    //    private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //    {
    //        base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
    //    }
    //}
}
