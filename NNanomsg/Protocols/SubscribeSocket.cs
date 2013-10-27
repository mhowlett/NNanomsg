using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class SubscribeSocket:NanomsgSocket
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
    }
}
