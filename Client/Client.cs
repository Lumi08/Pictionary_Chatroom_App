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
		private NetworkStream stream;
		private StreamReader streamReader;
		private StreamWriter streamWriter;

		private bool inApp;

		public Client(string ipAddress, int port)
		{
			tcpClient = new TcpClient();
			mainWindow = new MainWindow(this);

			if(ConnectToServer(ipAddress, port))
			{
				streamWriter.WriteLine("Ryan");
				streamWriter.Flush();

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

		public void ProcessServerResponse()
		{
			string data = streamReader.ReadLine();

			mainWindow.UpdateChatTextBox(data);
		}

		public void SendDataToServer(string data)
		{
			streamWriter.WriteLine(data);
			streamWriter.Flush();
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
