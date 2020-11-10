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
	public class Client
	{
		//Forms
		private MainWindow mainWindow;

		//Network
		private TcpClient tcpClient;
		private NetworkStream messageStream;
		private StreamReader messageStreamReader;
		private StreamWriter messageStreamWriter;

		private bool inApp;

		public Client(string ipAddress, int port)
		{
			tcpClient = new TcpClient();
			mainWindow = new MainWindow(this);

			if(ConnectToServer(ipAddress, port))
			{

				Thread formThread = new Thread(() => { ShowForm(mainWindow); });
				formThread.Start();
				Run();
				formThread.Join();
			}
		}

		public void Run()
		{
			inApp = true;

			while(inApp)
			{
				ProcessServerResponse();
			}
		}

		private bool ConnectToServer(string ipAddress, int port)
		{
			try
			{
				tcpClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
				messageStream = tcpClient.GetStream();
				messageStreamReader = new StreamReader(messageStream);
				messageStreamWriter = new StreamWriter(messageStream);

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
				return false;
			}
		}

		public void ProcessServerResponse()
		{
			string data;

			if ((data = messageStreamReader.ReadLine()) != null)
			{
				mainWindow.UpdateChatTextBox(data);
			}
		}

		public void SendDataToServer(string data)
		{
			messageStreamWriter.WriteLine(data);
			messageStreamWriter.Flush();
		}

		private void ShowForm(MainWindow mainWindow)
		{
			mainWindow.ShowDialog();
		}

		public void Close()
		{
			inApp = false;
			tcpClient.Close();
		}
	}
}
