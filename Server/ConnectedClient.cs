using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Server
{ 
	class ConnectedClient
	{
		public IPEndPoint endPoint;

		private object writeLock;
		private object readLock;

		private Socket socket;
		private NetworkStream stream;
		private BinaryWriter writer;
		private BinaryReader reader;
		private BinaryFormatter formatter;

		//Security
		private RSACryptoServiceProvider rsaProvider;
		private RSAParameters publicKey;
		private RSAParameters privateKey;
		private RSAParameters clientKey;

		private string nickname;

		public ConnectedClient(Socket socket)
		{
			this.socket = socket;
			readLock = new object();
			writeLock = new object();

			stream = new NetworkStream(socket);
			writer = new BinaryWriter(stream);
			reader = new BinaryReader(stream);
			formatter = new BinaryFormatter();

			rsaProvider = new RSACryptoServiceProvider();
			publicKey = rsaProvider.ExportParameters(false);
			privateKey = rsaProvider.ExportParameters(true);
		}

		public void Login(string nickname, IPEndPoint endPoint, RSAParameters publicKey)
		{
			this.nickname = nickname;
			this.endPoint = endPoint;
			clientKey = publicKey;

			TcpSend(new Packets.KeyPacket(this.publicKey));
		}

		public void TcpSend(Packets.Packet packet)
		{
			lock(writeLock)
			{
				MemoryStream memoryStream = new MemoryStream();

				formatter.Serialize(memoryStream, packet);

				byte[] buffer = memoryStream.GetBuffer();

				writer.Write(buffer.Length);
				writer.Write(buffer);
				writer.Flush();
			}	
		}

		public Packets.Packet TcpRead()
		{
			lock(readLock)
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
				catch (Exception e)
				{

				}
				return null;
			}
		}

		//Security
		public byte[] Encrypt(byte[] data)
		{
			lock(rsaProvider)
			{
				rsaProvider.ImportParameters(clientKey);
				return rsaProvider.Encrypt(data, true);
			}
		}

		public byte[] Decrypt(byte[] data)
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

		public string DecryptString(byte[] data)
		{
			byte[] buffer = Decrypt(data);
			return Encoding.UTF8.GetString(buffer);

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
