using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NNanomsg.Protocols;
using NNanomsg;

namespace Example
{
	public class Bus
	{
		public static void Execute(string[] args)
		{
			/* Usage: 
			* start example.exe bus ipc:///bus0 ipc:///bus1 ipc:///bus2 ipc:///bus3
			* start example.exe bus ipc:///bus1 ipc:///bus0 ipc:///bus2 ipc:///bus3
			* start example.exe bus ipc:///bus2 ipc:///bus0 ipc:///bus1 ipc:///bus3
			* start example.exe bus ipc:///bus3 ipc:///bus0 ipc:///bus1 ipc:///bus2
			*/
			Console.WriteLine(String.Join(" ", args));
			using (var send = new BusSocket())
			using (var receive = new BusSocket())
			{
				send.Bind(args[1]);
				for (int i = 2; i < args.Length; i++)
				{
					receive.Connect(args[i]);
				}
				while (true)
				{
					byte[] b = receive.ReceiveImmediate();
					if (b != null)
					{
						Console.WriteLine(Encoding.ASCII.GetString(b));
					}
					if (Console.KeyAvailable)
					{
						string output = args[1] + ":" + Console.ReadLine();
						send.Send(Encoding.ASCII.GetBytes(output));
					}
				}
			}
		}
	}
}
