using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			int port = 4444;
			string localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
			int maxClients;
			int pictionaryLobbySize;

			Console.WriteLine("Please enter the max number of players to connect to the server");
			Console.WriteLine("(Keep in mind each client connected is an extra server thread)");
			Console.Write("Max Amount of Clients: ");
			while(!int.TryParse(Console.ReadLine(), out maxClients))
			{
				Console.WriteLine("Please enter a valid Int");
				Console.Write("Max Amount of Clients: ");
			}

			Console.WriteLine("Please enter the Pictionary Lobby size");
			Console.WriteLine("(This is the amount of people needed to start the pictionary Game)");
			Console.Write("Pictionary Lobby Size: ");
			while (!int.TryParse(Console.ReadLine(), out pictionaryLobbySize))
			{
				Console.WriteLine("Please enter a valid Int");
				Console.Write("Pictionary Lobby Size: ");
			}

			Server server = new Server(localIP, port, maxClients, pictionaryLobbySize);
			server.PrintToConsoleAsLogMessage("Server Started on " + localIP + ":" + port);
			server.PrintToConsoleAsLogMessage("Server Only Accepting " + maxClients + " Clients Connected to the Server at Once");
			server.Start();
			server.Stop();
		}
	}
}
