using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace NNanomsg.Protocols
{
    public class SubscribeSocket : NanomsgSocketBase, IConnectSocket, IReceiveSocket
    {
        public SubscribeSocket() : base(Domain.SP, Protocol.SUB) { }

        public void Subscribe(string topic)
        {
            NanomsgSocketOptions.SetString(SocketID, SocketOptionLevel.Subscribe, SocketOption.SUB_SUBSCRIBE, topic);
        }

        public void Unsubscribe(string topic)
        {
            NanomsgSocketOptions.SetString(SocketID, SocketOptionLevel.Subscribe, SocketOption.SUB_SUBSCRIBE, topic);
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
}
