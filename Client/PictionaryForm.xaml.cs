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
using System.IO;
using System.Windows.Ink;
using System.Threading;

namespace Client
{
	/// <summary>
	/// Interaction logic for PictionaryForm.xaml
	/// </summary>
	public partial class PictionaryForm : Window
	{
        ClientManager clientManager;

		public PictionaryForm(ClientManager client)
		{
			InitializeComponent();

            clientManager = client;
			Thread thread = new Thread(() => { Run(); });
			thread.Start();
		}

		public void Run()
		{
			while(true)
			{
				if(clientManager.playingPictionary)
				{
					Thread.Sleep(1000);
					PaintCanvas.Dispatcher.Invoke(() =>
					{
						
						byte[] data;
						using (MemoryStream ms = new MemoryStream())
						{
							ms.Position = 0;
							PaintCanvas.Strokes.Save(ms);
							data = ms.ToArray();
						}

						clientManager.UdpSendDataToServer(new Packets.PictionaryPaintPacket(data));
					});
				}
			}
		}

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.PaintCanvas.Strokes.Clear();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
			byte[] data;
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Position = 0;
				PaintCanvas.Strokes.Save(ms);
				data = ms.ToArray();
			}
		
			clientManager.UdpSendDataToServer(new Packets.PictionaryPaintPacket(data));
		}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{

		}

		public void UpdateChatWindow(string message, Color color)
		{
			ChatBox.Dispatcher.Invoke(() =>
			{
				Run run = new Run(message);
				run.Foreground = new SolidColorBrush(color);
				Paragraph paragraph = new Paragraph(run);

				ChatBox.Document.Blocks.Add(paragraph);

				ChatBox.ScrollToEnd();
			});
		}

		public void UpdatePaintCanvas(byte[] data)
		{
			PaintCanvas.Dispatcher.Invoke(() =>
			{
				using (MemoryStream ms = new MemoryStream(data))
				{
					ms.Position = 0;
					StrokeCollection Strokes = new StrokeCollection(ms);
					PaintCanvas.Strokes = Strokes;
					ms.Close();
				}

			});
		}

		private void InputField_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter &&
				!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
				!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftShift)))
			{
				if (InputField.Text == "")
				{
					return;
				}
				clientManager.TcpSendDataToServer(new Packets.PictionaryChatMessagePacket(clientManager.EncryptString(InputField.Text)));
				
				InputField.Text = "";
			}
		}
	}
}
