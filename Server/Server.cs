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
				ConnectedClient newClient = new ConnectedClient(clientSocket);

				if (connectedClients.Count < 3)
				{
					connectedClients.Add(newClient);
					PrintToConsoleAsLogMessage(connectedClients.Last().GetNickname() + " Joined [" + clientSocket.RemoteEndPoint + "]");
					BroadcastDataToAllClients("[Server] " + connectedClients.Last().GetNickname() + " has joined the chat!");
					//UpdateClientsOnlineBox();
					threads.Add(new Thread(() => { ClientMethod(connectedClients.Last()); }));
					threads.Last().Start();
				}
				else
				{
					SendDataToSpecificClient(newClient, "/server.full ");
					Thread test = new Thread(() => { ClientMethod(newClient); });
				}
			}

		}

		public void Stop()
		{
			tcpListener.Stop();
		}

		private void ClientMethod(ConnectedClient connectedClient)
		{
			Packets.Packet recievedDataFromClientSocket;

			while((recievedDataFromClientSocket = connectedClient.Read()) != null)
			{
				ProcessDataSentFromClient(recievedDataFromClientSocket, connectedClient);
			}

			connectedClient.CloseConnection();
		}

		private void ProcessDataSentFromClient(Packets.Packet data, ConnectedClient client)
		{

			switch(data.m_PacketType)
			{
				case Packets.Packet.PacketType.Nickname:
					Packets.NicknamePacket nicknamePacket = data as Packets.NicknamePacket;
					client.SetNickname(nicknamePacket.Name);
					break;

				case Packets.Packet.PacketType.ChatMessage:
					Packets.ChatMessagePacket messagePacket = data as Packets.ChatMessagePacket;
					BroadcastDataToAllClients("[" + client.GetNickname() + "] " + messagePacket.Message);
					break;
			}
		}

		private void BroadcastDataToAllClients(string data)
		{
			foreach(ConnectedClient client in connectedClients)
			{
				client.Send(new Packets.ChatMessagePacket(data));
			}
		}

		private void SendDataToSpecificClient(ConnectedClient client, string data)
		{
			//client.GetStreamWriter().WriteLine(data);
			//client.GetStreamWriter().Flush();
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
