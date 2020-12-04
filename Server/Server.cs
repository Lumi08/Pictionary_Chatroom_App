using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Server
{
	class Server
	{
		//Locals
		private TcpListener tcpListener;
		private UdpClient udpListener;
		private List<Thread> threads = new List<Thread>();
		private List<ConnectedClient> connectedClients = new List<ConnectedClient>();

		public Server(string ipAddress, int port)
		{
			tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
			udpListener = new UdpClient(port);
		}

		public void Start()
		{
			try
			{
				tcpListener.Start();
				Thread udpListenThread = new Thread(() => { UdpListen(); });
				udpListenThread.Start();
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
					//UpdateClientsOnlineBox();
					threads.Add(new Thread(() => { ClientMethod(connectedClients.Last()); }));
					threads.Last().Start();
				}
				else
				{
					TcpSendDataToSpecificClient(newClient, new Packets.DisconnectPacket());
					Thread test = new Thread(() => { ClientMethod(newClient); });
				}
			}

		}

		private void UdpListen()
		{
			try
			{
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
				while (true)
				{
					BinaryFormatter formatter = new BinaryFormatter(); 
					byte[] bytes = udpListener.Receive(ref endPoint);
					MemoryStream memoryStream = new MemoryStream(bytes);
					Packets.Packet recievedPacket = formatter.Deserialize(memoryStream) as Packets.Packet;

					foreach (ConnectedClient client in connectedClients)
					{
						if (endPoint.ToString() == client.endPoint.ToString())
						{
							udpListener.Send(bytes, bytes.Length, client.endPoint);
						}
					}
				}
			}
			catch (SocketException e)
			{
				Console.WriteLine("Server UDP Read Method Exception: " + e.Message);
			}
		}

		private void ClientMethod(ConnectedClient connectedClient)
		{
			Packets.Packet recievedDataFromClientSocket;

			while((recievedDataFromClientSocket = connectedClient.TcpRead()) != null)
			{
				TcpProcessDataSentFromClient(recievedDataFromClientSocket, connectedClient);
			}

			connectedClient.CloseConnection();
		}

		private void TcpProcessDataSentFromClient(Packets.Packet data, ConnectedClient client)
		{
			switch (data.m_PacketType)
			{
				case Packets.Packet.PacketType.Login:
					Packets.LoginPacket loginPacket = data as Packets.LoginPacket;
					client.Login(loginPacket.Nickname, loginPacket.Endpoint, loginPacket.PublicKey);
					PrintToConsoleAsLogMessage("[TCP] New Login from " + loginPacket.Endpoint);
					TcpBroadcastDataToAllClients(new Packets.ChatMessagePacket("[Server] " + connectedClients.Last().GetNickname() + " has joined the chat!"));
					break;

				case Packets.Packet.PacketType.Nickname:
					Packets.NicknamePacket nicknamePacket = data as Packets.NicknamePacket;
					if(nicknamePacket.Name == client.GetNickname())
					{
						TcpSendDataToSpecificClient(client, new Packets.ChatMessagePacket("[Error] You Can't Change Your Name to the Same Name"));
						return;
					}
					
					PrintToConsoleAsLogMessage("[TCP] [" + nicknamePacket.m_PacketType + "] from: " + client.GetNickname() + " data: " + nicknamePacket.Name);
					PrintToConsoleAsLogMessage("[TCP] [" + client.GetNickname() + "] Changed Name to " + nicknamePacket.Name);
					TcpBroadcastDataToAllClients(new Packets.ChatMessagePacket("[Server] " + client.GetNickname() + " Changed Name to " + nicknamePacket.Name));
					client.SetNickname(nicknamePacket.Name);
					break;

				case Packets.Packet.PacketType.ChatMessage:
					Packets.ChatMessagePacket messagePacket = data as Packets.ChatMessagePacket;
					TcpBroadcastDataToAllClients(new Packets.ChatMessagePacket("[" + client.GetNickname() + "] " + messagePacket.Message));
					PrintToConsoleAsLogMessage("[TCP] [" + messagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Message: " + messagePacket.Message);
					break;

				case Packets.Packet.PacketType.EncryptedChatMessage:
					Packets.EncryptedChatMessagePacket encryptedMessagePacket = data as Packets.EncryptedChatMessagePacket;
					string decryptedString = client.DecryptString(encryptedMessagePacket.Data);
					TcpBroadcastDataToAllClients(new Packets.EncryptedChatMessagePacket(client.EncryptString("[" + client.GetNickname() + "] " + decryptedString)));
					PrintToConsoleAsLogMessage("[TCP] [" + encryptedMessagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Message: " + decryptedString);
					break;

				case Packets.Packet.PacketType.PrivateMessage:
					Packets.PrivateMessagePacket privateMessagePacket = data as Packets.PrivateMessagePacket;

					foreach(ConnectedClient target in connectedClients)
					{
						if(target.GetNickname().ToLower() == privateMessagePacket.TargetUser.ToLower())
						{
							if(target == client)
							{
								PrintToConsoleAsLogMessage("[TCP] [Error] [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Tried to send a Private Message to Themself");
								TcpSendDataToSpecificClient(client, new Packets.ChatMessagePacket("[Error] You Can't Message Yourself"));
								return;
							}

							PrintToConsoleAsLogMessage("[TCP] [" + privateMessagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] " +
								"To [" + target.GetNickname() + " " + target.GetSocket().RemoteEndPoint + "] Message: " + privateMessagePacket.Message);

							string message = privateMessagePacket.Message;
							privateMessagePacket.Message = "[To " + target.GetNickname() + "] " + message;
							TcpSendDataToSpecificClient(client, privateMessagePacket);
							privateMessagePacket.Message = "[From " + client.GetNickname() + "] " + message;
							TcpSendDataToSpecificClient(target, privateMessagePacket);
							return;
						}
					}

					PrintToConsoleAsLogMessage("[TCP] [Error] [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Tried to send a Private Message to a client that does not exist");
					TcpSendDataToSpecificClient(client, new Packets.ChatMessagePacket("[Error] User not Found"));
					break;
				
				case Packets.Packet.PacketType.Disconnect:
					threads.RemoveAt(connectedClients.IndexOf(client));
					connectedClients.Remove(client);
					PrintToConsoleAsLogMessage("[TCP] " + client.GetNickname() + " Left The Server. [" + client.GetSocket().RemoteEndPoint + "]");
					TcpBroadcastDataToAllClients(new Packets.ChatMessagePacket("[Server] " + client.GetNickname() + " has left the chat!"));
					client.CloseConnection();
					//UpdateClientsOnlineBox();
					break;
			}
		}

		private void TcpBroadcastDataToAllClients(Packets.Packet packet)
		{
			foreach(ConnectedClient client in connectedClients)
			{
				client.TcpSend(packet);
			}
		}

		private void TcpSendDataToSpecificClient(ConnectedClient client, Packets.Packet packet)
		{
			client.TcpSend(packet);
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

		public void Stop()
		{
			tcpListener.Stop();
		}

		public void PrintToConsoleAsLogMessage(string x)
		{
			DateTime now = DateTime.Now;

			Console.WriteLine("[" + now.Day + "/" + now.Month + "/" + now.Year + " " + now.Hour + ":" + now.Minute + ":" + now.Second + "] " + x);
		}
	}
}
