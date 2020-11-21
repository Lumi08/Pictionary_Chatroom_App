﻿using System;
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
using System.Windows.Shapes;

namespace Client
{
	/// <summary>
	/// Interaction logic for ClientForm.xaml
	/// </summary>
	public partial class ClientForm : Window
	{
		private ClientManager client;
		public bool isConnected;
		public bool menuOpened = false;

		public ClientForm(ClientManager client)
		{
			InitializeComponent();
			this.client = client;
		}

		public void UpdateChatWindow(string message)
		{
			MessageWindow.Dispatcher.Invoke(() =>
			{
				Run run = new Run(message);
				run.Foreground = new SolidColorBrush(Colors.Black);
				Paragraph paragraph = new Paragraph(run);

				MessageWindow.Document.Blocks.Add(paragraph);

				MessageWindow.ScrollToEnd();

			});
		}

		private void SubmitButton_Click(object sender, EventArgs e)
		{
			client.SendDataToServer(new Packets.ChatMessagePacket(InputField.Text));
			InputField.Clear();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.Close();
		}

		private void ConnectButton_Click(object sender, EventArgs e)
		{
			if(client.AttemptToConnect())
			{
				isConnected = true;
				ConnectButton.Visibility = Visibility.Hidden;
				DisconnectButton.Visibility = Visibility.Visible;
				InputField.IsReadOnly = false;
				SubmitButton.IsEnabled = true;
			}
		}

		private void DisconnectButton_Click(object sender, EventArgs e)
		{
			client.Close();
			ConnectButton.Visibility = Visibility.Visible;
			DisconnectButton.Visibility = Visibility.Hidden;
			InputField.IsReadOnly = true;
			SubmitButton.IsEnabled = false;
			isConnected = false;
		}

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			menuOpened = !menuOpened;

			if (menuOpened)
			{
				this.Width = 600;
				ConnectButton.Margin = new Thickness(0, 0, 390, 370);
				DisconnectButton.Margin = new Thickness(0, 0, 390, 370);
				MenuButton.Margin = new Thickness(190, 0, 200, 370);
				MessageWindow.Margin = new Thickness(0, 30, 200, 30);
				InputField.Margin = new Thickness(0, 0, 310, 0);
				SubmitButton.Margin = new Thickness(0, 0, 200, 0);
			}
			else
			{
				this.Width = 400;
				ConnectButton.Margin = new Thickness(0,0,190,370);
				DisconnectButton.Margin = new Thickness(0, 0, 190, 370);
				MenuButton.Margin = new Thickness(190, 0, 0, 370);
				MessageWindow.Margin = new Thickness(0, 30, 0, 30);
				InputField.Margin = new Thickness(0, 0, 110, 0);
				SubmitButton.Margin = new Thickness(0, 0, 0, 0);
			}
		}
	}
}
