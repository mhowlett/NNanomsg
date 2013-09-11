using System;
using System.Runtime.InteropServices;

namespace NNanomsg
{
    public enum Domain
    {
        SP = 1,
        SP_RAW = 2
    }

    public enum SocketOptions
    {
        LINGER = 1,
        SNDBUF = 2,
        RCVBUF = 3,
        SNDTIMEO = 4,
        RCVTIMEO = 5,
        RECONNECT_IVL = 6,
        RECONNECT_IVL_MAX = 7,
        SNDPRIO = 8,
        SNDFD = 10,
        RCVFD = 11,
        DOMAIN = 12,
        PROTOCOL = 13,
        IPV4ONLY = 14
    }

    public enum Protocol
    {
        PAIR = Constants.NN_PAIR,
        PUB = Constants.NN_PUB,
        SUB = Constants.NN_SUB,
        REQ = Constants.NN_REQ,
        REP = Constants.NN_REP,
        PUSH = Constants.NN_PUSH,
        PULL = Constants.NN_PULL,
        SURVEYOR = Constants.NN_SURVEYOR,
        RESPONDENT = Constants.NN_RESPONDENT,
        BUS = Constants.NN_BUS
    }

    internal class Constants
    {
        public const int NN_DONTWAIT = 1;

        // pair protocol related constants
        private const int NN_PROTO_PAIR = 1;
        public const int NN_PAIR = NN_PROTO_PAIR * 16 + 0;

        // pubsub protocol related constants
        private const int NN_PROTO_PUBSUB = 2;
        public const int NN_PUB = NN_PROTO_PUBSUB * 16 + 0;
        public const int NN_SUB = NN_PROTO_PUBSUB * 16 + 1;
        public const int NN_SUB_SUBSCRIBE = 1;
        public const int NN_SUB_UNSUBSCRIBE = 2;

        // reqrep protocol related constants
        private const int NN_PROTO_REQREP = 3;
        public const int NN_REQ = NN_PROTO_REQREP * 16 + 0;
        public const int NN_REP = NN_PROTO_REQREP * 16 + 1;
        public const int NN_REQ_RESEND_IVL = 1;

        // pipeline protocol related constants
        private const int NN_PROTO_PIPELINE = 5;
        public const int NN_PUSH = NN_PROTO_PIPELINE * 16 + 0;
        public const int NN_PULL = NN_PROTO_PIPELINE * 16 + 1;

        // survey protocol related constants
        private const int NN_PROTO_SURVEY = 6;
        public const int NN_SURVEYOR = NN_PROTO_SURVEY * 16 + 0;
        public const int NN_RESPONDENT = NN_PROTO_SURVEY * 16 + 1;
        public const int NN_SURVEYOR_DEADLINE = 1;

        // bus protocol related constants
        private const int NN_PROTO_BUS = 7;
        public const int NN_BUS = NN_PROTO_BUS * 16 + 0;        
    }

    public partial class NN
    {
        public static int Socket(Domain domain, Protocol protocol)
        {
            return UsingWindows
                       ? Interop_Windows.nn_socket((int) domain, (int) protocol)
                       : Interop_Linux.nn_socket((int) domain, (int) protocol);
        }

        public static int Connect(int s, string addr)
        {
            return UsingWindows ? Interop_Windows.nn_connect(s, addr + '\0') : Interop_Linux.nn_connect(s, addr + '\0');
        }

        public static int Bind(int s, string addr)
        {
            return UsingWindows ? Interop_Windows.nn_bind(s, addr + '\0') : Interop_Linux.nn_bind(s, addr + '\0');
        }

        public static int SetSocketOpt(int s, int level, int option, string val)
        {
            // todo: unsure if \0 termination is necessary. remove if not.
            return UsingWindows
                       ? Interop_Windows.nn_setsockopt_string(s, level, option, val + '\0', val.Length)
                       : Interop_Linux.nn_setsockopt_string(s, level, option, val + '\0', val.Length);
        }

        public static int SetSocketOpt(int s, int level, int option, int val)
        {
            return UsingWindows
                       ? Interop_Windows.nn_setsockopt_int(s, level, option, ref val, sizeof (int))
                       : Interop_Linux.nn_setsockopt_int(s, level, option, ref val, sizeof (int));
        }

        public static int GetSocketOpt(int s, int level, int option, ref int val)
        {
            IntPtr optval;
            int optvallen;

            int rc = UsingWindows
                         ? Interop_Windows.nn_getsockopt(s, level, option, out optval, out optvallen)
                         : Interop_Linux.nn_getsockopt(s, level, option, out optval, out optvallen);

            val = Marshal.ReadInt32(optval);

            return rc;
        }

        public static int GetSocketOpt(int s, int level, int option, out string val)
        {
            IntPtr optval;
            int optvallen;

            int rc = UsingWindows
                         ? Interop_Windows.nn_getsockopt(s, level, option, out optval, out optvallen)
                         : Interop_Linux.nn_getsockopt(s, level, option, out optval, out optvallen);

            val = Marshal.PtrToStringAnsi(optval, optvallen);

            return rc;
        }

        public static int Recv(int s, byte[] buf, int flags)
        {
            return UsingWindows
                       ? Interop_Windows.nn_recv(s, buf, buf.Length, flags)
                       : Interop_Linux.nn_recv(s, buf, buf.Length, flags);
        }

        public static int Send(int s, byte[] buf, int flags)
        {
            return UsingWindows
                       ? Interop_Windows.nn_send(s, buf, buf.Length, flags)
                       : Interop_Linux.nn_send(s, buf, buf.Length, flags);
        }

        public static int Errno()
        {
            return UsingWindows
                       ? Interop_Windows.nn_errno()
                       : Interop_Linux.nn_errno();
        }

        public static int Close(int s)
        {
            return UsingWindows
                       ? Interop_Windows.nn_close(s)
                       : Interop_Linux.nn_close(s);
        }

        public static int Shutdown(int s, int how)
        {
            return UsingWindows
                       ? Interop_Windows.nn_shutdown(s, how)
                       : Interop_Linux.nn_shutdown(s, how);
        }

        public static int Device(int s1, int s2)
        {
            return UsingWindows
                       ? Interop_Windows.nn_device(s1, s2)
                       : Interop_Linux.nn_device(s1, s2);
        }

        public static void Term()
        {
            if (UsingWindows)
            {
                Interop_Windows.nn_term();
            }
            else
            {
                Interop_Linux.nn_term();
            }
        }

        public static string StrError(int errnum)
        {
            return UsingWindows
                       ? Marshal.PtrToStringAnsi(Interop_Windows.nn_strerror(errnum))
                       : Marshal.PtrToStringAnsi(Interop_Linux.nn_strerror(errnum));
        }

        private static bool UsingWindows
        {
            get
            {
                if (_usingWindows == null)
                {
                    string os_s = Environment.OSVersion.ToString().ToLower();
                    _usingWindows = os_s.Contains("windows");
                }

                return _usingWindows.Value;
            }
        }

        private static bool? _usingWindows;
    }

}