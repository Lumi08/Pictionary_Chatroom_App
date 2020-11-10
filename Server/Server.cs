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
			tcpListener.Start();

			PrintToConsoleAsLogMessage("Server Awaiting Client");
			
			while (true)
			{
				Socket clientSocket = tcpListener.AcceptSocket();
				connectedClients.Add(new ConnectedClient(clientSocket));
				PrintToConsoleAsLogMessage(connectedClients.Last().GetNickname() + " Joined the server!");
				threads.Add(new Thread(() => { ClientMethod(clientSocket); }));
				threads.Last().Start();
			}

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
