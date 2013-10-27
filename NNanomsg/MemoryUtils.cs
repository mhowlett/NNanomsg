using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg
{
    public static class MemoryUtils
    {
        /// <summary>
        /// This isn't the fastest memory copy method for large reads (4k or above), but it has reliably good performance on all platforms.
        /// 
        /// This *is* the fastest method for reads up to around 512 bytes, on the platforms I tested.
        /// 
        /// Given that deserialization will be the most common consumption of this (protocol buffers and such), we will be far 
        /// more likely to see many small reads.  We could consider swapping to a platform-specific bulk copy path for larger sizes.
        /// </summary>
        public unsafe static void CopyMemory(byte* src, byte* dest, int length)
        {
            if (length >= 16)
            {
                do 
                {

                    *(long*)dest = *(long*) src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
                    dest += 16;
                    src += 16;
                }
                while ((length -= 16) >= 16);
            }
            if (length > 0)
            {
                if ((length & 8) != 0)
                {
                    *(long*)dest = *(long*)src;
                    dest += 8;
                    src += 8;
                }
                if ((length & 4) != 0)
                {
                    *(int*)dest = *(int*)src;
                    dest += 4;
                    src += 4;
                }
                if ((length & 2) != 0)
                {
                    *(short*)dest = *(short*)src;
                    dest += 2;
                    src += 2;
                }
                if ((length & 1) != 0)
                {
                    byte* finalByte = dest;
                    dest = finalByte + 1;
                    byte* finalByteSrc = src;
                    src = finalByteSrc + 1;
                    *finalByte = *finalByteSrc;
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = false)]
        static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

        public unsafe static void PinAndCopyMemory(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            if (len == 0)
                return;

            fixed (byte* ptr = dest)
                //memcpy((IntPtr)(ptr + destIndex), (IntPtr)(src + srcIndex), len);
                CopyMemory(src + srcIndex, ptr + destIndex, len);
        }
    }
}
