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
using System.Runtime.Serialization.Formatters.Binary;

namespace Client
{
	/// <summary>
	/// Interaction logic for PictionaryForm.xaml
	/// </summary>
	public partial class PictionaryForm : Window
	{
        ClientManager clientManager;
		List<Point> linePoints;

		public PictionaryForm(ClientManager client)
		{
			InitializeComponent();
			PaintCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
			linePoints = new List<Point>();
			clientManager = client;
		}

		private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
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
		
			//clientManager.UdpSendDataToServer(new Packets.PictionaryPaintPacket(data));
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
					var mStream = new MemoryStream();
					var binFormatter = new BinaryFormatter();

					// Where 'objectBytes' is your byte array.
					mStream.Write(data, 0, data.Length);
					mStream.Position = 0;

					var myObject = binFormatter.Deserialize(mStream) as List<Point>;

					for(int i = 0; i < myObject.Count-1; i++)
					{
						Line l = new Line();
						l.X1 = myObject[i].X;
						l.Y1 = myObject[i].Y;
						l.X2 = myObject[i+1].X;
						l.Y2 = myObject[i+1].Y;

						l.Stroke = new SolidColorBrush(Colors.Black);
						l.StrokeThickness = 2;
						PaintCanvas.Children.Add(l);
					}
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

		private void PaintCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				linePoints.Add(e.GetPosition(PaintCanvas));

				if(linePoints.Count >= 40)
				{
					var binFormatter = new BinaryFormatter();
					var mStream = new MemoryStream();
					binFormatter.Serialize(mStream, linePoints);
					clientManager.UdpSendDataToServer(new Packets.PictionaryPaintPacket(mStream.ToArray()));
					linePoints.Clear();
				}
			}
		}
	}
}
