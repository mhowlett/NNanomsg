﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NNanomsg
{
    public partial class NN
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        internal class Interop_Windows
        {
            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_socket(int domain, int protocol);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_connect(int s, string addr);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_bind(int s, string addr);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_send(int s, byte[] buf, int len, int flags);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_recv(int s, byte[] buf, int len, int flags);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_recv(int s, out IntPtr buf, int len, int flags);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_errno();

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_close(int s);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_shutdown(int s, int how);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr nn_strerror(int errnum);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_device(int s1, int s2);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void nn_term();

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_setsockopt")]
            public static extern int nn_setsockopt_string(int s, int level, int option, string optval, int optvallen);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_setsockopt")]
            public static extern int nn_setsockopt_int(int s, int level, int option, ref int optval, int optvallen);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_getsockopt(int s, int level, int option, ref int optval, ref int optvallen);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_getsockopt(int s, int level, int option, ref IntPtr optval, ref int optvallen);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_getsockopt(int s, int level, int option, ref string optval, ref int optvallen);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int nn_freemsg(IntPtr msg);

            [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr nn_symbol (int i, out int value);

            [DllImport("nanomsgx.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void nn_poll(int[] s, int slen, int events, int timeout, int[] res);
        }
    }
}
