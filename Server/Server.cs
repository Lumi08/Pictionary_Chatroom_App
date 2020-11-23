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

			PrintToConsoleAsLogMessage("Server Open to Client Connections");
			
			while (true)
			{
				
				Socket clientSocket = tcpListener.AcceptSocket();
				ConnectedClient newClient = new ConnectedClient(clientSocket);

				if (connectedClients.Count < 3)
				{
					connectedClients.Add(newClient);
					PrintToConsoleAsLogMessage(connectedClients.Last().GetNickname() + " Joined [" + clientSocket.RemoteEndPoint + "]");
					BroadcastDataToAllClients(new Packets.ChatMessagePacket("[Server] " + connectedClients.Last().GetNickname() + " has joined the chat!"));
					//UpdateClientsOnlineBox();
					threads.Add(new Thread(() => { ClientMethod(connectedClients.Last()); }));
					threads.Last().Start();
				}
				else
				{
					SendDataToSpecificClient(newClient, new Packets.DisconnectPacket());
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
			switch (data.m_PacketType)
			{
				case Packets.Packet.PacketType.Nickname:
					Packets.NicknamePacket nicknamePacket = data as Packets.NicknamePacket;
					if(nicknamePacket.Name == client.GetNickname())
					{
						SendDataToSpecificClient(client, new Packets.ChatMessagePacket("[Error] You Can't Change Your Name to the Same Name"));
						return;
					}
					
					PrintToConsoleAsLogMessage("[" + nicknamePacket.m_PacketType + "] from: " + client.GetNickname() + " data: " + nicknamePacket.Name);
					PrintToConsoleAsLogMessage("[" + client.GetNickname() + "] Changed Name to " + nicknamePacket.Name);
					BroadcastDataToAllClients(new Packets.ChatMessagePacket("[Server] " + client.GetNickname() + " Changed Name to " + nicknamePacket.Name));
					client.SetNickname(nicknamePacket.Name);
					break;

				case Packets.Packet.PacketType.ChatMessage:
					Packets.ChatMessagePacket messagePacket = data as Packets.ChatMessagePacket;
					BroadcastDataToAllClients(new Packets.ChatMessagePacket("[" + client.GetNickname() + "] " + messagePacket.Message));
					PrintToConsoleAsLogMessage("[" + messagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Message: " + messagePacket.Message);
					break;

				case Packets.Packet.PacketType.PrivateMessage:
					Packets.PrivateMessagePacket privateMessagePacket = data as Packets.PrivateMessagePacket;

					foreach(ConnectedClient target in connectedClients)
					{
						if(target.GetNickname().ToLower() == privateMessagePacket.TargetUser.ToLower())
						{
							if(target == client)
							{
								PrintToConsoleAsLogMessage("[Error] [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Tried to send a Private Message to Themself");
								SendDataToSpecificClient(client, new Packets.ChatMessagePacket("[Error] You Can't Message Yourself"));
								return;
							}

							PrintToConsoleAsLogMessage("[" + privateMessagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] " +
								"To [" + target.GetNickname() + " " + target.GetSocket().RemoteEndPoint + "] Message: " + privateMessagePacket.Message);

							string message = privateMessagePacket.Message;
							privateMessagePacket.Message = "[To " + target.GetNickname() + "] " + message;
							SendDataToSpecificClient(client, privateMessagePacket);
							privateMessagePacket.Message = "[From " + client.GetNickname() + "] " + message;
							SendDataToSpecificClient(target, privateMessagePacket);
							return;
						}
					}

					PrintToConsoleAsLogMessage("[Error] [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Tried to send a Private Message to a client that does not exist");
					SendDataToSpecificClient(client, new Packets.ChatMessagePacket("[Error] User not Found"));
					break;

				case Packets.Packet.PacketType.Disconnect:
					threads.RemoveAt(connectedClients.IndexOf(client));
					connectedClients.Remove(client);
					PrintToConsoleAsLogMessage(client.GetNickname() + " Left The Server. [" + client.GetSocket().RemoteEndPoint + "]");
					BroadcastDataToAllClients(new Packets.ChatMessagePacket("[Server] " + client.GetNickname() + " has left the chat!"));
					client.CloseConnection();
					//UpdateClientsOnlineBox();
					break;
			}
		}

		private void BroadcastDataToAllClients(Packets.Packet packet)
		{
			foreach(ConnectedClient client in connectedClients)
			{
				client.Send(packet);
			}
		}

		private void SendDataToSpecificClient(ConnectedClient client, Packets.Packet packet)
		{
			client.Send(packet);
		}

		private void UpdateClientsOnlineBox()
		{
			/*string clients = "/server.clientlist";

			foreach(ConnectedClient onlineClient in connectedClients)
			{
				clients += " " + onlineClient.GetNickname();
			}

			BroadcastDataToAllClients(clients);*/
		}

		public void PrintToConsoleAsLogMessage(string x)
		{
			DateTime now = DateTime.Now;

			Console.WriteLine("[" + now.Day + "/" + now.Month + "/" + now.Year + " " + now.Hour + ":" + now.Minute + ":" + now.Second + "] " + x);
		}
	}
}
