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

        public NanoMsgSocket(Domain domain, Protocol protocol)
        {
            Domain = domain;
            Protocol = protocol;
        }

        public bool Init()
        {
            const int InitializingSocket = -2;
            
            var currentSocket = Interlocked.CompareExchange(ref _socket, InitializingSocket, NullSocket);

            if (currentSocket == NullSocket)
            {
                _socket = Interop.nn_socket((int)Domain, (int)Protocol);
                if (_socket >= 0)
                    Options = new NanoMsgSocketOptions(_socket);
            }

            return _socket >= 0;
        }

        public NanoMsgEndpoint? Connect(string address)
        {
            int endpoint = -1;
            if (Init())
                endpoint = Interop.nn_connect(_socket, address);

            if (endpoint > 0)
                return new NanoMsgEndpoint() { ID = endpoint };
            return null;
        }

        public NanoMsgEndpoint? Bind(string address)
        {
            int endpoint = -1;
            if (Init())
                endpoint = Interop.nn_bind(_socket, address);

            if (endpoint > 0)
                return new NanoMsgEndpoint() { ID = endpoint };
            return null;
        }

        public bool Shutdown(NanoMsgEndpoint endpoint)
        {
            if (_socket == NullSocket)
                return false;

            return Interop.nn_shutdown(_socket, endpoint.ID) == 0;
        }

        public void Dispose()
        {
            var socket = Interlocked.Exchange(ref _socket, NullSocket);
            if (socket != NullSocket)
                Interop.nn_close(socket);

            GC.SuppressFinalize(this);
        }

        public bool Send(byte[] buffer)
        {
            return Interop.nn_send(_socket, buffer, buffer.Length, (int)SendRecvFlags.NONE) >= 0;
        }

        public bool SendImmediate(byte[] buffer)
        {
            return Interop.nn_send(_socket, buffer, buffer.Length, (int)SendRecvFlags.DONTWAIT) >= 0;
        }

        public System.IO.Stream Receive()
        {
            return Receive(SendRecvFlags.NONE);
        }
        public System.IO.Stream ReceiveImmediate()
        {
            return Receive(SendRecvFlags.DONTWAIT);
        }

        Stream Receive(SendRecvFlags flags)
        {
            IntPtr buffer = IntPtr.Zero;
            int rc = Interop.nn_recv(_socket, ref buffer, Constants.NN_MSG, (int)flags);
            if (rc <= 0 || buffer == null)
                return null;

            var stream = Interlocked.Exchange(ref _recycledStream, null);

            if (stream != null)
                stream.Reinitialize(buffer, rc);
            else
                stream = new NNMessageStream(buffer, rc, 
                    _freeMessageDisposer ?? (_freeMessageDisposer = new NanoMsgNativeDisposer(){Socket = this}));

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
            Dispose();
        }
    }
}
