using System;
using System.Diagnostics;
using System.Threading;
using NNanomsg;
using NNanomsg.Protocols;

namespace Test
{
    class Test_PushPull
    {
        
            static byte[] _clientData, _serverData;
            const string InprocAddress = "inproc://pushpull_test", InprocAddressReverse = "inproc://pushpull_test_reverse";
        const int DataSize = TestConstants.DataSize, BufferSize = 1024 * 4, Iter = TestConstants.Iterations;

        public static void Execute()
        {
            Console.WriteLine("Executing PushPull test");


            _clientData = new byte[DataSize];
            _serverData = new byte[DataSize];
            var r = new Random();
            r.NextBytes(_clientData);
            r.NextBytes(_serverData);

        
            var clientThread = new Thread(
                () =>
                {
                    var req = new PushSocket();
                    req.Connect(InprocAddress);

                    var revreq = new PullSocket();
                    revreq.Bind(InprocAddressReverse);
                    
                    byte[] streamOutput = new byte[BufferSize];
                    while (true)
                    {
                        var sw = Stopwatch.StartNew();
                        for (int i = 0; i < Iter; i++)
                        {
                            var result = req.SendImmediate(_clientData);
                            Trace.Assert(result);
                            int read = 0;
                            using (var stream = revreq.ReceiveStream())
                                while (stream.Length != stream.Position)
                                    read += stream.Read(streamOutput, 0, streamOutput.Length);
                            Trace.Assert(read == _serverData.Length);
                        }
                        sw.Stop();
                        var secondsPerSend = sw.Elapsed.TotalSeconds / (double)Iter;
                        Console.WriteLine("PushPull Time {0} us, {1} per second, {2} mb/s ",
                            (int)(secondsPerSend * 1000d * 1000d),
                            (int)(1d / secondsPerSend),
                            (int)(DataSize * 2d / (1024d * 1024d * secondsPerSend)));
                    }
                });
            clientThread.Start();

            {
                var rep = new PullSocket();
                rep.Bind(InprocAddress);

                var revrep = new PushSocket();
                revrep.Connect(InprocAddressReverse);

                byte[] streamOutput = new byte[BufferSize];

                var sw = Stopwatch.StartNew();
                while (sw.Elapsed.TotalSeconds < 10)
                {
                    int read = 0;
                    using (var stream = rep.ReceiveStream())
                        while (stream.Length != stream.Position)
                            read += stream.Read(streamOutput, 0, streamOutput.Length);
                    revrep.SendImmediate(_serverData);
                }

                clientThread.Abort();
            }
        
        }
    }
}
