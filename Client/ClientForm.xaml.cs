using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Client
{
	/// <summary>
	/// Interaction logic for ClientForm.xaml
	/// </summary>
	public partial class ClientForm : Window
	{
		private string VERSION = "0.28";

		private ClientManager client;
		public bool isConnected;
		public bool menuOpened = false;
		public bool clientsOpen = false;

		private ClientConnectionForm connectionForm;

		public ClientForm(ClientManager client)
		{
			InitializeComponent();
			this.client = client;
			MessageWindow.Foreground = new SolidColorBrush(Colors.Gray);
			MessageWindow.AppendText("Welcome to Chat Facility v" + VERSION); 
		}

		public void UpdateChatWindow(string message, Color color)
		{
			MessageWindow.Dispatcher.Invoke(() =>
			{
				Run run = new Run(message);
				run.Foreground = new SolidColorBrush(color);
				Paragraph paragraph = new Paragraph(run);

				MessageWindow.Document.Blocks.Add(paragraph);

				MessageWindow.ScrollToEnd();

			});
		}

		public void ServerFullLogic()
		{
			this.Dispatcher.Invoke(() =>
			{
				UpdateChatWindow("[Error] Server is Currently Full", Colors.Red);
				ConnectButton.Visibility = Visibility.Visible;
				DisconnectButton.Visibility = Visibility.Hidden;
				InputField.IsReadOnly = true;
				SubmitButton.IsEnabled = false;
				isConnected = false;
			});
		}

		private void SubmitButton_Click(object sender, EventArgs e)
		{
			if(InputField.Text.StartsWith("/"))
			{
				string[] split = InputField.Text.Split(' ');
				if (split.Length < 2)
				{
					UpdateChatWindow("[Error] Missing Message Args", Colors.Red);
					return;
				}
				string targetUser = split[0].Substring(1);
				string message = InputField.Text.Substring(2 + targetUser.Length);
				client.TcpSendDataToServer(new Packets.PrivateMessagePacket(client.EncryptString(message), targetUser));
				InputField.Clear();
			}
			else
			{
				client.TcpSendDataToServer(new Packets.ChatMessagePacket(client.EncryptString(InputField.Text)));
				InputField.Clear();
			}
		}

		private void ConnectButton_Click(object sender, EventArgs e)
		{
			connectionForm = new ClientConnectionForm();
			connectionForm.ShowDialog();

			if(client.AttemptToConnect(connectionForm.GetIPEndPoint(), connectionForm.GetNickname()))
			{
				isConnected = true;
				ConnectButton.Visibility = Visibility.Hidden;
				DisconnectButton.Visibility = Visibility.Visible;
				InputField.IsReadOnly = false;
				SubmitButton.IsEnabled = true;
				NicknameChangeBox.IsEnabled = true;
				ChangeNicknameButton.IsEnabled = true;
			}
		}

		private void DisconnectButton_Click(object sender, EventArgs e)
		{
			UpdateChatWindow("[Server] You Have Left the Chat!", Colors.Red);
			client.Close();
			ConnectButton.Visibility = Visibility.Visible;
			DisconnectButton.Visibility = Visibility.Hidden;
			InputField.IsReadOnly = true;
			SubmitButton.IsEnabled = false;
			NicknameChangeBox.IsEnabled = false;
			ChangeNicknameButton.IsEnabled = false;
			isConnected = false;
		}

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			if (clientsOpen)
			{
				clientsOpen = false;
				ClientsConnectedBox.Visibility = Visibility.Hidden;
				ClientsLabel.Visibility = Visibility.Hidden;
			}

			menuOpened = !menuOpened;

			if (menuOpened)
			{
				this.Width = 600;
				ChangeNicknameButton.Visibility = Visibility.Visible;
				NicknameChangeBox.Visibility = Visibility.Visible;
				SettingsLabel.Visibility = Visibility.Visible;

				OpenSidePannel();
			}
			else
			{
				this.Width = 400;
				ChangeNicknameButton.Visibility = Visibility.Hidden;
				NicknameChangeBox.Visibility = Visibility.Hidden;
				SettingsLabel.Visibility = Visibility.Hidden;

				CloseSidePannel();
			}
		}

		private void ClientsButton_Click(object sender, RoutedEventArgs e)
		{
			if(menuOpened)
			{
				menuOpened = false;
				ChangeNicknameButton.Visibility = Visibility.Hidden;
				NicknameChangeBox.Visibility = Visibility.Hidden;
				SettingsLabel.Visibility = Visibility.Hidden;
			}

			clientsOpen = !clientsOpen;

			if (clientsOpen)
			{
				ClientsConnectedBox.Visibility = Visibility.Visible;
				ClientsLabel.Visibility = Visibility.Visible;

				OpenSidePannel();
			}
			else
			{
				ClientsConnectedBox.Visibility = Visibility.Hidden;
				ClientsLabel.Visibility = Visibility.Hidden;

				CloseSidePannel();
			}
		}

		private void OpenSidePannel()
		{
			this.Width = 600;
			ConnectButton.Margin = new Thickness(0, 0, 390, 370);
			DisconnectButton.Margin = new Thickness(0, 0, 390, 370);
			MenuButton.Margin = new Thickness(340, 0, 200, 370);
			ClientsButton.Margin = new Thickness(310, 0, 230, 370);
			MessageWindow.Margin = new Thickness(0, 30, 200, 30);
			InputField.Margin = new Thickness(0, 0, 310, 0);
			SubmitButton.Margin = new Thickness(0, 0, 200, 0);
		}

		public void CloseSidePannel()
		{
			this.Width = 400;
			ConnectButton.Margin = new Thickness(0, 0, 190, 370);
			DisconnectButton.Margin = new Thickness(0, 0, 190, 370);
			MenuButton.Margin = new Thickness(340, 0, 0, 370);
			ClientsButton.Margin = new Thickness(310, 0, 30, 370);
			MessageWindow.Margin = new Thickness(0, 30, 0, 30);
			InputField.Margin = new Thickness(0, 0, 110, 0);
			SubmitButton.Margin = new Thickness(0, 0, 0, 0);
		}

		private void ChangeNicknameButton_Click(object sender, RoutedEventArgs e)
		{
			if(NicknameChangeBox.Text == "")
			{
				UpdateChatWindow("[Error] You Can't Change Your Name to Nothing", Colors.Red);
				return;
			}

			client.TcpSendDataToServer(new Packets.NicknamePacket(NicknameChangeBox.Text));
			NicknameChangeBox.Text = "";
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.Close();
		}
	}
}
