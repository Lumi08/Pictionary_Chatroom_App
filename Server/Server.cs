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
		private List<ConnectedClient> pictionaryLobby = new List<ConnectedClient>();

		private int maxClients;
		private bool playingPictionary;
		private string pictionaryCurrentWordBeingDrawn;
		private int pictionaryLobbyMaxSize;
		private int[] pictionaryScores;

		public Server(string ipAddress, int port, int maxClients, int pictionaryLobbyMaxSize)
		{
			tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
			udpListener = new UdpClient(port);
			this.maxClients = maxClients;
			this.pictionaryLobbyMaxSize = pictionaryLobbyMaxSize;
			pictionaryScores = new int[pictionaryLobbyMaxSize];
			playingPictionary = false;
		}

		public void Start()
		{
			try
			{
				tcpListener.Start();
				Thread udpListenThread = new Thread(() => { UdpListen(); });
				udpListenThread.Start();
			}
			catch (Exception e)
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

				if (connectedClients.Count < maxClients)
				{
					connectedClients.Add(newClient);
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
						udpListener.Send(bytes, bytes.Length, client.endPoint);	
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
					PrintToConsoleAsLogMessage("[TCP] " + loginPacket.Nickname + " Joined the Server [" + loginPacket.Endpoint + "]");
					UpdateClientsOnlineBox();
					SendEncryptedChatPacket("[Server] " + connectedClients.Last().GetNickname() + " has joined the chat!");
					break;

				case Packets.Packet.PacketType.Nickname:
					Packets.NicknamePacket nicknamePacket = data as Packets.NicknamePacket;
					if(nicknamePacket.Name == client.GetNickname())
					{
						TcpSendDataToSpecificClient(client, new Packets.ChatMessagePacket(client.EncryptString("[Error] You Can't Change Your Name to the Same Name")));
						return;
					}
					
					PrintToConsoleAsLogMessage("[TCP] [" + nicknamePacket.m_PacketType + "] from: " + client.GetNickname() + " data: " + nicknamePacket.Name);
					PrintToConsoleAsLogMessage("[TCP] [" + client.GetNickname() + "] Changed Name to " + nicknamePacket.Name);
					UpdateClientsOnlineBox();
					SendEncryptedChatPacket("[Server] " + client.GetNickname() + " Changed Name to " + nicknamePacket.Name);
					client.SetNickname(nicknamePacket.Name);
					break;

				case Packets.Packet.PacketType.ChatMessage:
					Packets.ChatMessagePacket messagePacket = data as Packets.ChatMessagePacket;
					string chatMessage = client.DecryptString(messagePacket.Message);
					SendEncryptedChatPacket("[" + client.GetNickname() + "] " + chatMessage);
					PrintToConsoleAsLogMessage("[TCP] [" + messagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Message: " + chatMessage);
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
								TcpSendDataToSpecificClient(client, new Packets.ChatMessagePacket(client.EncryptString("[Error] You Can't Message Yourself")));
								return;
							}

							string privateMessage = client.DecryptString(privateMessagePacket.Message);
							PrintToConsoleAsLogMessage("[TCP] [" + privateMessagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] " +
								"To [" + target.GetNickname() + " " + target.GetSocket().RemoteEndPoint + "] Message: " + privateMessage);

							privateMessagePacket.Message = client.EncryptString("[To " + target.GetNickname() + "] " + privateMessage);
							TcpSendDataToSpecificClient(client, privateMessagePacket);
							privateMessagePacket.Message = target.EncryptString("[From " + client.GetNickname() + "] " + privateMessage);
							TcpSendDataToSpecificClient(target, privateMessagePacket);
							return;
						}
					}

					PrintToConsoleAsLogMessage("[TCP] [Error] [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Tried to send a Private Message to a client that does not exist");
					TcpSendDataToSpecificClient(client, new Packets.ChatMessagePacket(client.EncryptString("[Error] User not Found")));
					break;

				case Packets.Packet.PacketType.GameConnectionPacket:
					Packets.GameConnectionPacket gameConnectionPacket = data as Packets.GameConnectionPacket;

					if(gameConnectionPacket.GameToPlay == Packets.Packet.GameType.Pictionary)
					{
						if(pictionaryLobby.Contains(client))
						{
							pictionaryLobby.Remove(client);
							SendEncryptedChatPacket("[Server] " + client.GetNickname() + " has left the Pictionary Lobby [" + pictionaryLobby.Count + "|" + pictionaryLobbyMaxSize + "]");
							if (playingPictionary)
							{
								foreach (ConnectedClient c in pictionaryLobby)
								{
									TcpSendDataToSpecificClient(c, new Packets.PictionaryChatMessagePacket(c.EncryptString("[Server] " + c.GetNickname() + " Has Left The Game!")));
								}
							}
							if (pictionaryLobby.Count == 0)
							{
								playingPictionary = false;
							}
						}
						else
						{
							if(pictionaryLobby.Count > pictionaryLobbyMaxSize - 1 || playingPictionary)
							{
								TcpSendDataToSpecificClient(client, new Packets.ChatMessagePacket(client.EncryptString("[Server] Server is Full")));
							}
							else
							{
								pictionaryLobby.Add(client);
								SendEncryptedChatPacket("[Server] " + client.GetNickname() + " have joined the Pictionary Lobby [" + pictionaryLobby.Count + "|" + pictionaryLobbyMaxSize +"]");
							}
						}

						if (pictionaryLobby.Count > pictionaryLobbyMaxSize - 1)
						{
							StartPictionaryRound();
						}
					}
					break;
				
				case Packets.Packet.PacketType.Disconnect:
					PrintToConsoleAsLogMessage("[TCP] " + client.GetNickname() + " Left The Server. [" + client.GetSocket().RemoteEndPoint + "]");
					SendEncryptedChatPacket("[Server] " + client.GetNickname() + " has left the chat!");
					if(pictionaryLobby.Contains(client))
					{
						SendEncryptedPictionartChatPacket("[Server] " + client.GetNickname() + " has Left the Game!");
						SendEncryptedChatPacket("[Server] " + client.GetNickname() + " has left the Pictionary Lobby [" + pictionaryLobby.Count + "|" + pictionaryLobbyMaxSize + "]");
						pictionaryLobby.Remove(client);
					}
					threads.RemoveAt(connectedClients.IndexOf(client));
					connectedClients.Remove(client);

					client.CloseConnection();
					UpdateClientsOnlineBox();
					break;

				case Packets.Packet.PacketType.PictionaryChatMessage:
					Packets.PictionaryChatMessagePacket pictionaryChatMessagePacket = data as Packets.PictionaryChatMessagePacket;
					string pictionaryChatMessage = client.DecryptString(pictionaryChatMessagePacket.Message);
					SendEncryptedPictionartChatPacket("[" + client.GetNickname() + "] " + pictionaryChatMessage);
					PrintToConsoleAsLogMessage("[TCP] [" + pictionaryChatMessagePacket.m_PacketType + "] from [" + client.GetNickname() + " " + client.GetSocket().RemoteEndPoint + "] Message: " + pictionaryChatMessage);

					if(pictionaryChatMessage.ToLower() == pictionaryCurrentWordBeingDrawn)
					{
						pictionaryScores[pictionaryLobby.IndexOf(client)]++;
						SendEncryptedPictionartChatPacket("[Server] " + client.GetNickname() + " Guessed The Word!");
						SendEncryptedPictionartChatPacket("[Server] New Round!");

						foreach(ConnectedClient c in pictionaryLobby)
						{
							TcpSendDataToSpecificClient(c, new Packets.PictionaryClearCanvasPacket());
						}
						StartPictionaryRound();
					}
					break;
			}
		}

		private void StartPictionaryRound()
		{
			playingPictionary = true;
			string[] pictionaryWordList = PictionaryWordsFromTextFile("F:/Projects/Chat-Facility/Server/PictionaryWords.txt");

			if (pictionaryWordList == null)
			{
				foreach (ConnectedClient c in pictionaryLobby)
				{
					TcpSendDataToSpecificClient(c, new Packets.ChatMessagePacket(c.EncryptString("[Server Error] Pictionary Words couldnt be loaded")));
				}
				pictionaryLobby.Clear();
				return;
			}

			Random rand = new Random();
			int r = rand.Next(connectedClients.Count);
			int r2 = rand.Next(pictionaryWordList.Length - 1);
			pictionaryCurrentWordBeingDrawn = pictionaryWordList[r2];

			for (int i = 0; i < pictionaryLobby.Count; i++)
			{
				if (i == r)
				{
					TcpSendDataToSpecificClient(pictionaryLobby[i], new Packets.PictionarySetupClientPacket(true));
					TcpSendDataToSpecificClient(pictionaryLobby[i], new Packets.PictionaryWordToDrawPacket(pictionaryLobby[i].EncryptString(pictionaryCurrentWordBeingDrawn)));
				}
				else
				{
					TcpSendDataToSpecificClient(pictionaryLobby[i], new Packets.PictionarySetupClientPacket(false));
				}
			}
			SendEncryptedPictionartChatPacket("[Server] " + pictionaryLobby[r].GetNickname() + " Is Drawing!");
		}

		private string[] PictionaryWordsFromTextFile(string path)
		{
			try
			{
				string[] lines = File.ReadAllLines(path);
				return lines;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			return null;
		}

		private void SendEncryptedChatPacket(string message)
		{
			foreach (ConnectedClient client in connectedClients)
			{
				client.TcpSend(new Packets.ChatMessagePacket(client.EncryptString(message)));
			}
		}

		private void SendEncryptedPictionartChatPacket(string message)
		{
			foreach (ConnectedClient client in connectedClients)
			{
				client.TcpSend(new Packets.PictionaryChatMessagePacket(client.EncryptString(message)));
			}
		}

		private void TcpBroadcastDataToAllClients(Packets.Packet packet)
		{
			foreach(ConnectedClient c in connectedClients)
			{
				c.TcpSend(packet);
			}
		}

		private void TcpSendDataToSpecificClient(ConnectedClient client, Packets.Packet packet)
		{
			client.TcpSend(packet);
		}

		private void UpdateClientsOnlineBox()
		{
			string[] clients = new string[maxClients];

			int i = 0;
			foreach(ConnectedClient client in connectedClients)
			{
				clients[i] = client.GetNickname();
				i++;
			}

			TcpBroadcastDataToAllClients(new Packets.ClientsPacket(clients));
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
