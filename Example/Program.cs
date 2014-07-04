using System;
using System.IO;
using System.Linq;
using NNanomsg;
using NNanomsg.Protocols;

namespace Example
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("Usage: Example.exe <ReqRep|Pair|Listener|Device> [params]");
            Console.WriteLine();
            Console.WriteLine("-- Device Example --");
            Console.WriteLine("The Device Example runs multiple REP workers and multiple REQ workers");
            Console.WriteLine("in parallel. By default, it responds with \"Hello World\",");
            Console.WriteLine("but you can specify names by one ore more [params].");
        }

        /// <summary>
        ///     
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            switch (args[0].ToLower())
            {
                case "reqrep": ReqRep.Execute(args);
                    break;
                case "pair": Pair.Execute(args);
                    break;
                case "listener": Listener.Execute(args);
                    break;
                case "device": Device.Execute(args.Skip(1).ToArray());
                    break;
                default:
                    PrintUsage();
                    break;
            }

        }
    }
}
