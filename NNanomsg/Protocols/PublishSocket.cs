using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class PublishSocket : NanomsgSocketBase, IBindSocket, ISendSocket
    {
        public PublishSocket() : base(Domain.SP, Protocol.PUB) { }

        #region Bind
        public NanomsgEndpoint Bind(string address)
        {
            return BindImpl(address);
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
    }
}
