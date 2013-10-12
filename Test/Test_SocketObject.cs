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
        const string InprocAddress = "inproc://127.0.0.1:6522";
        const int DataSize = 1024 * 100, BufferSize = 1024 * 4;

        public static void Execute()
        {
            _clientData = new byte[DataSize];
            _serverData = new byte[DataSize];
            var r = new Random();
            r.NextBytes(_clientData);
            r.NextBytes(_serverData);

            Console.WriteLine("Executing Socket object test");

            var clientThread = new Thread(
                () =>
                {
                    var req = new NanoMsgSocket(Domain.SP, Protocol.REQ);
                    req.Connect(InprocAddress);
                    byte[] streamOutput = new byte[BufferSize];
                    while (true)
                    {
                        var sw = Stopwatch.StartNew();
                        for (int i = 0; i < 10000; i++)
                        {
                            var result = req.SendImmediate(_clientData);
                            Trace.Assert(result);
                            int read = 0;
                            using (var stream = req.Receive())
                                while (stream.Length != stream.Position)
                                    read += stream.Read(streamOutput, 0, streamOutput.Length);
                            Trace.Assert(read == _serverData.Length);
                        }
                        sw.Stop();
                        var secondsPerSend = sw.Elapsed.TotalSeconds/ 10000d;
                        Console.WriteLine("Time {0} microsec, {1} per second, {2} mb/s ", 
                            (int)(secondsPerSend * 1000d * 1000d), 
                            (int)(1d / secondsPerSend),
                            (int)(DataSize * 2d / (1024d * 1024d * secondsPerSend)));
                    }

                });
            clientThread.Start();

            {
                var rep = new NanoMsgSocket(Domain.SP, Protocol.REP);
                rep.Bind(InprocAddress);

                byte[] streamOutput = new byte[BufferSize];

                var listener = new Listener();
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
                while (true)
                {
                    listener.Listen(TimeSpan.FromMinutes(30));
                }
            }
        }
    }
}
