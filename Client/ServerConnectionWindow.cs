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
	public partial class ServerConnectionWindow : Form
	{
		private string nickname;
		private string ipAddress;
		private int port;
		private Client client;


		public ServerConnectionWindow(Client client)
		{
			this.client = client;
			InitializeComponent();
		}

		private void ConnectButton_Click(object sender, EventArgs e)
		{
			ErrorLabel.Text = "";

			if (NicknameInputBox.Text == "")
			{
				ErrorLabel.Text = "Please Enter a Nickname";
				return;
			}

			if (IpInputBox.Text == "")
			{
				ErrorLabel.Text = "Please Enter a IP Number";
				return;
			}

			if (PortInputBox.Text == "")
			{
				ErrorLabel.Text = "Please Enter a Port Address";
				return;
			}

			this.nickname = NicknameInputBox.Text;
			this.ipAddress = IpInputBox.Text;
			this.port = Int32.Parse(PortInputBox.Text);

			if (client.ConnectToServer(ipAddress, port))
			{
				Close();
			}
			else
			{
				ErrorLabel.ForeColor = Color.Red;
				ErrorLabel.Text = "Failed to connect";
			}
		}

		public void SetErrorMessage(string message)
		{
			ErrorLabel.Text = message;
		}

		public string GetNickname()
		{
			return this.nickname;
		}

		public string GetIpAdress()
		{
			return this.ipAddress;
		}

		public int GetPort()
		{
			return this.port;
		}
	}
}
