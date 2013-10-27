using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class PushSocket:NanomsgSocket
    {
        public PushSocket() : base(Domain.SP, Protocol.PUSH) { }
    }
}
