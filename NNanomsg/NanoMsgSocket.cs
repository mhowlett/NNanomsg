using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace NNanomsg
{
    public struct NanoMsgEndpoint
    {
        public int ID;
    }

    public class NanoMsgSocket : IDisposable
    {
        const int NullSocket = -1;

        public Domain Domain { get; private set; }
        public Protocol Protocol { get; private set; }
        public NanoMsgSocketOptions Options { get; private set; }
        public int SocketID { get { return _socket; } }

        int _socket = NullSocket;
        INativeDisposer<NNMessageStream> _freeMessageDisposer;
        NNMessageStream _recycledStream;

        /// <summary>
        /// Initialize a new nanomsg socket for the given domain and protocol
        /// </summary>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if a socket can't be created for this domain and protocol</exception>
        public NanoMsgSocket(Domain domain, Protocol protocol)
        {
            Domain = domain;
            Protocol = protocol;

            _socket = Interop.nn_socket((int)Domain, (int)Protocol);
            if (_socket >= 0)
                Options = new NanoMsgSocketOptions(_socket);
            else
                throw new NanomsgException(string.Format("nn_socket {0} {1}", domain, protocol));
        }

        /// <summary>
        /// Connects the socket to the remote address.  This can be called multiple times per socket.
        /// </summary>
        /// <param name="address">The addr argument consists of two parts as follows: transport://address. The transport specifies the underlying transport protocol to use. The meaning of the address part is specific to the underlying transport protocol.</param>
        /// <returns>An endpoint identifier which can be used to reference the connected endpoint in the future</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the address is invalid</exception>
        public NanoMsgEndpoint Connect(string address)
        {
            int endpoint = -1;
            endpoint = Interop.nn_connect(_socket, address);

            if (endpoint > 0)
                return new NanoMsgEndpoint() { ID = endpoint };
            else
                throw new NanomsgException("nn_connect " + address);

        }

        /// <summary>
        /// Binds the socket to the local address.  This can be called multiple times per socket.
        /// </summary>
        /// <param name="address">The addr argument consists of two parts as follows: transport://address. The transport specifies the underlying transport protocol to use. The meaning of the address part is specific to the underlying transport protocol.</param>
        /// <returns>An endpoint identifier which can be used to reference the bound endpoint in the future</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the address is invalid</exception>
        public NanoMsgEndpoint Bind(string address)
        {
            int endpoint = -1;
            endpoint = Interop.nn_bind(_socket, address);

            if (endpoint > 0)
                return new NanoMsgEndpoint() { ID = endpoint };
            else
                throw new NanomsgException("nn_bind " + address);
        }

        /// <summary>
        /// Shuts down a specific endpoint of this socket.
        /// </summary>
        /// <param name="endpoint">The endpoint created by Connect or Bind which is being shut down.</param>
        /// <returns>True if the endpoint was shut down, false if the shutdown attempt was interrupted and should be reattempted.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state or the endpoint's shutdown attempt was interrupted and should be reattempted.</exception>
        public bool Shutdown(NanoMsgEndpoint endpoint)
        {
            const int ValidShutdownResult = 0, MaxShutdownAttemptCount = 5;
            int attemptCount = 0;
            while (true)
            {
                if (Interop.nn_shutdown(_socket, endpoint.ID) != ValidShutdownResult)
                {
                    int error = Interop.nn_errno();

                    // if we were interrupted by a signal, reattempt is allowed by the native library
                    if (error == NanoMsgSymbols.EINTR)
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
                        if (error == NanoMsgSymbols.EINTR)
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
                            Debug.Assert(error == NanoMsgSymbols.EBADF);
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

        /// <summary>
        /// Sends the data, blocking until a send buffer can be acquired.
        /// </summary>
        /// <param name="buffer">The data to send</param>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the send was interrupted, or the send timeout has expired</exception>
        public void Send(byte[] buffer)
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
        public bool SendImmediate(byte[] buffer)
        {
            int sentBytes = Interop.nn_send(_socket, buffer, buffer.Length, (int)SendRecvFlags.DONTWAIT);
            if (sentBytes < 0)
            {
                int error = Interop.nn_errno();
                if (error == NanoMsgSymbols.EAGAIN)
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

        /// <summary>
        /// Blocks until a message is received, and then returns a stream containing its contents.
        /// </summary>
        /// <returns>The stream containing the message data.  This stream should be disposed in order to free the message resources.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the receive was interrupted, or the receive timeout has expired</exception>
        public System.IO.Stream Receive()
        {
            var stream =  Receive(SendRecvFlags.NONE);
            if (stream == null)
                throw new NanomsgException("nn_recv");

            return stream;
        }

        /// <summary>
        /// If a message is pending, returns a stream containing its contents.  If no message is pending, returns null.
        /// </summary>
        /// <returns>A stream with message data, or null if no message has currently been received.</returns>
        /// <exception cref="NNanomsg.NanomsgException">Thrown if the socket is in an invalid state, the receive was interrupted, or the receive timeout has expired</exception>
        public System.IO.Stream ReceiveImmediate()
        {
            var stream = Receive(SendRecvFlags.DONTWAIT);
            if (stream == null)
            {
                int error = Interop.nn_errno();
                if (error == NanoMsgSymbols.EAGAIN)
                    return null;
                else 
                    throw new NanomsgException("nn_recv");
            }
            else
                return stream;
        }

        Stream Receive(SendRecvFlags flags)
        {
            IntPtr buffer = IntPtr.Zero;
            int rc = Interop.nn_recv(_socket, ref buffer, Constants.NN_MSG, (int)flags);

            if (rc <= 0 || buffer == null)
                return null;

            /*
             * In order to prevent managed allocations per receive, we attempt to recycle stream objects.  This
             * will work optimally if the stream is disposed before the next receive call, as in this case each
             * socket class will always reuse the same stream.
             * 
             * Disposing the stream will both release its nanomsg-allocated native buffer and return it to its
             * socket class for reuse.  
             */

            var stream = Interlocked.Exchange(ref _recycledStream, null);

            if (stream != null)
                stream.Reinitialize(buffer, rc);
            else
                stream = new NNMessageStream(buffer, rc,
                    _freeMessageDisposer ?? (_freeMessageDisposer = new NanoMsgNativeDisposer() { Socket = this }));

            return stream;
        }

        void RecycleStream(NNMessageStream messageStream)
        {
            _recycledStream = messageStream;
        }

        class NanoMsgNativeDisposer : INativeDisposer<NNMessageStream>
        {
            public NanoMsgSocket Socket;

            public void DisposeOf(IntPtr nativeResource, NNMessageStream owner)
            {
                Interop.nn_freemsg(nativeResource);
                Socket.RecycleStream(owner);
            }
        }

        ~NanoMsgSocket()
        {
            Dispose(false);
        }
    }
}
