using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class ReplySocket : NanomsgSocket
    {
        public ReplySocket()
            : base(Domain.SP, Protocol.REP)
        {
        }
    }
}
