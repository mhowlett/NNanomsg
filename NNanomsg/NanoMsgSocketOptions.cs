using System;

namespace NNanomsg
{
    /// <summary>
    /// Provides access to a sockets various settings.  Each read or write marshals to the native library, so avoid preventable property access.
    /// </summary>
    public class NanomsgSocketOptions
    {
        int _socket;

        public NanomsgSocketOptions(int socket)
        {
            _socket = socket;
        }

        public static TimeSpan? GetTimespan(int socket, SocketOptionLevel level, SocketOption opts)
        {
            int value = 0, size = sizeof(int);
            int result = Interop.nn_getsockopt(socket, (int)level, (int)opts, ref value, ref size);
            if (result != 0)
                throw new NanomsgException(string.Format("nn_getsockopt {0}", opts));
            return value < 0 ? (TimeSpan?)null : TimeSpan.FromMilliseconds(value);
        }

        public static void SetTimespan(int socket, SocketOptionLevel level, SocketOption opts, TimeSpan? value)
        {
            int v = value.HasValue ? (int)value.Value.TotalMilliseconds : -1, size = sizeof(int);
            int result = Interop.nn_setsockopt_int(socket, (int)level, (int)opts, ref v, size);
            if (result != 0)
                throw new NanomsgException(string.Format("nn_setsockopt {0}", opts));
        }

        public static int GetInt(int socket, SocketOptionLevel level, SocketOption opts)
        {
            int value = 0, size = sizeof(int);
            int result = Interop.nn_getsockopt(socket, (int)level, (int)opts, ref value, ref size);
            if (result != 0)
                throw new NanomsgException(string.Format("nn_getsockopt {0}", opts));
            return value;
        }

        public static void SetInt(int socket, SocketOptionLevel level, SocketOption opts, int value)
        {
            int v = value, size = sizeof(int);
            int result = Interop.nn_setsockopt_int(socket, (int)level, (int)opts, ref v, size);
            if (result != 0)
                throw new NanomsgException(string.Format("nn_setsockopt {0}", opts));
        }

        public static string GetString(int socket, SocketOptionLevel level, SocketOption opts)
        {
            string value = null;
            int size = 0;
            int result = Interop.nn_getsockopt_string(socket, (int)level, (int)opts, ref value, ref size);
            if (result != 0)
                throw new NanomsgException(string.Format("nn_getsockopt {0}", opts));
            return value;
        }

        public static void SetString(int socket, SocketOptionLevel level, SocketOption opts, string value)
        {
            string v = value;
            int size = value.Length;
            int result = Interop.nn_setsockopt_string(socket, (int)level, (int)opts, v, size);
            if (result != 0)
                throw new NanomsgException(string.Format("nn_setsockopt {0}", opts));
        }

        /// <summary>
        /// Returns the domain constant as it was passed to nn_socket().
        /// </summary>
        public Domain Domain
        {
            get
            {
                return (Domain)GetInt(_socket, SocketOptionLevel.Default, SocketOption.DOMAIN);
            }
        }

        /// <summary>
        /// Returns the protocol constant as it was passed to nn_socket().
        /// </summary>
        public Protocol Protocol
        {
            get
            {
                return (Protocol)GetInt(_socket, SocketOptionLevel.Default, SocketOption.PROTOCOL);
            }
        }

        /// <summary>
        /// Specifies how long should the socket try to send pending outbound messages after nn_close() have been called, in milliseconds. Negative value means infinite linger. The type of the option is int. Default value is 1000 (1 second).
        /// </summary>
        public TimeSpan? Linger
        {
            get
            {
                return GetTimespan(_socket, SocketOptionLevel.Default, SocketOption.LINGER);
            }
            set
            {
                SetTimespan(_socket, SocketOptionLevel.Default, SocketOption.LINGER, value);
            }
        }

        /// <summary>
        /// Size of the send buffer, in bytes. To prevent blocking for messages larger than the buffer, exactly one message may be buffered in addition to the data in the send buffer. The type of this option is int. Default value is 128kB.
        /// </summary>
        public int SendBuffer
        {
            get
            {
                return GetInt(_socket, SocketOptionLevel.Default, SocketOption.SNDBUF);
            }
            set
            {
                SetInt(_socket, SocketOptionLevel.Default, SocketOption.SNDBUF, value);
            }
        }

        /// <summary>
        /// Size of the receive buffer, in bytes. To prevent blocking for messages larger than the buffer, exactly one message may be buffered in addition to the data in the receive buffer. The type of this option is int. Default value is 128kB.
        /// </summary>
        public int ReceiveBuffer
        {
            get
            {
                return GetInt(_socket, SocketOptionLevel.Default, SocketOption.RCVBUF);
            }
            set
            {
                SetInt(_socket, SocketOptionLevel.Default, SocketOption.RCVBUF, value);
            }
        }

