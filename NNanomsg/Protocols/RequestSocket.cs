using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace NNanomsg.Protocols
{
    public class RequestSocket : NanomsgSocketBase, IConnectSocket, ISendSocket, IReceiveSocket
    {
        public RequestSocketOptions RequestOptions { get; private set; }

        public RequestSocket()
            : base(Domain.SP, Protocol.REQ)
        {
            if (SocketID >= 0)
                RequestOptions = new RequestSocketOptions(SocketID);
        }

        #region Connect
        public NanomsgEndpoint Connect(string address)
        {
            return ConnectImpl(address);
        }

        public NanomsgEndpoint Connect(IPAddress address, int port)
        {
            return ConnectImpl(address, port);
        }
        #endregion

        #region Send
        public void Send(byte[] buffer)
        {
            SendImpl(buffer);
        }

        public bool SendImmediate(byte[] buffer)
        {
            return SendImmediateImpl(buffer);
        }

        public NanomsgWriteStream CreateSendStream()
        {
            return CreateSendStreamImpl();
        }

        public void SendStream(NanomsgWriteStream stream)
        {
            SendStreamImpl(stream);
        }

        public bool SendStreamImmediate(NanomsgWriteStream stream)
        {
            return SendStreamImmediateImpl(stream);
        }
        #endregion

        #region Receive
        public byte[] Receive()
        {
            return ReceiveImpl();
        }

        public byte[] ReceiveImmediate()
        {
            return ReceiveImmediateImpl();
        }

        public NanomsgReadStream ReceiveStream()
        {
            return ReceiveStreamImpl();
        }

        public NanomsgReadStream ReceiveStreamImmediate()
        {
            return ReceiveStreamImmediateImpl();
        }
        #endregion
    }

    public class RequestSocketOptions
    {
        int _socket;

        public RequestSocketOptions(int socket)
        {
            _socket = socket;
        }

        /// <summary>
        /// This option is defined on the full REQ socket. If reply is not received in specified amount of milliseconds, the request will be automatically resent. The type of this option is int. Default value is 60000 (1 minute). 
        /// </summary>
        public TimeSpan ResendInterval
        {
            get
            {
                return NanomsgSocketOptions.GetTimespan(_socket, SocketOptionLevel.Request, SocketOption.REQ_RESEND_IVL).Value;
            }
            set
            {
                NanomsgSocketOptions.SetTimespan(_socket, SocketOptionLevel.Request, SocketOption.REQ_RESEND_IVL, value);
            }
        }
    }
}
