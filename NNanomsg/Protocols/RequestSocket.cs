using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class RequestSocket : NanomsgSocket
    {
        public RequestSocketOptions RequestOptions { get; private set; }

        public RequestSocket()
            : base(Domain.SP, Protocol.REQ)
        {
            if (SocketID >= 0)
                RequestOptions = new RequestSocketOptions(SocketID);
        }
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
