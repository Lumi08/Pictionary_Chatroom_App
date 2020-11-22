using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace Client
{
	/// <summary>
	/// Interaction logic for ClientConnectionForm.xaml
	/// </summary>
	public partial class ClientConnectionForm : Window
	{
		private string nickname;
		private IPAddress ipAddress;
		private int port;
		private IPEndPoint endPoint;

		public ClientConnectionForm()
		{
			InitializeComponent();
		}

		private void SubmitButton_Click(object sender, RoutedEventArgs e)
		{
			if (NicknameBox.Text == "")
			{
				ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
				ErrorLabel.Content = "Please Enter a Nickname";
				return;
			}

			if(IpBox.Text == "")
			{
				ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
				ErrorLabel.Content = "Please Enter a IP Number";
				return;
			}

			if(PortBox.Text == "")
			{
				ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
				ErrorLabel.Content = "Please Enter a Port Address";
				return;
			}

			this.nickname = NicknameBox.Text;
			

			try
			{
				this.port = Int32.Parse(PortBox.Text);
			}
			catch
			{
				ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
				ErrorLabel.Content = "Enter a Valid Port Number";
				return;
			}

			try
			{
				this.ipAddress = IPAddress.Parse(IpBox.Text);
			}
			catch
			{
				ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
				ErrorLabel.Content = "Enter a Valid IP Address";
				return;
			}

			endPoint = new IPEndPoint(ipAddress, port);
			Close();
		}

		public string GetNickname()
		{
			return this.nickname;
		}

		public IPEndPoint GetIPEndPoint()
		{
			return endPoint;
		}
	}
}
