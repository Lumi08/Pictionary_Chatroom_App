using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{ 
	class ConnectedClient
	{
		private Socket socket;
		private NetworkStream stream;
		private BinaryWriter writer;
		private BinaryReader reader;
		private BinaryFormatter formatter;

		private string nickname;

		public ConnectedClient(Socket socket)
		{
			this.socket = socket;

			stream = new NetworkStream(socket);
			writer = new BinaryWriter(stream);
			reader = new BinaryReader(stream);
			formatter = new BinaryFormatter();

			Packets.NicknamePacket initialNicknamePacket = Read() as Packets.NicknamePacket;
			nickname = initialNicknamePacket.Name;
		}

		public void Send(Packets.Packet packet)
		{
			MemoryStream memoryStream = new MemoryStream();

			formatter.Serialize(memoryStream, packet);

			byte[] buffer = memoryStream.GetBuffer();

			writer.Write(buffer.Length);
			writer.Write(buffer);
			writer.Flush();
		}

		public Packets.Packet Read()
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
			}
			catch(Exception e)
			{
				
			}
			return null;
		}

		public void CloseConnection()
		{
			socket.Close();
			stream.Close();
			reader.Close();
			writer.Close();
		}
		public void SetNickname(string name)
		{
			this.nickname = name;
		}

		public string GetNickname()
		{
			return nickname;
		}

		public Socket GetSocket()
		{
			return socket;
		}

	}
}