        /// <summary>
        /// The timeout for send operation on the socket, in milliseconds. If message cannot be sent within the specified timeout, EAGAIN error is returned. Negative value means infinite timeout. The type of the option is int. Default value is -1.
        /// </summary>
        public TimeSpan? SendTimeout
        {
            get
            {
                return GetTimespan(_socket, SocketOptionLevel.Default, SocketOption.SNDTIMEO);
            }
            set
            {
                SetTimespan(_socket, SocketOptionLevel.Default, SocketOption.SNDTIMEO, value);
            }
        }

        /// <summary>
        /// The timeout for recv operation on the socket, in milliseconds. If message cannot be received within the specified timeout, EAGAIN error is returned. Negative value means infinite timeout. The type of the option is int. Default value is -1.
        /// </summary>
        public TimeSpan? ReceiveTimeout
        {
            get
            {
                return GetTimespan(_socket, SocketOptionLevel.Default, SocketOption.RCVTIMEO);
            }
            set
            {
                SetTimespan(_socket, SocketOptionLevel.Default, SocketOption.RCVTIMEO, value);
            }
        }

        /// <summary>
        /// For connection-based transports such as TCP, this option specifies how long to wait, in milliseconds, when connection is broken before trying to re-establish it. Note that actual reconnect interval may be randomised to some extent to prevent severe reconnection storms. The type of the option is int. Default value is 100 (0.1 second).
        /// </summary>
        public TimeSpan ReconnectInterval
        {
            get
            {
                return GetTimespan(_socket, SocketOptionLevel.Default, SocketOption.RECONNECT_IVL).Value;
            }
            set
            {
                SetTimespan(_socket, SocketOptionLevel.Default, SocketOption.RECONNECT_IVL, value);
            }
        }

        /// <summary>
        /// This option is to be used only in addition to NN_RECONNECT_IVL option. It specifies maximum reconnection interval. On each reconnect attempt, the previous interval is doubled until NN_RECONNECT_IVL_MAX is reached. Value of zero means that no exponential backoff is performed and reconnect interval is based only on NN_RECONNECT_IVL. If NN_RECONNECT_IVL_MAX is less than NN_RECONNECT_IVL, it is ignored. The type of the option is int. Default value is 0.
        /// </summary>
        public TimeSpan ReconnectIntervalMax
        {
            get
            {
                return GetTimespan(_socket, SocketOptionLevel.Default, SocketOption.RECONNECT_IVL_MAX).Value;
            }
            set
            {
                SetTimespan(_socket, SocketOptionLevel.Default, SocketOption.RECONNECT_IVL_MAX, value);
            }
        }

        /// <summary>
        /// Retrieves outbound priority currently set on the socket. This option has no effect on socket types that send messages to all the peers. However, if the socket type sends each message to a single peer (or a limited set of peers), peers with high priority take precedence over peers with low priority. The type of the option is int. Highest priority is 1, lowest priority is 16. Default value is 8.
        /// </summary>
        public int SendPriority
        {
            get
            {
                return GetInt(_socket, SocketOptionLevel.Default, SocketOption.SNDPRIO);
            }
            set
            {
                SetInt(_socket, SocketOptionLevel.Default, SocketOption.SNDPRIO, value);
            }
        }

        /// <summary>
        /// If set to 1, only IPv4 addresses are used. If set to 0, both IPv4 and IPv6 addresses are used. The type of the option is int. Default value is 1.
        /// </summary>
        public bool IPV4Only
        {
            get
            {
                return GetInt(_socket, SocketOptionLevel.Default, SocketOption.IPV4ONLY) == 1;
            }
            set
            {
                SetInt(_socket, SocketOptionLevel.Default, SocketOption.IPV4ONLY, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Retrieves a file descriptor that is readable when a message can be sent to the socket. The descriptor should be used only for polling and never read from or written to. The type of the option is same as the type of file descriptor on the platform. That is, int on POSIX-complaint platforms and SOCKET on Windows. The descriptor becomes invalid and should not be used any more once the socket is closed. This socket option is not available for unidirectional recv-only socket types.
        /// </summary>
        public IntPtr SendFileDescriptor
        {
            get
            {
                return new IntPtr(GetInt(_socket, SocketOptionLevel.Default, SocketOption.SNDFD));
            }
        }

        /// <summary>
        /// Retrieves a file descriptor that is readable when a message can be received from the socket. The descriptor should be used only for polling and never read from or written to. The type of the option is same as the type of file descriptor on the platform. That is, int on POSIX-complaint platforms and SOCKET on Windows. The descriptor becomes invalid and should not be used any more once the socket is closed. This socket option is not available for unidirectional send-only socket types.
        /// </summary>
        public IntPtr ReceiveFileDescriptor
        {
            get
            {
                return new IntPtr(GetInt(_socket, SocketOptionLevel.Default, SocketOption.RCVFD));
            }
        }


        public bool TcpNoDelay
        {
            get { return GetInt(_socket, SocketOptionLevel.Tcp, SocketOption.TCP_NODELAY) == 1; }
            set { SetInt(_socket, SocketOptionLevel.Tcp, SocketOption.TCP_NODELAY, value ? 1 : 0); }
        }
    }
}
