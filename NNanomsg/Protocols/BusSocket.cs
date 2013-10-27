using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class BusSocket:NanomsgSocket
    {
        public BusSocket() : base(Domain.SP, Protocol.BUS) { }
    }
}
