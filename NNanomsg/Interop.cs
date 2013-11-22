using System;
using System.Runtime.InteropServices;
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
        static extern IntPtr dlerror();

        [DllImport("libdl.so")]
        static extern IntPtr dlsym(IntPtr handle, String symbol);


        static NanomsgLibraryLoader()
        {
            if (Environment.OSVersion.Platform.ToString().Contains("Win32"))
            {
                CustomLoadLibrary = LoadWindowsLibrary;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix ||
                     Environment.OSVersion.Platform == PlatformID.MacOSX ||
                     (int) Environment.OSVersion.Platform == 128)
            {
                CustomLoadLibrary = LoadPosixLibrary;
            }
        }

        static IntPtr LoadWindowsLibrary(string libName, out SymbolLookupDelegate symbolLookup)
        {
            string libFile = libName + ".dll";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var path in new[] {
                    Path.Combine(rootDirectory, "native", Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, libFile)
                })
            {
                if (File.Exists(path))
                {
                    var addr = LoadLibrary(path);
                    if (addr == IntPtr.Zero)
                    {
                        // Not using NanomsgException because it depends on nn_errno.
                        throw new Exception("LoadLibrary failed: " + path);
                    }
                    symbolLookup = GetProcAddress;
                    return addr;
                }
            }

            throw new Exception("LoadLibrary failed: unable to locate library " + libFile);
        }

        static IntPtr LoadPosixLibrary(string libName, out SymbolLookupDelegate symbolLookup)
        {
            const int RTLD_NOW = 2;
            string libFile = "lib" + libName.ToLower() + ".so";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var path in new [] {
                    Path.Combine(rootDirectory, "native", Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, libFile),
                    Path.Combine("/usr/local/lib", libFile),
                    Path.Combine("/usr/lib", libFile)
                })
            {
                if (File.Exists(path))
                {
                    var addr = dlopen(path, RTLD_NOW);
                    if (addr == IntPtr.Zero)
                    {
                        // Not using NanosmgException because it depends on nn_errno.
                        throw new Exception("dlopen failed: " + path + " : " +  Marshal.PtrToStringAnsi(dlerror()));
                    }
                    symbolLookup = dlsym;
                    return addr;
                }
            }

            throw new Exception("dlopen failed: unable to locate library "  + libFile);
        }

        public delegate IntPtr SymbolLookupDelegate(IntPtr addr, string name);

        public delegate IntPtr LoadLibraryDelegate(string libName, out SymbolLookupDelegate symbolLookup);
        
        public static LoadLibraryDelegate CustomLoadLibrary { get; set; }
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    static unsafe class Interop
    {
        static Interop()
        {
            if (NanomsgLibraryLoader.CustomLoadLibrary != null)
            {
                NanomsgLibraryLoader.SymbolLookupDelegate symbolLookup;
                var nanomsgAddr = NanomsgLibraryLoader.CustomLoadLibrary("Nanomsg", out symbolLookup);
                var nanomsgxAddr = NanomsgLibraryLoader.CustomLoadLibrary("Nanomsgx", out symbolLookup);
                
                InitializeDelegates(nanomsgAddr, nanomsgxAddr, symbolLookup);
            }
        }

        static void InitializeDelegates(IntPtr nanomsgAddr, IntPtr nanomsgxAddr, NanomsgLibraryLoader.SymbolLookupDelegate lookup)
        {
            // When running under mono and the native nanomsg libraries are in a non-standard location (e.g. are placed in application_dir/x86|x64), 
            // we cannot just load the native libraries and have everything automatically work. Hence all these delegates. 

            // TODO: The performance impact of this over conventional P/Invoke is evidently not good - there is about a 50% increase in overhead.
            // http://ybeernet.blogspot.com/2011/03/techniques-of-calling-unmanaged-code.html

            // There appears to be an alternative "dllmap" approach:
            // http://www.mono-project.com/Interop_with_Native_Libraries
            // but this requires config file entries.

            // Perhaps the method of calling native methods would better depend on which platform is being used. 

            // anyway, delegates work everywhere so that's what we'll use for now. get it working first, optimize later.. 

            // in many scenarios it won't matter anyway - sending or receiving data, over TCP at least - will dwarf 
            // the overhead of calling the delegate.

            nn_socket = (nn_socket_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_socket"), typeof(nn_socket_delegate));
            nn_connect = (nn_connect_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_connect"), typeof(nn_connect_delegate));
            nn_bind = (nn_bind_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_bind"), typeof(nn_bind_delegate));
            nn_send = (nn_send_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_send"), typeof(nn_send_delegate));
            nn_recv = (nn_recv_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_recv"), typeof(nn_recv_delegate));
            nn_recv_array = (nn_recv_array_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_recv"), typeof(nn_recv_array_delegate));
            nn_errno = (nn_errno_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_errno"), typeof(nn_errno_delegate));
            nn_close = (nn_close_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_close"), typeof(nn_close_delegate));
            nn_shutdown = (nn_shutdown_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_shutdown"), typeof(nn_shutdown_delegate));
            nn_strerror = (nn_strerror_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_strerror"), typeof(nn_strerror_delegate));
            nn_device = (nn_device_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_device"), typeof(nn_device_delegate));
            nn_term = (nn_term_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_term"), typeof(nn_term_delegate));
            nn_setsockopt_string = (nn_setsockopt_string_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_setsockopt"), typeof(nn_setsockopt_string_delegate));
            nn_setsockopt_int = (nn_setsockopt_int_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_setsockopt"), typeof(nn_setsockopt_int_delegate));
            nn_getsockopt = (nn_getsockopt_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_getsockopt"), typeof(nn_getsockopt_delegate));
            nn_getsockopt_intptr = (nn_getsockopt_intptr_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_getsockopt"), typeof(nn_getsockopt_intptr_delegate));
            nn_getsockopt_string = (nn_getsockopt_string_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_getsockopt"), typeof(nn_getsockopt_string_delegate));
            nn_allocmsg = (nn_allocmsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_allocmsg"), typeof(nn_allocmsg_delegate));
            nn_freemsg = (nn_freemsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_freemsg"), typeof(nn_freemsg_delegate));
            nn_sendmsg = (nn_sendmsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_sendmsg"), typeof(nn_sendmsg_delegate));
            nn_recvmsg = (nn_recvmsg_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_recvmsg"), typeof(nn_recvmsg_delegate));
            nn_symbol = (nn_symbol_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgAddr, "nn_symbol"), typeof(nn_symbol_delegate));

            nn_fd_size = (nn_fd_size_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgxAddr, "nn_fd_size"), typeof(nn_fd_size_delegate));
            nn_poll = (nn_poll_delegate)Marshal.GetDelegateForFunctionPointer(lookup(nanomsgxAddr, "nn_poll"), typeof(nn_poll_delegate));
        }

        public delegate int nn_socket_delegate(int domain, int protocol);
        public static nn_socket_delegate nn_socket;
        
        public delegate int nn_connect_delegate(int s, [MarshalAs(UnmanagedType.LPStr)]string addr);
        public static nn_connect_delegate nn_connect;

        public delegate int nn_bind_delegate(int s, [MarshalAs(UnmanagedType.LPStr)]string addr);
        public static nn_bind_delegate nn_bind;

        public delegate int nn_send_delegate(int s, byte[] buf, int len, int flags);
        public static nn_send_delegate nn_send;

        public delegate int nn_recv_delegate(int s, ref IntPtr buf, int len, int flags);
        public static nn_recv_delegate nn_recv;

        public delegate int nn_recv_array_delegate(int s, byte[] buf, int len, int flags);
        public static nn_recv_array_delegate nn_recv_array;

        public delegate int nn_errno_delegate();
        public static nn_errno_delegate nn_errno;

        public delegate int nn_close_delegate(int s);
        public static nn_close_delegate nn_close;

        public delegate int nn_shutdown_delegate(int s, int how);
        public static nn_shutdown_delegate nn_shutdown;

        // the return value can't be automatically marshalled to a string, as the framework tries to deallocate the pointer,
        // which in this case is a litera. Use Marshal.PtrToStringAnsi
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

        public delegate int nn_getsockopt_intptr_delegate(int s, int level, int option, IntPtr optval, ref int optvallen);
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

        public delegate int nn_poll_delegate(IntPtr rcvfds, int slen, int timeout, IntPtr res);
        public static nn_poll_delegate nn_poll;

        public delegate int nn_fd_size_delegate();
        public static nn_fd_size_delegate nn_fd_size;
    }
}
