using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class RespondentSocket:NanomsgSocket
    {
        public RespondentSocket() : base(Domain.SP, Protocol.RESPONDENT) { }
    }
}
