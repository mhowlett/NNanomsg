using System;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NNanomsg
{

    public abstract class NanomsgSocketBase : IDisposable
    {
        const int NullSocket = -1;

        public Domain Domain { get; private set; }
        public Protocol Protocol { get; private set; }
        public NanomsgSocketOptions Options { get; private set; }
        public int SocketID { get { return _socket; } }

        int _socket = NullSocket;
        INativeDisposer<NanomsgReadStream> _freeReadDisposer;
        INativeDisposer<NanomsgWriteStream> _freeWriteDisposer;
        NanomsgReadStream _recycledReadStream;
        NanomsgWriteStream _recycledWriteStream;

        /// <summary>
        /// Initialize a new nanomsg socket for the given domain and protocol
        /// </summary>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if a socket can't be created for this domain and protocol</exception>
        public NanomsgSocketBase(Domain domain, Protocol protocol)
        {
            Domain = domain;
            Protocol = protocol;

            _socket = Interop.nn_socket((int)Domain, (int)Protocol);
            if (_socket >= 0)
                Options = new NanomsgSocketOptions(_socket);
            else
                throw new NanomsgException(string.Format("nn_socket {0} {1}", domain, protocol));
        }

        /// <summary>
        /// Connects the socket to the remote address.  This can be called multiple times per socket.
        /// </summary>
        /// <param name="address">The addr argument consists of two parts as follows: transport://address. The transport specifies the underlying transport protocol to use. The meaning of the address part is specific to the underlying transport protocol.</param>
        /// <returns>An endpoint identifier which can be used to reference the connected endpoint in the future</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the address is invalid</exception>
        protected NanomsgEndpoint ConnectImpl(string address)
        {
            int endpoint = -1;
            endpoint = Interop.nn_connect(_socket, address);

            if (endpoint > 0)
                return new NanomsgEndpoint() { ID = endpoint };
            else
                throw new NanomsgException("nn_connect " + address);

        }

        /// <summary>
        /// Connects the socket to the remote address.  This can be called multiple times per socket.
        /// </summary>
        /// <param name="address">The IP address to which this client is connecting</param>
        /// <param name="port">The port number to which this client is connecting</param>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the address is invalid</exception>
        protected NanomsgEndpoint ConnectImpl(IPAddress address, int port)
        {
            int endpoint = -1;
            endpoint = Interop.nn_connect(_socket, string.Format("tcp://{0}:{1}", address, port));

            if (endpoint > 0)
                return new NanomsgEndpoint() { ID = endpoint };
            else
                throw new NanomsgException("nn_connect " + address);
        }

        /// <summary>
        /// Binds the socket to the local address.  This can be called multiple times per socket.
        /// </summary>
        /// <param name="address">The addr argument consists of two parts as follows: transport://address. The transport specifies the underlying transport protocol to use. The meaning of the address part is specific to the underlying transport protocol.</param>
        /// <returns>An endpoint identifier which can be used to reference the bound endpoint in the future</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the address is invalid</exception>
        protected NanomsgEndpoint BindImpl(string address)
        {
            int endpoint = -1;
            endpoint = Interop.nn_bind(_socket, address);

            if (endpoint > 0)
                return new NanomsgEndpoint() { ID = endpoint };
            else
                throw new NanomsgException("nn_bind " + address);
        }

        /// <summary>
        /// Shuts down a specific endpoint of this socket.
        /// </summary>
        /// <param name="endpoint">The endpoint created by Connect or Bind which is being shut down.</param>
        /// <returns>True if the endpoint was shut down, false if the shutdown attempt was interrupted and should be reattempted.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state or the endpoint's shutdown attempt was interrupted and should be reattempted.</exception>
        public bool Shutdown(NanomsgEndpoint endpoint)
        {
            const int ValidShutdownResult = 0, MaxShutdownAttemptCount = 5;
            int attemptCount = 0;
            while (true)
            {
                if (Interop.nn_shutdown(_socket, endpoint.ID) != ValidShutdownResult)
                {
                    int error = Interop.nn_errno();

                    // if we were interrupted by a signal, reattempt is allowed by the native library
                    if (error == NanomsgSymbols.EINTR)
                    {
                        if (attemptCount++ >= MaxShutdownAttemptCount)
                            return false;
                        else
                        {
                            Thread.SpinWait(1);
                            continue;
                        }
                    }

                    throw new NanomsgException("nn_shutdown " + endpoint.ID.ToString(), error);
                }
                return true;
            }
        }

        /// <summary>
        /// Closes the socket, releasing its resources and making it invalid for future use.
        /// </summary>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is invalid or the close attempt was interrupted and should be reattempted.</exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            const int ValidCloseResult = 0, MaxCloseAttemptCount = 5;

            // ensure that cleanup is only ever called once
            var socket = Interlocked.Exchange(ref _socket, NullSocket);

            if (socket != NullSocket)
            {
                int attemptCount = 0;
                while (true)
                {
                    if (Interop.nn_close(socket) != ValidCloseResult)
                    {
                        int error = Interop.nn_errno();

                        // if we were interrupted by a signal, reattempt is allowed by the native library
                        if (error == NanomsgSymbols.EINTR)
                        {
                            if (attemptCount++ >= MaxCloseAttemptCount)
                            {
                                if (disposing)
                                    throw new NanomsgException("nn_close " + socket.ToString(), error);
                                else
                                {
                                    // if we couldn't close the socket and we're on a finalizer thread, an exception would usually kill the process
                                    string errorText = string.Format("nn_close was repeatedly interrupted for socket {0}, which has not been successfully closed and may be leaked", socket);
                                    Trace.TraceError(errorText);
                                    Debug.Fail(errorText);
                                    return;
                                }
                            }
                            else
                            {
                                // reattempt the close
                                Thread.SpinWait(1);
                                continue;
                            }
                        }
                        else
                        {
                            Debug.Assert(error == NanomsgSymbols.EBADF);
                            // currently the only non-interrupt errors are for invalid sockets, which can't be closed
                            if (disposing)
                                throw new NanomsgException("nn_close " + socket.ToString(), error);
                            else
                                return;
                        }
                    }
                    else break;
                }
            }
        }


        internal unsafe void SendMessage(nn_msghdr* messageHeader)
        {
            int sentBytes = Interop.nn_sendmsg(_socket, messageHeader, (int)SendRecvFlags.NONE);
            if (sentBytes < 0)
                throw new NanomsgException("nn_send");
        }

        /// <summary>
        /// Sends the data, blocking until a send buffer can be acquired.
        /// </summary>
        /// <param name="buffer">The data to send</param>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the send was interrupted, or the send timeout has expired</exception>
        protected void SendImpl(byte[] buffer)
        {
            int sentBytes = Interop.nn_send(_socket, buffer, buffer.Length, (int)SendRecvFlags.NONE);
            if (sentBytes < 0)
                throw new NanomsgException("nn_send");
            else
                Debug.Assert(sentBytes == buffer.Length);
        }

        /// <summary>
        /// Sends the data.  If a send buffer cannot be immediately acquired, this method returns false and no send is performed.
        /// </summary>
        /// <param name="buffer">The data to send.</param>
        /// <returns>True if the data was sent, false if the data couldn't be sent at this time, and should be reattempted.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the send was interrupted, or the send timeout has expired</exception>
        protected bool SendImmediateImpl(byte[] buffer)
        {
            int sentBytes = Interop.nn_send(_socket, buffer, buffer.Length, (int)SendRecvFlags.DONTWAIT);
            if (sentBytes < 0)
            {
                int error = Interop.nn_errno();
                if (error == NanomsgSymbols.EAGAIN)
                    return false;
                else
                    throw new NanomsgException("nn_send", error);
            }
            else
            {
                Debug.Assert(sentBytes == buffer.Length);
                return true;
            }
        }

        protected NanomsgWriteStream CreateSendStreamImpl()
        {
            return new NanomsgWriteStream((NanomsgSocketBase)this);
        }

        protected void SendStreamImpl(NanomsgWriteStream stream)
        {
            SendStreamImpl(stream, SendRecvFlags.NONE);
        }

        protected int SendStreamImpl(NanomsgWriteStream stream, SendRecvFlags flags)
        {
            unsafe
            {
                int bufferCount = stream.PageCount;
                nn_iovec* iovec = stackalloc nn_iovec[bufferCount];
                nn_msghdr* hdr = stackalloc nn_msghdr[1];

                var buffer = stream.FirstPage();
                int i = 0;
                do
                {
                    iovec[i].iov_len = buffer.Length;
                    iovec[i].iov_base = (void*)buffer.Buffer;
                    buffer = stream.NextPage(buffer);
                } while (buffer.Buffer != IntPtr.Zero && i++ < bufferCount);

                (*hdr).msg_control = null;
                (*hdr).msg_controllen = 0;
                (*hdr).msg_iov = iovec;
                (*hdr).msg_iovlen = bufferCount;

                return Interop.nn_sendmsg(SocketID, hdr, (int)flags);
            }
        }

        protected bool SendStreamImmediateImpl(NanomsgWriteStream stream)
        {
            int sentBytes = SendStreamImpl(stream, SendRecvFlags.DONTWAIT);
            if (sentBytes < 0)
            {
                int error = Interop.nn_errno();
                if (error == NanomsgSymbols.EAGAIN)
                    return false;
                else
                    throw new NanomsgException("nn_send", error);
            }
            else
            {
                Debug.Assert(sentBytes == stream.Length);
                return true;
            }
        }

        /// <summary>
        /// Blocks until a message is received, and then returns a buffer containing its contents.  Note that this intermediate byte array can be avoided using the ReceiveStream method.
        /// </summary>
        /// <returns>A buffer containing the received message.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the receive was interrupted, or the receive timeout has expired</exception>
        protected byte[] ReceiveImpl()
        {
            return Receive(SendRecvFlags.NONE);
        }

        /// <summary>
        /// If a message is pending, returns a buffer containing its contents.  If no message is pending, returns null.  Note that this intermediate byte array can be avoided using the ReceiveStreamImmediate method.
        /// </summary>
        /// <returns>A buffer with message data, or null if no message has currently been received.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the receive was interrupted, or the receive timeout has expired</exception>
        protected byte[] ReceiveImmediateImpl()
        {
            return Receive(SendRecvFlags.DONTWAIT);
        }

        byte[] Receive(SendRecvFlags flags)
        {
            IntPtr buffer = IntPtr.Zero;
            int rc = Interop.nn_recv(_socket, ref buffer, Constants.NN_MSG, (int)flags);

            if (rc < 0 || buffer == null)
                return null;

            byte[] output = new byte[rc];
            try
            {
                Marshal.Copy(buffer, output, 0, rc);
            }
            finally
            {
                rc = Interop.nn_freemsg(buffer);
                if (rc != 0)
                    throw new NanomsgException("freemsg");
            }

            return output;
        }

        /// <summary>
        /// Blocks until a message is received, and then returns a stream containing its contents.
        /// </summary>
        /// <returns>The stream containing the message data.  This stream should be disposed in order to free the message resources.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the receive was interrupted, or the receive timeout has expired</exception>
        protected NanomsgReadStream ReceiveStreamImpl()
        {
            var stream = ReceiveStream(SendRecvFlags.NONE);
            if (stream == null)
                throw new NanomsgException("nn_recv");

            return stream;
        }

        /// <summary>
        /// If a message is pending, returns a stream containing its contents.  If no message is pending, returns null.
        /// </summary>
        /// <returns>A stream with message data, or null if no message has currently been received.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the receive was interrupted, or the receive timeout has expired</exception>
        protected NanomsgReadStream ReceiveStreamImmediateImpl()
        {
            var stream = ReceiveStream(SendRecvFlags.DONTWAIT);
            if (stream == null)
            {
                int error = Interop.nn_errno();
                if (error == NanomsgSymbols.EAGAIN)
                    return null;
                else
                    throw new NanomsgException("nn_recv");
            }
            else
                return stream;
        }

        NanomsgReadStream ReceiveStream(SendRecvFlags flags)
        {
            IntPtr buffer = IntPtr.Zero;
            int rc = Interop.nn_recv(_socket, ref buffer, Constants.NN_MSG, (int)flags);

            if (rc < 0 || buffer == null)
                return null;

            /*
             * In order to prevent managed allocations per receive, we attempt to recycle stream objects.  This
             * will work optimally if the stream is disposed before the next receive call, as in this case each
             * socket class will always reuse the same stream.
             * 
             * Disposing the stream will both release its nanomsg-allocated native buffer and return it to its
             * socket class for reuse.  
             */

            var stream = Interlocked.Exchange(ref _recycledReadStream, null);

            if (stream != null)
                stream.Reinitialize(buffer, rc);
            else
                stream = new NanomsgReadStream(buffer, rc,
                    _freeReadDisposer ?? (_freeReadDisposer = new NanomsgNativeDisposer() { Socket = (NanomsgSocketBase)this }));

            return stream;
        }

        void RecycleStream(NanomsgReadStream messageStream)
        {
            _recycledReadStream = messageStream;
        }

        class NanomsgNativeDisposer : INativeDisposer<NanomsgReadStream>
        {
            public NanomsgSocketBase Socket;

            public void DisposeOf(IntPtr nativeResource, NanomsgReadStream owner)
            {
                Interop.nn_freemsg(nativeResource);
                Socket.RecycleStream(owner);
            }
        }

        ~NanomsgSocketBase()
        {
            Dispose(false);
        }
    }
}
