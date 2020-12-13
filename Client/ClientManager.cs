using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
	public class ClientManager
	{
		//Forms
		private ClientForm clientForm;
		private PictionaryForm pictionaryForm;

		//Network
		private TcpClient tcpClient;
		private UdpClient udpClient;
		private NetworkStream stream;
		private BinaryReader reader;
		private BinaryWriter writer;
		private BinaryFormatter formatter;

		//Security
		private RSACryptoServiceProvider rsaProvider;
		private RSAParameters publicKey;
		private RSAParameters privateKey;
		private RSAParameters serverKey;

		private Thread tcpNetworkProcessingThread;
		private Thread udpNetworkProcessingThread;

		//private Thread formThread;
		public bool playingPictionary;
		public string clientNickname;

		public ClientManager()
		{
			clientForm = new ClientForm(this);
			pictionaryForm = new PictionaryForm(this);
			rsaProvider = new RSACryptoServiceProvider(1024);
			publicKey = rsaProvider.ExportParameters(false);
			privateKey = rsaProvider.ExportParameters(true);

			ShowForm(clientForm);
		}

		//Connections
		public void Login(string nickname)
		{
			clientNickname = nickname;
			TcpSendDataToServer(new Packets.LoginPacket(nickname, (IPEndPoint)udpClient.Client.LocalEndPoint, publicKey));
		}

		public bool ConnectToServer(IPEndPoint iPEndPoint)
		{
			try
			{
				tcpClient = new TcpClient();
				tcpClient.Connect(iPEndPoint);
				stream = tcpClient.GetStream();
				formatter = new BinaryFormatter();
				reader = new BinaryReader(stream);
				writer = new BinaryWriter(stream);
				udpClient = new UdpClient();
				udpClient.Connect(iPEndPoint);
				tcpNetworkProcessingThread = new Thread(() => { TcpProcessServerResponse(); });
				udpNetworkProcessingThread = new Thread(() => { UdpProccessServerResponse(); });

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
				return false;
			}
		}

		public bool AttemptToConnect(IPEndPoint iPEndPoint, string nickname)
		{
			if (ConnectToServer(iPEndPoint))
			{
				Login(nickname);

				tcpNetworkProcessingThread.Start();
				udpNetworkProcessingThread.Start();
				clientForm.Title = "Chat Facility  |  Nickname: " + nickname;
				return true;
			}
			else
			{
				clientForm.UpdateChatWindow("[Error] Failed to Connect to server!", Colors.Red);
			}

			return false;
		}

		public void PlayPictionary()
		{
			TcpSendDataToServer(new Packets.GameConnectionPacket(Packets.Packet.GameType.Pictionary));
		}

		//Processing server response
		private void UdpProccessServerResponse()
		{
			try
			{
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
				while (true)
				{
					byte[] bytes = udpClient.Receive(ref endPoint);
					MemoryStream memoryStream = new MemoryStream(bytes);
					Packets.Packet recievedPacket = formatter.Deserialize(memoryStream) as Packets.Packet;

					switch (recievedPacket.m_PacketType)
					{
						case Packets.Packet.PacketType.PictionaryPaint:
							Packets.PictionaryPaintPacket paintPacket = recievedPacket as Packets.PictionaryPaintPacket;
							pictionaryForm.UpdatePaintCanvas(paintPacket.XPositions, paintPacket.YPositions, paintPacket.PenColor, paintPacket.SameLine);
							break;

						case Packets.Packet.PacketType.PictionaryClearCanvas:
							pictionaryForm.ClearCanvas();
							break;
					}
				}
			}
			catch(SocketException e)
			{
				Console.WriteLine("Client UDP Read Method Exception: " + e.Message);
			}
		}

		private void TcpProcessServerResponse()
		{
			Packets.Packet serverResponse;
			while ((serverResponse = TcpReadDataFromserver()) != null)
			{
				switch (serverResponse.m_PacketType)
				{
					case Packets.Packet.PacketType.ChatMessage:
						Packets.ChatMessagePacket chatPacket = serverResponse as Packets.ChatMessagePacket;
						clientForm.UpdateChatWindow(DecryptString(chatPacket.Message), Colors.Black);
						break;

					case Packets.Packet.PacketType.PrivateMessage:
						Packets.PrivateMessagePacket privateMessagePacket = serverResponse as Packets.PrivateMessagePacket;
						clientForm.UpdateChatWindow(DecryptString(privateMessagePacket.Message), Colors.DeepPink);
						break;

					case Packets.Packet.PacketType.Disconnect:
						clientForm.ServerFullLogic();
						Close();
						break;

					case Packets.Packet.PacketType.KeyPacket:
						Packets.KeyPacket keyPacket = serverResponse as Packets.KeyPacket;
						serverKey = keyPacket.Key;
						break;

					case Packets.Packet.PacketType.ClientList:
						Packets.ClientsPacket clientsPacket = serverResponse as Packets.ClientsPacket;
						clientForm.UpdateClientWindow(clientsPacket.Clients);
						break;

					case Packets.Packet.PacketType.PictionaryChatMessage:
						Packets.PictionaryChatMessagePacket pictionaryChatMessagePacket = serverResponse as Packets.PictionaryChatMessagePacket;
						pictionaryForm.UpdateChatWindow(DecryptString(pictionaryChatMessagePacket.message), Colors.Black);
						break;

					case Packets.Packet.PacketType.PictionarySetupClient:
						Packets.PictionarySetupClientPacket pictionarySetupClientPacket = serverResponse as Packets.PictionarySetupClientPacket;
						SetUpPictionary(pictionarySetupClientPacket.IsDrawer);
						break;

					case Packets.Packet.PacketType.PictionaryWordToDraw:
						Packets.PictionaryWordToDrawPacket pictionaryWordToDrawPacket = serverResponse as Packets.PictionaryWordToDrawPacket;
						pictionaryForm.RecievedWordToDraw(DecryptString(pictionaryWordToDrawPacket.WordToDraw));
						break;

					case Packets.Packet.PacketType.PictionaryClearCanvas:
						pictionaryForm.ClearCanvas();
						break;
				}
			}
		}

		public void SetUpPictionary(bool isDrawer)
		{
			clientForm.Dispatcher.Invoke(() => 
			{
				if(pictionaryForm == null || !pictionaryForm.IsVisible)
				{
					pictionaryForm = new PictionaryForm(this);
					pictionaryForm.SetUpDrawer(isDrawer);
					pictionaryForm.Title = "Pictionary  |  Nickname: " + clientNickname;
					pictionaryForm.Show();		
				}

				if(pictionaryForm.IsVisible)
				{
					pictionaryForm.SetUpDrawer(isDrawer);
				}
			});
		}

		public void PictionaryFormClosed()
		{
		}

		//Server Data management
		public void UdpSendDataToServer(Packets.Packet packet)
		{
			MemoryStream memoryStream = new MemoryStream();
			formatter.Serialize(memoryStream, packet);
			byte[] buffer = memoryStream.GetBuffer();

			udpClient.Send(buffer, buffer.Length);
		}

		public void TcpSendDataToServer(Packets.Packet packet)
		{
			MemoryStream memoryStream = new MemoryStream();
			formatter.Serialize(memoryStream, packet);
			byte[] buffer = memoryStream.GetBuffer();

			writer.Write(buffer.Length);
			writer.Write(buffer);
			writer.Flush();
		}

		public Packets.Packet TcpReadDataFromserver()
		{
			int numberOfBytes;
			try
			{
				if ((numberOfBytes = reader.ReadInt32()) != -1)
				{
					byte[] buffer = reader.ReadBytes(numberOfBytes);
					MemoryStream memoryStream = new MemoryStream(buffer);
					return formatter.Deserialize(memoryStream) as Packets.Packet;
				}
			} catch (Exception e){ }

			return null;
		}

		private void ShowForm(ClientForm window)
		{
			window.ShowDialog();
		}

		//Security
		public byte[] Encrypt(byte[] data)
		{
			lock (rsaProvider)
			{
				rsaProvider.ImportParameters(serverKey);
				return rsaProvider.Encrypt(data, true);
			}
		}

		private byte[] Decrypt(byte[] data)
		{
			lock (rsaProvider)
			{
				rsaProvider.ImportParameters(privateKey);
				return rsaProvider.Decrypt(data, true);
			}
		}

		public byte[] EncryptString(string data)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(data);
			return Encrypt(buffer);
		}

		private string DecryptString(byte[] data)
		{
			byte[] buffer = Decrypt(data);
			return Encoding.UTF8.GetString(buffer);
		}

		public void Close()
		{
			if (pictionaryForm != null)
			{
				pictionaryForm.Close();
			}
			if(clientForm.isConnected)
			{
				tcpNetworkProcessingThread.Abort();
				TcpSendDataToServer(new Packets.DisconnectPacket());
				udpClient.Close();
				tcpClient.Close();
			}
		}
	}
}
