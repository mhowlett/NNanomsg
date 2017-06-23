using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using NNanomsg.Protocols;

namespace Example
{
	public class Survey
	{
		static void Surveyor(string url)
		{
			using (var s = new SurveyorSocket())
			{
				s.SurveyorOptions.Deadline = new TimeSpan(0, 0, 0, 1);
				s.Bind(url);
				while (true)
				{
					string message = "Services";
					byte[] buffer = Encoding.ASCII.GetBytes(message);
					s.Send(buffer);
					Console.Write("Starting Survey:");
					while (true)
					{
						byte[] response = s.Receive();
						if (response == null)
						{
							break;
						}
						message = Encoding.ASCII.GetString(response);
						Console.WriteLine(message);
					}
					Console.WriteLine("\nSurvey ended.\n");
					Thread.Sleep(1000);

				}
			}
		}

		static void Respondant(string url)
		{
			using (var s = new RespondentSocket())
			{
				s.Connect(url);
				while (true)
				{
					byte[] survey = s.Receive();
					if (survey != null)
					{
						string message = "Update ";
						byte[] response = Encoding.ASCII.GetBytes(message);
						try
						{
							s.Send(response);
							Console.WriteLine("Sent: " + message);
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
						}
					}
				}
			}
		}

		public static void Execute(string[] args)
		{
			if( args.Length != 3 )
			{
				printUsage();
				return;
			}
			switch (args[1].ToLower())
			{
				case "surveyor":
					Surveyor(args[2]);
					break;
				case "respondant":
					Respondant(args[2] );
					break;
				default:
					printUsage();
					break;
			}
		}

		private static void printUsage()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("Example.exe Survey surveyor tcp://127.0.0.1:5555");
			Console.WriteLine("Example.exe Survey respondant tcp://127.0.0.1:5555");
		}
	}
}
