using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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
            unsafe
            {
                var bs = Encoding.UTF8.GetBytes(val);
                fixed (byte* pBs = bs)
                {
                    return Interop.nn_setsockopt(s, Constants.NN_SOL_SOCKET, (int) option, new IntPtr(pBs), bs.Length);
                }
            }
        }

        public static int SetSockOpt(int s, SocketOptionLevel level, SocketOption option, string val)
        {
            unsafe
            {
                var bs = Encoding.UTF8.GetBytes(val);
                fixed (byte* pBs = bs)
                {
                    return Interop.nn_setsockopt(s, (int)level, (int)option, new IntPtr(pBs), bs.Length);
                }
            }
        }

        public static int SetSockOpt(int s, SocketOption option, int val)
        {
            unsafe
            {
                return Interop.nn_setsockopt(s, Constants.NN_SOL_SOCKET, (int) option, new IntPtr(&val), sizeof (int));
            }
        }

        public static int SetSockOpt(int s, SocketOptionLevel level, SocketOption option, int val)
        {
            unsafe
            {
                return Interop.nn_setsockopt(s, (int)level, (int)option, new IntPtr(&val), sizeof(int));
            }
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
        public static int Recv(int s, byte[] buf, SendRecvFlags flags)
        {
            return Interop.nn_recv_array(s, buf, buf.Length, (int)flags);
        }

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
            IntPtr buffer = IntPtr.Zero;
            int rc = Interop.nn_recv(s, ref buffer, Constants.NN_MSG, (int)flags);
            
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
                buf[i] = Marshal.ReadByte(buffer, i);
            }
            
            int rc_free = Interop.nn_freemsg(buffer);
            Debug.Assert(rc_free == 0);

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

        public static int[] Poll(int[] s, TimeSpan? timeout)
        {
            return Poll(s, s.Length, timeout);
        }

        public static int[] Poll(int[] s, int ct, TimeSpan? timeout)
        {
            var result = new int[ct];
            nn_pollfd[] pollfd = new nn_pollfd[ct];
            Poll(s, ct, result, pollfd, timeout);
            return result;
        }

        internal static void Poll(int[] s, int ct, int[] result, nn_pollfd[] info, TimeSpan? timeout)
        {
            int milliseconds = -1;
            if (timeout != null)
            {
                milliseconds = (int)timeout.Value.TotalMilliseconds;
            }
            else
            {
                milliseconds = int.MaxValue;
            }

            unsafe
            {
                for (int i = 0; i < ct; ++i)
                {
                    info[i] = new nn_pollfd { fd = s[i], events = (short)Events.POLLIN, revents = 0 };
                }

                fixed (nn_pollfd* pInfo = info)
                {
                    Interop.nn_poll(pInfo, ct, milliseconds);
                }
            }

            for (int i = 0; i < ct; ++i)
            {
                result[i] = (info[i].revents & (short)Events.POLLIN) != 0 ? 1 : 0;
            }
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