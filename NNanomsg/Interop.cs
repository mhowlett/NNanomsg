using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace NNanomsg
{
    /// <summary>
    /// This class can allow platforms to provide a custom method for loading the nanomsg library.
    /// 
    /// This uses the convention of a library being in:
    ///   Win32 - [architecture]/module.dll
    ///   Posix - [architecture]/libmodule.so
    /// 
    /// If you want to use Mono's dllmap instead, initialize the CustomLoadLibrary property to null
    /// to prevent dynamic loading.
    /// </summary>
    public static class NNLibraryLoader
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("libdl.so")]
        static extern IntPtr dlopen(String fileName, int flags);

        static NNLibraryLoader()
        {
            if (Environment.OSVersion.Platform.ToString().Contains("Win32"))
                CustomLoadLibrary = LoadWindowsLibrary;
            else if (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX ||
                    (int)Environment.OSVersion.Platform == 128)
                CustomLoadLibrary = LoadPosixLibrary;
        }

        static void LoadWindowsLibrary(string libName)
        {
            string libFile = libName + ".dll";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile);

            if (File.Exists(fullPath))
            {
                IntPtr result = LoadLibrary(fullPath);
                Trace.Assert(result != IntPtr.Zero);
            }
            else
            {
                fullPath = Path.Combine(rootDirectory, libFile);
                if (File.Exists(fullPath))
                {
                    IntPtr result = LoadLibrary(fullPath);
                    Trace.Assert(result != IntPtr.Zero);
                }
            }
        }

        static void LoadPosixLibrary(string libName)
        {
            const int RTLD_NOW = 2;
            string libFile = "lib" + libName + ".so";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile);

            if (File.Exists(fullPath))
                dlopen(fullPath, RTLD_NOW);
            else
            {
                fullPath = Path.Combine(rootDirectory, libFile);
                if (File.Exists(fullPath))
                    dlopen(fullPath, RTLD_NOW);
            }
        }

        public static Action<string> CustomLoadLibrary { get; set; }
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    static unsafe class Interop
    {
        static Interop()
        {
            // if a custom method is specified for loading these libraries, let it take over
            if (NNLibraryLoader.CustomLoadLibrary != null)
            {
                NNLibraryLoader.CustomLoadLibrary("Nanomsg");
                NNLibraryLoader.CustomLoadLibrary("Nanomsgx");
            }
        }

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_socket(int domain, int protocol);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_connect(int s, [MarshalAs(UnmanagedType.LPStr)]string addr);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_bind(int s, [MarshalAs(UnmanagedType.LPStr)]string addr);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_send(int s, byte[] buf, int len, int flags);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_recv(int s, byte[] buf, int len, int flags);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_recv(int s, ref IntPtr buf, int len, int flags);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_errno();

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_close(int s);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_shutdown(int s, int how);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string nn_strerror(int errnum);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_device(int s1, int s2);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void nn_term();

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_setsockopt", SetLastError = false)]
        public static extern int nn_setsockopt_string(int s, int level, int option, [MarshalAs(UnmanagedType.LPStr)]string optval, int optvallen);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_setsockopt", SetLastError = false)]
        public static extern int nn_setsockopt_int(int s, int level, int option, ref int optval, int optvallen);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_getsockopt(int s, int level, int option, ref int optval, ref int optvallen);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_getsockopt(int s, int level, int option, ref IntPtr optval, ref int optvallen);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_getsockopt(int s, int level, int option, [MarshalAs(UnmanagedType.LPStr)] ref string optval, ref int optvallen);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int nn_freemsg(IntPtr msg);

        [DllImport("Nanomsg", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public  static  extern string  nn_symbol(int i, out int value);

        [DllImport("Nanomsgx", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void nn_poll(int[] s, int slen, int events, int timeout, int[] res);
    }
}
