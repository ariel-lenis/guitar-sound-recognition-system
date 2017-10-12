using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TsFilesTools
{
    public static class IntPtrExtensors
    {

        public static IntPtr ToIntPtr(this byte[] data)
        {
            IntPtr ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            return ptr;
        }
        public static IntPtr ToIntPtr(this short[] data,int ptrsize)
        {
            if (ptrsize < data.Length)
                throw new Exception("The pointer size must be greater or equal that the data size.");
            IntPtr ptr = Marshal.AllocHGlobal(ptrsize*sizeof(short));
            Marshal.Copy(data, 0, ptr, data.Length);
            return ptr;
        }
        public static IntPtr ToIntPtr(this int[] data)
        {
            IntPtr ptr = Marshal.AllocHGlobal(data.Length * sizeof(int));
            Marshal.Copy(data, 0, ptr, data.Length);
            return ptr;
        }
        public static void FreeHGlobalIntPtr(this IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
