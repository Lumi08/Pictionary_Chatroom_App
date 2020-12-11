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
using System.Reflection;

namespace Client
{
	/// <summary>
	/// Interaction logic for PictionaryForm.xaml
	/// </summary>
	public partial class PictionaryForm : Window
	{
		private ClientManager clientManager;
		private List<double> linePointsX;
		private List<double> linePointsY;
		private Color penColor;

		public PictionaryForm(ClientManager client)
		{
			InitializeComponent();

			createGridOfColor();

			linePointsX = new List<double>();
			linePointsY = new List<double>();
			clientManager = client;
			penColor = Colors.Black;
			ChatBox.Foreground = new SolidColorBrush(Colors.Gray);
			ChatBox.AppendText("Welcome to Pictionary!");
		}

		private void createGridOfColor()
		{
			PropertyInfo[] props = typeof(Brushes).GetProperties(BindingFlags.Public |
												  BindingFlags.Static);
			// Create individual items
			foreach (PropertyInfo p in props)
			{
				Button b = new Button();
				b.Background = (SolidColorBrush)p.GetValue(null, null);
				b.Foreground = Brushes.Transparent;
				b.BorderBrush = Brushes.Transparent;
				b.Click += new RoutedEventHandler(b_Click);
				this.colorGrid.Children.Add(b);
			}
		}
		private void b_Click(object sender, RoutedEventArgs e)
		{
			SolidColorBrush sb = (SolidColorBrush)(sender as Button).Background;
			penColor = sb.Color;
			PaintCanvas.DefaultDrawingAttributes.Color = sb.Color;
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

		public void UpdatePaintCanvas(double[] xPositions, double[] yPositions, float[] penColor)
		{
			PaintCanvas.Dispatcher.Invoke(() =>
			{ 
				for (int i = 0; i < xPositions.Length - 1; i++)
				{
					Line l = new Line();
					l.X1 = xPositions[i];
					l.Y1 = yPositions[i];
					l.X2 = xPositions[i + 1];
					l.Y2 = yPositions[i + 1];

					if (penColor != null)
					{
						l.Stroke = new SolidColorBrush(Color.FromScRgb(penColor[3], penColor[0], penColor[1], penColor[2]));
					}
					else
					{
						l.Stroke = new SolidColorBrush(Colors.Black);
					}
					l.StrokeThickness = 2;
					PaintCanvas.Children.Add(l);
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
			//Thread sendDataThread = new Thread(() => { clientManager.UdpSendDataToServer(new Packets.PictionaryPaintPacket(linePointsX, linePointsY); });

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				linePointsX.Add(e.GetPosition(PaintCanvas).X);
				linePointsY.Add(e.GetPosition(PaintCanvas).Y);

				if (linePointsX.Count >= 20)
				{
					Packets.PictionaryPaintPacket packet = new Packets.PictionaryPaintPacket(linePointsX, linePointsY);
					packet.SetPenColour(penColor.ScR, penColor.ScG, penColor.ScB, penColor.ScA);
					clientManager.UdpSendDataToServer(packet);
					linePointsX.Clear();
					linePointsY.Clear();
				}
			}

			if (e.LeftButton == MouseButtonState.Released)
			{
				if (linePointsX.Count != 0)
				{
					Packets.PictionaryPaintPacket packet = new Packets.PictionaryPaintPacket(linePointsX, linePointsY);
					packet.SetPenColour(penColor.ScR, penColor.ScG, penColor.ScB, penColor.ScA);
					clientManager.UdpSendDataToServer(packet);
					linePointsX.Clear();
					linePointsY.Clear();
				}
			}
		}

		public void ClearCanvas()
		{
			PaintCanvas.Dispatcher.Invoke(() =>
			{
				this.PaintCanvas.Strokes.Clear();
				this.PaintCanvas.Children.Clear();
			});
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			clientManager.UdpSendDataToServer(new Packets.PictionaryClearCanvasPacket());
		}
	}
}

