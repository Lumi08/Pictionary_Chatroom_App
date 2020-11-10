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
		private ServerConnectionWindow serverConnectionWindow;

		//Network
		private TcpClient tcpClient;
		private NetworkStream stream;
		private StreamReader streamReader;
		private StreamWriter streamWriter;

		public bool inApp;

		public Client(string ipAddress, int port)
		{
			tcpClient = new TcpClient();
			serverConnectionWindow = new ServerConnectionWindow(this);
			serverConnectionWindow.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			serverConnectionWindow.MaximizeBox = false;
			Thread formThread1 = new Thread(() => { ShowForm(serverConnectionWindow); });

			formThread1.Start();
			formThread1.Join();

			mainWindow = new MainWindow(this);
			mainWindow.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			mainWindow.MaximizeBox = false;
			mainWindow.Text = "Chat Room  -  Nickname: " + serverConnectionWindow.GetNickname();
			formThread1 = new Thread(() => { ShowForm(mainWindow); });

			streamWriter.WriteLine(serverConnectionWindow.GetNickname());
			streamWriter.Flush();

			formThread1.Start();
			Run();
			formThread1.Join();
		}

		public void Run()
		{ 
			while(inApp)
			{
				ProcessServerResponse();
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

		public void ProcessServerResponse()
		{
			try
			{
				string data = streamReader.ReadLine();
				mainWindow.UpdateChatTextBox(data);
			}
			catch(Exception e)
			{

			}
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
		
		private void ShowForm(ServerConnectionWindow mainWindow)
		{
			mainWindow.ShowDialog();
		}

		public void Close()
		{
			inApp = false;
			SendDataToServer("/client.disconnect");
			tcpClient.Close();
		}
	}
}
