using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace NNanomsg
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct nn_iovec
    {
        public void* iov_base;
        public int iov_len;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct nn_msghdr
    {
        public nn_iovec* msg_iov;
        public int msg_iovlen;
        public void* msg_control;
        public int msg_controllen;
    }

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
    public static class NanomsgLibraryLoader
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, String procname);

        [DllImport("libdl.so")]
        static extern IntPtr dlopen(String fileName, int flags);

        [DllImport("libdl.so")]
        static extern IntPtr dlsym(IntPtr handle, String symbol);

        static NanomsgLibraryLoader()
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

            foreach (var path in new[] {
                    Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, libFile)
                })
            {
                if (File.Exists(path))
                {
                    var addr = LoadLibrary(path);
                    Trace.Assert(addr != IntPtr.Zero);

                    Console.WriteLine("lib: " + addr);

                    if (libName == "Nanomsg")
                    {
                        InitializeDelegates(addr, GetProcAddress);
                    }
                    else
                    {
                        IntPtr procaddr = GetProcAddress(addr, "nn_poll");
                        Console.WriteLine("poll: " + procaddr);
                        Interop.nn_poll =
                            (Interop.nn_poll_delegate)
                            Marshal.GetDelegateForFunctionPointer(procaddr, typeof(Interop.nn_poll_delegate));
                    }

                    return;
                }
            }
        }

        private delegate IntPtr SymbolLookup(IntPtr addr, string name);

        static void InitializeDelegates(IntPtr addr, SymbolLookup lookup)
        {
            Interop.nn_socket = (Interop.nn_socket_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_socket"), typeof(Interop.nn_socket_delegate));
            Interop.nn_connect = (Interop.nn_connect_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_connect"), typeof(Interop.nn_connect_delegate));
            Interop.nn_bind = (Interop.nn_bind_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_bind"), typeof(Interop.nn_bind_delegate));
            Interop.nn_setsockopt_int = (Interop.nn_setsockopt_int_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_setsockopt"), typeof(Interop.nn_setsockopt_int_delegate));
            Interop.nn_send = (Interop.nn_send_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_send"), typeof(Interop.nn_send_delegate));
            Interop.nn_recv = (Interop.nn_recv_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_recv"), typeof(Interop.nn_recv_delegate));
            Interop.nn_recv_intptr = (Interop.nn_recv_intptr_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_recv"), typeof(Interop.nn_recv_intptr_delegate));
            Interop.nn_errno = (Interop.nn_errno_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_errno"), typeof(Interop.nn_errno_delegate));
            Interop.nn_close = (Interop.nn_close_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_close"), typeof(Interop.nn_close_delegate));
            Interop.nn_shutdown = (Interop.nn_shutdown_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_shutdown"), typeof(Interop.nn_shutdown_delegate));
            Interop.nn_strerror = (Interop.nn_strerror_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_strerror"), typeof(Interop.nn_strerror_delegate));
            Interop.nn_device = (Interop.nn_device_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_device"), typeof(Interop.nn_device_delegate));
            Interop.nn_term = (Interop.nn_term_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_term"), typeof(Interop.nn_term_delegate));
            Interop.nn_setsockopt_string = (Interop.nn_setsockopt_string_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_setsockopt"), typeof(Interop.nn_setsockopt_string_delegate));
            Interop.nn_setsockopt_int = (Interop.nn_setsockopt_int_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_setsockopt"), typeof(Interop.nn_setsockopt_int_delegate));
            Interop.nn_getsockopt = (Interop.nn_getsockopt_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_getsockopt"), typeof(Interop.nn_getsockopt_delegate));
            Interop.nn_getsockopt_intptr = (Interop.nn_getsockopt_intptr_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_getsockopt"), typeof(Interop.nn_getsockopt_intptr_delegate));
            Interop.nn_getsockopt_string = (Interop.nn_getsockopt_string_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_getsockopt"), typeof(Interop.nn_getsockopt_string_delegate));
            Interop.nn_allocmsg = (Interop.nn_allocmsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_allocmsg"), typeof(Interop.nn_allocmsg_delegate));
            Interop.nn_freemsg = (Interop.nn_freemsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_freemsg"), typeof(Interop.nn_freemsg_delegate));
            Interop.nn_sendmsg = (Interop.nn_sendmsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_sendmsg"), typeof(Interop.nn_sendmsg_delegate));
            Interop.nn_recvmsg = (Interop.nn_recvmsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_recvmsg"), typeof(Interop.nn_recvmsg_delegate));
            Interop.nn_symbol = (Interop.nn_symbol_delegate)Marshal.GetDelegateForFunctionPointer(lookup(addr, "nn_symbol"), typeof(Interop.nn_symbol_delegate));

        }

        static void LoadPosixLibrary(string libName)
        {
            const int RTLD_NOW = 2;
            string libFile = "lib" + libName.ToLower() + ".so";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 
            // http://stackoverflow.com/questions/13461989/p-invoke-to-dynamically-loaded-library-on-mono
            
            foreach (var path in new [] {
                    Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, libFile),
                    Path.Combine("/usr/local/lib", libName),
                    Path.Combine("/usr/lib", libName)
                })
            {
                if (File.Exists(path))
                {
                    var addr = dlopen(path, RTLD_NOW);
                    Trace.Assert(addr != IntPtr.Zero);

                    Console.WriteLine("lib: " + addr);
                    
                    if (libName == "Nanomsg")
                    {
                        InitializeDelegates(addr, dlsym);
                    }
                    else
                    {
                        IntPtr procaddr = dlsym(addr, "nn_poll");
                        Console.WriteLine("poll: " + procaddr);
                        Interop.nn_poll =
                            (Interop.nn_poll_delegate)
                            Marshal.GetDelegateForFunctionPointer(procaddr, typeof(Interop.nn_poll_delegate));
                    }
                    
                    return;
                }
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
            if (NanomsgLibraryLoader.CustomLoadLibrary != null)
            {
                NanomsgLibraryLoader.CustomLoadLibrary("Nanomsg");
                NanomsgLibraryLoader.CustomLoadLibrary("Nanomsgx");
            }
        }

        public delegate int nn_socket_delegate(int domain, int protocol);
        public static nn_socket_delegate nn_socket;
        
        public delegate int nn_connect_delegate(int s, [MarshalAs(UnmanagedType.LPStr)]string addr);
        public static nn_connect_delegate nn_connect;

        public delegate int nn_bind_delegate(int s, [MarshalAs(UnmanagedType.LPStr)]string addr);
        public static nn_bind_delegate nn_bind;

        public delegate int nn_send_delegate(int s, byte[] buf, int len, int flags);
        public static nn_send_delegate nn_send;

        public delegate int nn_recv_delegate(int s, byte[] buf, int len, int flags);
        public static nn_recv_delegate nn_recv;

        public delegate int nn_recv_intptr_delegate(int s, ref IntPtr buf, int len, int flags);
        public static nn_recv_intptr_delegate nn_recv_intptr;

        public delegate int nn_errno_delegate();
        public static nn_errno_delegate nn_errno;

        public delegate int nn_close_delegate(int s);
        public static nn_close_delegate nn_close;

        public delegate int nn_shutdown_delegate(int s, int how);
        public static nn_shutdown_delegate nn_shutdown;

        // the return value can't be automatically marshalled to a string, as the framework tries to deallocate the pointer, which in this case is a literal
        // use Marshal.PtrToStringAnsi
        public delegate IntPtr nn_strerror_delegate(int errnum);
        public static nn_strerror_delegate nn_strerror;

        public delegate int nn_device_delegate(int s1, int s2);
        public static nn_device_delegate nn_device;

        public delegate void nn_term_delegate();
        public static nn_term_delegate nn_term;

        public delegate int nn_setsockopt_string_delegate(int s, int level, int option, [MarshalAs(UnmanagedType.LPStr)]string optval, int optvallen);
        public static nn_setsockopt_string_delegate nn_setsockopt_string;

        public delegate int nn_setsockopt_int_delegate(int s, int level, int option, ref int optval, int optvallen);
        public static nn_setsockopt_int_delegate nn_setsockopt_int;


        public delegate int nn_getsockopt_delegate(int s, int level, int option, ref int optval, ref int optvallen);
        public static nn_getsockopt_delegate nn_getsockopt;

        public delegate int nn_getsockopt_intptr_delegate(int s, int level, int option, ref IntPtr optval, ref int optvallen);
        public static nn_getsockopt_intptr_delegate nn_getsockopt_intptr;

        public delegate int nn_getsockopt_string_delegate(int s, int level, int option, [MarshalAs(UnmanagedType.LPStr)] ref string optval, ref int optvallen);
        public static nn_getsockopt_string_delegate nn_getsockopt_string;

        public delegate IntPtr nn_allocmsg_delegate(int size, int type);
        public static nn_allocmsg_delegate nn_allocmsg;

        public delegate int nn_freemsg_delegate(IntPtr msg);
        public static nn_freemsg_delegate nn_freemsg;

        public delegate int nn_sendmsg_delegate(int s, nn_msghdr* msghdr, int flags);
        public static nn_sendmsg_delegate nn_sendmsg;

        public delegate int nn_recvmsg_delegate(int s, nn_msghdr* msghdr, int flags);
        public static nn_recvmsg_delegate nn_recvmsg;

        //[return: MarshalAs(UnmanagedType.LPStr)]
        public delegate IntPtr nn_symbol_delegate(int i, out int value);
        public static nn_symbol_delegate nn_symbol;


        // -- nanomsgx

        public delegate int nn_poll_delegate(int[] s, int slen, int events, int timeout, int[] res);
        public static nn_poll_delegate nn_poll;
        
    }
}
