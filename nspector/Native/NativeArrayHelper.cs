namespace nspector.Native;

class NativeArrayHelper
{
    public static T GetArrayItemData<T>(System.IntPtr sourcePointer)
        =>(T)System.Runtime.InteropServices.Marshal.PtrToStructure(sourcePointer,typeof(T));

    public static T[] GetArrayData<T>(System.IntPtr sourcePointer,int itemCount)
    {
        var lstResult=new System.Collections.Generic.List<T>();
        if(sourcePointer!=System.IntPtr.Zero&&itemCount>0)
        {
            var sizeOfItem=System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            for(var i=0;i<itemCount;i++)
            {
                lstResult.Add(NativeArrayHelper.GetArrayItemData<T>(sourcePointer+sizeOfItem*i));
            }
        }

        return lstResult.ToArray();
    }

    public static void SetArrayData<T>(T[] items,out System.IntPtr targetPointer)
    {
        if(items!=null&&items.Length>0)
        {
            var sizeOfItem=System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            targetPointer=System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfItem*items.Length);
            for(var i=0;i<items.Length;i++)
            {
                System.Runtime.InteropServices.Marshal.StructureToPtr(items[i],targetPointer+sizeOfItem*i,true);
            }
        }
        else
        {
            targetPointer=System.IntPtr.Zero;
        }
    }

    public static void SetArrayItemData<T>(T item,out System.IntPtr targetPointer)
    {
        var sizeOfItem=System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
        targetPointer=System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfItem);
        System.Runtime.InteropServices.Marshal.StructureToPtr(item,targetPointer,true);
    }
}