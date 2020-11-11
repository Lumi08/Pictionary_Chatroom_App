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
			try
			{
				tcpListener.Start();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				PrintToConsoleAsLogMessage("Error Making Server, you may already have a server running on your machine!");
				Console.ReadLine();
				return;
			}

			PrintToConsoleAsLogMessage("Server Awaiting Client");
			
			while (true)
			{
				Socket clientSocket = tcpListener.AcceptSocket();
				connectedClients.Add(new ConnectedClient(clientSocket));
				PrintToConsoleAsLogMessage(connectedClients.Last().GetNickname() + " Joined [" + clientSocket.RemoteEndPoint + "]");
				BroadcastDataToAllClients("/server.message [Server] " + connectedClients.Last().GetNickname() + " has joined the chat!");
				UpdateClientsOnlineBox();
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
				UpdateClientsOnlineBox();
				return;
			}

			string[] dataArray = data.Split(' ');
			string clientProcess = dataArray[0];
			string clientMessage = data.Substring(clientProcess.Length + 1);
			PrintToConsoleAsLogMessage(client.GetNickname() + ": " + clientMessage);

			if (clientProcess == "/client.message")
			{
				BroadcastDataToAllClients("/server.message [" + client.GetNickname() + "] " + clientMessage);
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

		private void UpdateClientsOnlineBox()
		{
			string clients = "/server.clientlist";

			foreach(ConnectedClient onlineClient in connectedClients)
			{
				clients += " " + onlineClient.GetNickname();
			}

			BroadcastDataToAllClients(clients);
		}

		public void PrintToConsoleAsLogMessage(string x)
		{
			DateTime now = DateTime.Now;

			Console.WriteLine("[" + now.Day + "/" + now.Month + "/" + now.Year + " " + now.Hour + ":" + now.Minute + ":" + now.Second + "] " + x);
		}
	}
}
