using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Server
{
	class Server
	{
		//Locals
		private TcpListener tcpListener;
		private List<Thread> threads = new List<Thread>();
		private List<ConnectedClient> connectedClients = new List<ConnectedClient>();

		public Server(string ipAddress, int port)
		{
			tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
		}

		public void Start()
		{
			tcpListener.Start();

			PrintToConsoleAsLogMessage("Server Awaiting Client");
			
			while (true)
			{
				Socket clientSocket = tcpListener.AcceptSocket();
				connectedClients.Add(new ConnectedClient(clientSocket));
				PrintToConsoleAsLogMessage(connectedClients.Last().GetNickname() + " Joined the server!");
				threads.Add(new Thread(() => { ClientMethod(connectedClients.Last()); }));
				threads.Last().Start();
			}

		}

		public void Stop()
		{
			tcpListener.Stop();
		}

		private void ClientMethod(ConnectedClient connectedClient)
		{
			string recievedDataFromClientSocket;

			while((recievedDataFromClientSocket = connectedClient.GetStreamReader().ReadLine()) != null)
			{
				ProcessDataSentFromClient(recievedDataFromClientSocket, connectedClient);
			}

			connectedClient.CloseConnection();
		}

		private void ProcessDataSentFromClient(string data, ConnectedClient client)
		{
			if(data.StartsWith("/client.disconnect"))
			{
				threads.RemoveAt(connectedClients.IndexOf(client));
				connectedClients.Remove(client);
				PrintToConsoleAsLogMessage(client.GetNickname() + " Left The Server. [" + client.GetSocket().RemoteEndPoint + "]");
				BroadcastDataToAllClients("/server.message [Server] " + client.GetNickname() + " has left the chat!");
				return;
			}

			if(data == "hi")
			{
				BroadcastDataToAllClients("hey");
			}
		}

		private void BroadcastDataToAllClients(string data)
		{
			foreach(ConnectedClient client in connectedClients)
			{
				client.GetStreamWriter().WriteLine(data);
				client.GetStreamWriter().Flush();
			}
		}

		public void PrintToConsoleAsLogMessage(string x)
		{
			DateTime now = DateTime.Now;

			Console.WriteLine("[" + now.Day + "/" + now.Month + "/" + now.Year + " " + now.Hour + ":" + now.Minute + ":" + now.Second + "] " + x);
		}
	}
}
