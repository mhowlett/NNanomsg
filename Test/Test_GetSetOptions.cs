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
            byte[] bs;

            var s = NN.Socket(Domain.SP, Protocol.REP);
            var rc = NN.SetSocketOpt(s, SocketOptions.RCVTIMEO, 5000);
            Debug.Assert(rc >= 0);
            rc = NN.GetSockOpt(s, SocketOptions.RCVTIMEO, out v);
            Debug.Assert(rc >= 0);

            NN.Bind(s, inprocAddress);
            NN.Recv(s, out bs, SendRecvFlags.NONE);

            // setting the rcvtimeo works as expected.
        }
    }
}
