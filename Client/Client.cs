using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{
	public class Client
	{
		MainWindow mainWindow;

		public Client()
		{
			mainWindow = new MainWindow(this);

			Thread formThread = new Thread(() => { ShowForm(mainWindow); });
			formThread.Start();
			formThread.Join();
		}

		private void ShowForm(MainWindow mainWindow)
		{
			mainWindow.ShowDialog();
		}
	}
}
