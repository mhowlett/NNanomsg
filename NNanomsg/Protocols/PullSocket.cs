using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class PullSocket:NanomsgSocket
    {
        public PullSocket() : base(Domain.SP, Protocol.PULL) { }
    }
}
