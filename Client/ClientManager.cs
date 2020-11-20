using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Client
{
	public class ClientManager
	{
		//Forms
		private ClientForm clientForm;

		//Network
		private TcpClient tcpClient;
		private NetworkStream stream;
		private StreamReader streamReader;
		private StreamWriter streamWriter;

		private Thread processingThread;
		public bool inApp;

		public ClientManager()
		{
			clientForm = new ClientForm(this);
			Start();
		}

		public void Start()
		{
			processingThread = new Thread(() => { Run(); });
			tcpClient = new TcpClient();

			if (ConnectToServer("192.168.0.13", 4444))
			{
				streamWriter.WriteLine("Ryan");
				streamWriter.Flush();

				processingThread.Start();
				ShowForm(clientForm);
				processingThread.Join();
			}
			else
			{
				clientForm.UpdateChatWindow("Cant Connect to server!");
			}

		}

		public void Run()
		{
			string serverResponse;
			while ((serverResponse = streamReader.ReadLine()) != null)
			{
				ProcessServerResponse(serverResponse);
			}
		}

		public bool ConnectToServer(string ipAddress, int port)
		{
			try
			{
				tcpClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
				stream = tcpClient.GetStream();
				streamReader = new StreamReader(stream);
				streamWriter = new StreamWriter(stream);

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
				return false;
			}
		}

		public void ProcessServerResponse(string serverResponse)
		{
			string[] dataArray = serverResponse.Split(' ');
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
				processingThread.Abort();
				//mainWindow.Close();
				Close();
			}
		}

		public void SendDataToServer(string data)
		{
			streamWriter.WriteLine(data);
			streamWriter.Flush();
		}

		private void ShowForm(ClientForm window)
		{
			window.ShowDialog();
		}

		public void Close()
		{
			processingThread.Abort();
			SendDataToServer("/client.disconnect");
			tcpClient.Close();
		}
	}
}
