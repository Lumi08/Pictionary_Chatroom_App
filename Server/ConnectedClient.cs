using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Server
{ 
	class ConnectedClient
	{
		private Socket socket;
		private NetworkStream stream;
		private StreamWriter streamWriter;
		private StreamReader streamReader;

		private string nickname;

		public ConnectedClient(Socket socket)
		{
			this.socket = socket;

			stream = new NetworkStream(socket);
			streamReader = new StreamReader(stream);
			streamWriter = new StreamWriter(stream);

			//First thing the client will send to the server is the clients nickname
			nickname = streamReader.ReadLine();
		}

		public void CloseConnection()
		{
			socket.Close();
		}

		public StreamReader GetStreamReader()
		{
			return streamReader;
		}

		public StreamWriter GetStreamWriter()
		{
			return streamWriter;
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
