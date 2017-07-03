using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NNanomsg.Protocols;
using System.Threading;

namespace Example
{
	public class PubSub
	{
		public static void Execute(string[] args) {
			string[] values = args;
			if(values.Length != 3 )
			{
				throw new ArgumentException("Invalid number parameters.");
			}
			switch (values[1].Trim().ToLower() )
			{
				case "publisher":
					using (var s = new PublishSocket())
					{
						s.Bind(values[2]);
						int i = 0;
						while(true)
						{
							byte[] b = Encoding.ASCII.GetBytes("Publish Counter is " + ++i ) ;
							s.Send(b);							
							Console.Write(".");
							Thread.Sleep(2000);
						}
					}
					break;
				case "subscriber":
					using (var s = new SubscribeSocket())
					{						
						//Needs to match the first portion of the message being received.
						s.Subscribe("Publish Counter");
						s.Connect(values[2]);
						while(true)
						{
							byte[] b = s.Receive();
							if( b != null )
							{
								Console.WriteLine("Received: " + Encoding.ASCII.GetString(b));
							} else
							{
								Console.WriteLine("x");
							}
						}
					}
					break;
			}
		}
	}
}
