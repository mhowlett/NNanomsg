using System.Diagnostics;
using NNanomsg;

namespace Test
{
    class Test_GetSetOptions
    {
        public static void Execute()
        {
            const string inprocAddress = "inproc://getsetoption_test";

            int v;
            byte[] bs = new byte[32];

            var s = NN.Socket(Domain.SP, Protocol.REP);
            NN.SetSocketOpt(s, SocketOptions.RCVTIMEO, 5000);
            NN.Bind(s, inprocAddress);
            NN.Recv(s, bs, SendRecvFlags.NONE);

            // setting the rcvtimeo works as expected.

            // note: currently get option isn't working.

        }
    }
}
