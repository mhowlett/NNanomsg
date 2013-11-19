using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NNanomsg
{
    public static partial class NN
    {
        public static int Socket(Domain domain, Protocol protocol)
        {
            return Interop.nn_socket((int)domain, (int)protocol);
        }

        public static int Connect(int s, string addr)
        {
            return Interop.nn_connect(s, addr);
        }

        public static int Bind(int s, string addr)
        {
            return Interop.nn_bind(s, addr);
        }

        public static int SetSockOpt(int s, SocketOption option, string val)
        {
            return Interop.nn_setsockopt_string(s, Constants.NN_SOL_SOCKET, (int)option, val, val.Length);
        }

        public static int SetSockOpt(int s, Protocol level, int option, string val)
        {
            return Interop.nn_setsockopt_string(s, (int)level, option, val, val.Length);
        }

        public static int SetSockOpt(int s, SocketOption option, int val)
        {
            return Interop.nn_setsockopt_int(s, Constants.NN_SOL_SOCKET, (int)option, ref val, sizeof(int));
        }

        public static int SetSockOpt(int s, Protocol level, int option, int val)
        {
            return Interop.nn_setsockopt_int(s, (int)level, option, ref val, sizeof(int));
        }

        public static int GetSockOpt(int s, SocketOption option, out int val)
        {
            int optvallen = sizeof(int);
            int optval = 0;

            int rc = Interop.nn_getsockopt(s, Constants.NN_SOL_SOCKET, (int)option, ref optval, ref optvallen);

            val = optval;

            return rc;
        }

        public static int GetSockOpt(int s, Protocol level, int option, out int val)
        {
            int optvallen = sizeof(int);
            int optval = 0;

            int rc = Interop.nn_getsockopt(s, (int)level, option, ref optval, ref optvallen);

            val = optval;

            return rc;
        }

        /// <summary>
        ///     Receives a message into an already allocated buffer
        ///     If the message is longer than the allocated buffer, it is truncated.
        /// </summary>
        /// <returns>
        ///     The length, in bytes, of the message.
        /// </returns>
        /*
        public static int Recv(int s, byte[] buf, SendRecvFlags flags)
        {
            return Interop.nn_recv(s, buf, buf.Length, (int)flags);
        }
        */

        /// <summary>
        ///     Receives a message of unknown length into a new buffer.
        /// </summary>
        /// <returns>
        ///     The error code from nn_freemsg. Should always be 0. Probably safe to ignore.
        /// </returns>
        /// <remarks>
        ///     This is a bit inefficient at the moment.
        /// </remarks>
        public static int Recv(int s, out byte[] buf, SendRecvFlags flags)
        {
            Console.WriteLine("a");
            IntPtr buffer = IntPtr.Zero;
            int rc = Interop.nn_recv(s, ref buffer, Constants.NN_MSG, (int)flags);
            Console.WriteLine("b");

            if (rc < 0)
            {
                buf = null;
                return rc;
            }

            int numberOfBytes = rc;

            // this inefficient, I'm sure there must be a better way.
            buf = new byte[numberOfBytes];
            for (int i = 0; i < numberOfBytes; ++i)
            {
                Console.WriteLine("c");
                buf[i] = Marshal.ReadByte(buffer, i);
            }
            
            Console.WriteLine("d");
            int rc2 = Interop.nn_freemsg(buffer);
            
            Debug.Assert(rc2 == 0);

            return rc;
        }

        public static int Send(int s, byte[] buf, SendRecvFlags flags)
        {
            return Interop.nn_send(s, buf, buf.Length, (int)flags);
        }

        public static int Errno()
        {
            return Interop.nn_errno();
        }

        public static int Close(int s)
        {
            return Interop.nn_close(s);
        }

        public static int Shutdown(int s, int how)
        {
            return Interop.nn_shutdown(s, how);
        }

        public static int Device(int s1, int s2)
        {
            return Interop.nn_device(s1, s2);
        }

        public static void Term()
        {
            Interop.nn_term();

        }

        public static int[] Poll(int[] s, Events events, TimeSpan? timeout)
        {
            int milliseconds = -1;
            if (timeout != null)
            {
                milliseconds = (int)timeout.Value.TotalMilliseconds;
            }

            var res = new int[s.Length];
            Interop.nn_poll(s, s.Length, (int)events, milliseconds, res);

            return res;
        }

        public static string StrError(int errnum)
        {
            return Marshal.PtrToStringAnsi(Interop.nn_strerror(errnum));
        }

        public static string Symbol(int i, out int value)
        {
            return Marshal.PtrToStringAnsi(Interop.nn_symbol(i, out value));
        }
    }

}