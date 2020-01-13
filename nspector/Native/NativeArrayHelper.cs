using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nspector.Native
{
    internal class NativeArrayHelper
    {
        public static T GetArrayItemData<T>(IntPtr sourcePointer)
        {
            return (T)Marshal.PtrToStructure(sourcePointer, typeof(T));
        }

        public static T[] GetArrayData<T>(IntPtr sourcePointer, int itemCount)
        {
            var lstResult = new List<T>();
            if (sourcePointer != IntPtr.Zero && itemCount > 0)
            {
                var sizeOfItem = Marshal.SizeOf(typeof(T));
                for (int i = 0; i < itemCount; i++)
                {
                    lstResult.Add(GetArrayItemData<T>(sourcePointer + (sizeOfItem * i)));
                }
            }
            return lstResult.ToArray();
        }

        public static void SetArrayData<T>(T[] items, out IntPtr targetPointer)
        {
            if (items != null && items.Length > 0)
            {
                var sizeOfItem = Marshal.SizeOf(typeof(T));
                targetPointer = Marshal.AllocHGlobal(sizeOfItem * items.Length);
                for (int i = 0; i < items.Length; i++)
                {
                    Marshal.StructureToPtr(items[i], targetPointer + (sizeOfItem * i), true);
                }
            }
            else
            {
                targetPointer = IntPtr.Zero;
            }

        }

        public static void SetArrayItemData<T>(T item, out IntPtr targetPointer)
        {
            var sizeOfItem = Marshal.SizeOf(typeof(T));
            targetPointer = Marshal.AllocHGlobal(sizeOfItem);
            Marshal.StructureToPtr(item, targetPointer, true);
        }

    }
}
