using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class PublishSocket:NanomsgSocket
    {
        public PublishSocket(): base(Domain.SP, Protocol.PUB){}
    }
}
