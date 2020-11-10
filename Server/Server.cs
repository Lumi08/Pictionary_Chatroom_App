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
	class Server
	{
		//Locals
		private TcpListener tcpListener;

		public Server(string ipAddress, int port)
		{
			tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
		}

		public void Start()
		{
			tcpListener.Start();

			PrintToConsoleAsLogMessage("Server Awaiting Client");

			Socket clientSocket = tcpListener.AcceptSocket();
			PrintToConsoleAsLogMessage("Client Connected");
			ClientMethod(clientSocket);

		}

		public void Stop()
		{
			tcpListener.Stop();
		}

		private void ClientMethod(Socket socket)
		{
			string recievedDataFromClientSocket;
			
			NetworkStream stream = new NetworkStream(socket);
			StreamReader streamReader = new StreamReader(stream);
			StreamWriter streamWriter = new StreamWriter(stream);

			while((recievedDataFromClientSocket = streamReader.ReadLine()) != null)
			{
				string reply = ProcessDataSentFromClient(recievedDataFromClientSocket);
				Console.WriteLine(reply);
				streamWriter.WriteLine(reply);
				streamWriter.Flush();
			}

		}

		private string ProcessDataSentFromClient(string data)
		{
			if(data == "hi")
			{
				return "hello";
			}

			if(data == "bye")
			{
				return "goodbye";
			}

			return "I dont understand what youve inputed";
		}

		public void PrintToConsoleAsLogMessage(string x)
		{
			DateTime now = DateTime.Now;

			Console.WriteLine("[" + now.Day + "/" + now.Month + "/" + now.Year + " " + now.Hour + ":" + now.Minute + ":" + now.Second + "] " + x);
		}
	}
}
