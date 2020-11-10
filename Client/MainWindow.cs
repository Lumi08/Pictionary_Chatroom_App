using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
	public partial class MainWindow : Form
	{
		//Delegates
		public delegate void UpdateChatTextBoxDelegate(string data);
		public UpdateChatTextBoxDelegate updateChatTextBoxDelegate;

		//Locals
		Client client;

		public MainWindow(Client client)
		{
			this.client = client;
			InitializeComponent();
			updateChatTextBoxDelegate = new UpdateChatTextBoxDelegate(UpdateChatTextBox);
		}

		public void UpdateChatTextBox(string data)
		{
			if(ChatTextBox.InvokeRequired)
			{
				Invoke(updateChatTextBoxDelegate, data);
			}
			else
			{
				ChatTextBox.Text += data += Environment.NewLine;
				ChatTextBox.SelectionStart = ChatTextBox.Text.Length;
				ChatTextBox.ScrollToCaret();
			}
		}

		public void SendButton_Click(object sender, EventArgs e)
		{
			//If there is no message to send then dont send the message
			if(InputMessageTextBox.Text == "")
			{
				return;
			}

			client.SendDataToServer(InputMessageTextBox.Text);
		}

		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			client.Close();
		}
	}
}
