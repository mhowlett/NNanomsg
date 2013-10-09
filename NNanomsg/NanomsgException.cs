using System;

namespace NNanomsg
{
    public class NanomsgException : Exception
    {
        public NanomsgException()
            : base(NN.StrError(NN.Errno()))
        {
        }
    }
}
