using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class PairSocket:NanomsgSocket
    {
        public PairSocket() : base(Domain.SP, Protocol.PAIR) { }
    }
}
