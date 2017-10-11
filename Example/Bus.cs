using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NNanomsg.Protocols;
using System.Threading;

namespace Example
{
	public class Bus
	{
		public static void Execute(string[] args)
		{
			/* Usage: 
			* start example.exe bus ipc:///busexample.0 ipc:///busexample.1 ipc:///busexample.2
			* start example.exe bus ipc:///busexample.1 ipc:///busexample.2
			* start example.exe bus ipc:///busexample.2
			*/
			Console.WriteLine(String.Join(" ", args));
			using (var sock = new BusSocket())
			{
				sock.Bind(args[1]);
				for (int i = 2; i < args.Length; i++)
				{
					sock.Connect(args[i]);
				}
				while (true)
				{
					byte[] b = sock.ReceiveImmediate();
					if (b != null)
					{
						Console.WriteLine(Encoding.ASCII.GetString(b));
					}
					if (Console.KeyAvailable)
					{
						string output = args[1] + ":" + Console.ReadLine();
						sock.Send(Encoding.ASCII.GetBytes(output));
					}
				}
			}
		}
	}
}
