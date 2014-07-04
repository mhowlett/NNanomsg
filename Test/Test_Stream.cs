using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NNanomsg;
using System.Diagnostics;

namespace Test
{
    unsafe class Test_Stream
    {
        static byte[] src;

        internal static void Execute()
        {
            src = new byte[TestConstants.DataSize];
            new Random().NextBytes(src);

            var bufferSizes = Enumerable.Range(8,7).Select(i => 1 << i);
            const int IterationCount = 1000;
            foreach (var bufferSize in bufferSizes)
            {
                Console.WriteLine("Buffers: " + string.Join(" ", bufferSize));

                Test("msgstream", () =>
                {
                    fixed (byte* srcptr = src)
                    {
                        byte[] buffer = new byte[bufferSize];
                        for (int i = 0; i < IterationCount; i++)
                            using (var stream = new NanomsgReadStream((IntPtr)srcptr, src.Length))
                            {
                                int read;
                                do
                                {
                                    read = stream.Read(buffer, 0, buffer.Length);
                                } while (read > 0);
                            }
                    }
                });

                /*Test("msgstream_bytes", () =>
                {
                    fixed (byte* srcptr = src)
                    //foreach (var bufferSize in bufferSizes)
                    {
                        for (int i = 0; i < IterationCount; i++)
                            using (var stream = new NanomsgReadStream((IntPtr)srcptr, src.Length, null))
                                while (stream.ReadByte() >= 0) { }
                    }
                });

                Test("msgstream_int32", () =>
                {
                    fixed (byte* srcptr = src)
                    //foreach (var bufferSize in bufferSizes)
                    {
                        for (int i = 0; i < IterationCount; i++)
                            using (var stream = new NanomsgReadStream((IntPtr)srcptr, src.Length, null))
                                while (stream.ReadInt32().HasValue) { }
                    }
                });


                Test("msgstream_int64", () =>
                {
                    fixed (byte* srcptr = src)
                    //foreach (var bufferSize in bufferSizes)
                    {
                        for (int i = 0; i < IterationCount; i++)
                            using (var stream = new NanomsgReadStream((IntPtr)srcptr, src.Length, null))
                                while (stream.ReadInt64().HasValue) { }
                    }
                });*/

                Test("writestream", () =>
                    {
                        for (int i = 0; i < IterationCount; i++)
                            using (var dest = new NanomsgWriteStream())
                                dest.Write(src, 0, src.Length);
                    });

            }
        }

        static void Test(string name, Action a)
        {
            a();
            var duration = Enumerable.Range(1, 3).Select(
                _ =>
                {
                    var sw = Stopwatch.StartNew();
                    a();
                    sw.Stop();
                    return sw.Elapsed;
                }).Min();
            Console.WriteLine("{0}: {1} ms", name, duration.TotalMilliseconds);
        }

    }
}
