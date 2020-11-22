using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;

namespace Client
{
	public class ClientManager
	{
		//Forms
		private ClientForm clientForm;

		//Network
		private TcpClient tcpClient;
		private NetworkStream stream;
		private BinaryReader reader;
		private BinaryWriter writer;
		private BinaryFormatter formatter;

		private Thread networkProcessingThread;
		//private Thread formThread;
		public bool inApp;

		public ClientManager()
		{
			clientForm = new ClientForm(this);
			Start();
		}

		public void Start()
		{
			networkProcessingThread = new Thread(() => { Run(); });
			//tcpClient = new TcpClient();
			ShowForm(clientForm);
		}

		public void Run()
		{
			Packets.Packet serverResponse;
			while ((serverResponse = ReadDataFromserver()) != null)
			{
				ProcessServerResponse(serverResponse);
			}
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
				networkProcessingThread = new Thread(() => { Run(); });

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
				SendDataToServer(new Packets.NicknamePacket(nickname));

				networkProcessingThread.Start();
				return true;
			}
			else
			{
				clientForm.UpdateChatWindow("[Error] Failed to Connect to server!", Colors.Red);
			}

			return false;
		}

		public void ProcessServerResponse(Packets.Packet serverResponse)
		{
			switch(serverResponse.m_PacketType)
			{
				case Packets.Packet.PacketType.ChatMessage:
					Packets.ChatMessagePacket chatPacket = serverResponse as Packets.ChatMessagePacket;
					clientForm.UpdateChatWindow(chatPacket.Message, Colors.Black);
					break;

				case Packets.Packet.PacketType.Disconnect:
					clientForm.ServerFullLogic();
					Close();
					break;
					
			}


			/*string[] dataArray = serverResponse.Split(' ');
			string serverProcess = dataArray[0];
			string serverData = serverResponse.Substring(serverProcess.Length + 1);

			if (serverProcess == "/server.message")
			{
				clientForm.UpdateChatWindow(serverData);
			}

			if (serverProcess == "/server.clientlist")
			{
				//clientForm.UpdateClientsOnlineBox(serverData);
			}

			if (serverProcess == "/server.full")
			{
				networkProcessingThread.Abort();
				//mainWindow.Close();
				Close();
			}*/
		}

		public void SendDataToServer(Packets.Packet packet)
		{
			MemoryStream memoryStream = new MemoryStream();

			formatter.Serialize(memoryStream, packet);

			byte[] buffer = memoryStream.GetBuffer();

			writer.Write(buffer.Length);
			writer.Write(buffer);
			writer.Flush();
		}

		public Packets.Packet ReadDataFromserver()
		{
			int numberOfBytes;
			if ((numberOfBytes = reader.ReadInt32()) != -1)
			{
				byte[] buffer = reader.ReadBytes(numberOfBytes);
				MemoryStream memoryStream = new MemoryStream(buffer);
				return formatter.Deserialize(memoryStream) as Packets.Packet;
			}
			return null;
		}

		private void ShowForm(ClientForm window)
		{
			window.ShowDialog();
		}

		public void Close()
		{
			networkProcessingThread.Abort();
			if(clientForm.isConnected)
			{
				SendDataToServer(new Packets.DisconnectPacket());
				tcpClient.Close();
			}
		}
	}
}
