using System;
using System.Diagnostics;
using System.Threading;
using NNanomsg;
using System.Runtime.InteropServices;

namespace Test
{
    class Test_SocketObject
    {
        static byte[] _clientData, _serverData;
        const string InprocAddress = "inproc://127.0.0.1:6520";
        const int DataSize = TestConstants.DataSize, BufferSize = 1024 * 4, Iter = TestConstants.Iterations;

        public static void Execute()
        {
            _clientData = new byte[DataSize];
            _serverData = new byte[DataSize];
            var r = new Random();
            r.NextBytes(_clientData);
            r.NextBytes(_serverData);

            Console.WriteLine("Executing Socket object test " + NanomsgSymbols.NN_VERSION_CURRENT.ToString());

            var clientThread = new Thread(
                () =>
                {
                    var req = new NanomsgSocket(Domain.SP, Protocol.REQ);
                    req.Connect(InprocAddress);
                    //req.Options.TcpNoDelay = true;

                    /*unsafe
                    {
                        byte* s1 = (byte*)Interop.nn_allocmsg(4, 0), s2 = (byte*) Interop.nn_allocmsg(4,0);
                        *(uint*)s1 = 0x01020304;
                        *(uint*)s2 = 0x05060708;
                        //byte[] scatter1 = new byte[] { 1, 2, 3, 4 }, scatter2 = new byte[] { 5, 6, 7, 8 };
                        //fixed (byte* s1 = scatter1, s2 = scatter2)
                        {
                            nn_iovec* iovecs = stackalloc nn_iovec[2];
                            *iovecs = new nn_iovec() { iov_base = s1, iov_len = 4 };
                            *(iovecs + 1) = new nn_iovec() { iov_base = s2, iov_len = 4 };
                            nn_msghdr* msghdr = stackalloc nn_msghdr[1];
                            *msghdr = new nn_msghdr()
                            {
                                msg_control = null,
                                msg_controllen = 0,
                                msg_iov = iovecs,
                                msg_iovlen = 2
                            };

                            req.SendMessage(msghdr);
                        }
                        Interop.nn_freemsg((IntPtr)s1);
                        Interop.nn_freemsg((IntPtr)s2);
                    }*/

                    byte[] streamOutput = new byte[BufferSize];
                    while (true)
                    {
                        var sw = Stopwatch.StartNew();
                        for (int i = 0; i < Iter; i++)
                        {
                            var result = req.SendImmediate(_clientData);
                            Trace.Assert(result);
                            int read = 0;
                            using (var stream = req.ReceiveStream())
                                while (stream.Length != stream.Position)
                                    read += stream.Read(streamOutput, 0, streamOutput.Length);
                            Trace.Assert(read == _serverData.Length);
                        }
                        sw.Stop();
                        var secondsPerSend = sw.Elapsed.TotalSeconds / (double)Iter;
                        Console.WriteLine("Time {0} us, {1} per second, {2} mb/s ",
                            (int)(secondsPerSend * 1000d * 1000d),
                            (int)(1d / secondsPerSend),
                            (int)(DataSize * 2d / (1024d * 1024d * secondsPerSend)));
                    }
                });
            clientThread.Start();

            {
                var rep = new NanomsgSocket(Domain.SP, Protocol.REP);
                rep.Bind(InprocAddress);
                //rep.Options.TcpNoDelay = true;

                byte[] streamOutput = new byte[BufferSize];

                var sw = Stopwatch.StartNew();
                while (sw.Elapsed.TotalSeconds < 10)
                {
                    int read = 0;
                    using (var stream = rep.ReceiveStream())
                        while (stream.Length != stream.Position)
                            read += stream.Read(streamOutput, 0, streamOutput.Length);
                    rep.SendImmediate(_serverData);
                }

                /*var listener = new NanomsgListener();
                listener.AddSocket(rep.SocketID);
                listener.ReceivedMessage += delegate(int s)
                {
                    var stream = rep.ReceiveImmediate();
                    if (stream == null) { Trace.Fail("receive immediate failed"); return; }

                    int read = 0;
                    while (stream.Length != stream.Position)
                        read += stream.Read(streamOutput, 0, streamOutput.Length);
                    stream.Dispose();
                    Trace.Assert(read == _clientData.Length);
                    rep.SendImmediate(_serverData);
                };

                var sw = Stopwatch.StartNew();
                while (sw.Elapsed.TotalSeconds < 10)
                    listener.Listen(TimeSpan.FromSeconds(5));*/
                clientThread.Abort();
            }
        }
    }
}
