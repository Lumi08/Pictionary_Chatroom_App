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
using System.Windows.Shapes;

namespace Client
{
	/// <summary>
	/// Interaction logic for ClientForm.xaml
	/// </summary>
	public partial class ClientForm : Window
	{
		private ClientManager client;

		public ClientForm(ClientManager client)
		{
			InitializeComponent();
			this.client = client;
		}

		/*public ClientForm()
		{
			Program.Start();
		}*/

		public void UpdateChatWindow(string message)
		{
			MessageWindow.Dispatcher.Invoke(() =>
			{

				MessageWindow.Text += message + Environment.NewLine;

				MessageWindow.ScrollToEnd();

			});
		}

		private void SubmitButton_Click(Object sender, EventArgs e)
		{
			client.SendDataToServer("/client.message " + InputField.Text);
			InputField.Clear();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.Close();
		}
	}
}
