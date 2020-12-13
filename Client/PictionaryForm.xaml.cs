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
		private PaintTool paintTool;
		private static readonly int COLUMS = 5;

		private Point lastRecievedPoint;
		private bool newLine;

		private enum PaintTool
		{
			Pen,
			Eraser,
			Fill
		}

		public PictionaryForm(ClientManager client)
		{
			InitializeComponent();
			ColorComboBox.SelectedIndex = 7;

			linePointsX = new List<double>();
			linePointsY = new List<double>();
			clientManager = client;
			newLine = true;
			paintTool = PaintTool.Pen;
			penColor = Colors.Black;
			ColorComboBox.ItemsSource = typeof(Colors).GetProperties();
			ChatBox.Foreground = new SolidColorBrush(Colors.Gray);
			PaintCanvas.DefaultDrawingAttributes.Width = 2;
			ChatBox.AppendText("Welcome to Pictionary!");
		}

		private void table_Loaded(object sender, RoutedEventArgs e)
		{
			Grid grid = sender as Grid;
			if (grid != null)
			{
				if (grid.RowDefinitions.Count == 0)
				{
					for (int r = 0; r <= ColorComboBox.Items.Count / COLUMS; r++)
					{
						grid.RowDefinitions.Add(new RowDefinition());
					}
				}
				if (grid.ColumnDefinitions.Count == 0)
				{
					for (int c = 0; c < Math.Min(ColorComboBox.Items.Count, COLUMS); c++)
					{
						grid.ColumnDefinitions.Add(new ColumnDefinition());
					}
				}
				for (int i = 0; i < grid.Children.Count; i++)
				{
					Grid.SetColumn(grid.Children[i], i % COLUMS);
					Grid.SetRow(grid.Children[i], i / COLUMS);
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			clientManager.TcpSendDataToServer(new Packets.GameConnectionPacket(Packets.Packet.GameType.Pictionary));
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

		public void UpdatePaintCanvas(double[] xPositions, double[] yPositions, float[] penColor, bool sameLine)
		{
			PaintCanvas.Dispatcher.Invoke(() =>
			{
				if (sameLine)
				{
					Line connectionLine = new Line();
					connectionLine.X1 = lastRecievedPoint.X;
					connectionLine.Y1 = lastRecievedPoint.Y;
					connectionLine.X2 = xPositions[0];
					connectionLine.Y2 = yPositions[0];
					if (penColor != null)
					{
						connectionLine.Stroke = new SolidColorBrush(Color.FromScRgb(penColor[3], penColor[0], penColor[1], penColor[2]));
					}
					else
					{
						connectionLine.Stroke = new SolidColorBrush(Colors.Black);
					}
					connectionLine.StrokeThickness = 2;
					PaintCanvas.Children.Add(connectionLine);
				}

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
				lastRecievedPoint = new Point(xPositions[xPositions.Length-1], yPositions[yPositions.Length-1]);
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
				linePointsX.Add(e.GetPosition(PaintCanvas).X);
				linePointsY.Add(e.GetPosition(PaintCanvas).Y);

				if (linePointsX.Count >= 20)
				{
					Packets.PictionaryPaintPacket packet = new Packets.PictionaryPaintPacket(linePointsX, linePointsY, newLine);
					newLine = true;
					if(paintTool == PaintTool.Eraser)
					{
						packet.SetPenColour(Colors.White.ScR, Colors.White.ScG, Colors.White.ScB, Colors.White.ScA);
					}
					if (paintTool == PaintTool.Pen)
					{
						packet.SetPenColour(penColor.ScR, penColor.ScG, penColor.ScB, penColor.ScA);
					}
					clientManager.UdpSendDataToServer(packet);
					linePointsX.Clear();
					linePointsY.Clear();
				}
			}

			if (e.LeftButton == MouseButtonState.Released)
			{
				newLine = false;
				if (linePointsX.Count != 0)
				{
					Packets.PictionaryPaintPacket packet = new Packets.PictionaryPaintPacket(linePointsX, linePointsY, true);
					if (paintTool == PaintTool.Eraser)
					{
						packet.SetPenColour(Colors.White.ScR, Colors.White.ScG, Colors.White.ScB, Colors.White.ScA);
					}
					if (paintTool == PaintTool.Pen)
					{
						packet.SetPenColour(penColor.ScR, penColor.ScG, penColor.ScB, penColor.ScA);
					}
					clientManager.UdpSendDataToServer(packet);
					linePointsX.Clear();
					linePointsY.Clear();
				}
			}
		}

		public void SetUpDrawer(bool isDrawer)
		{
			if(!isDrawer)
			{
				PaintCanvas.IsEnabled = false;
				ColorComboBox.IsEnabled = false;
				ColorComboBox.Visibility = Visibility.Hidden;
				ToolsComboBox.IsEnabled = false;
				ToolsComboBox.Visibility = Visibility.Hidden;
				ClearButton.IsEnabled = false;
				ClearButton.Visibility = Visibility.Hidden;
				ItemToDrawLabel.Visibility = Visibility.Hidden;
				InputField.IsReadOnly = false;
			}
			else
			{
				PaintCanvas.IsEnabled = true;
				ColorComboBox.IsEnabled = true;
				ColorComboBox.Visibility = Visibility.Visible;
				ToolsComboBox.IsEnabled = true;
				ToolsComboBox.Visibility = Visibility.Visible;
				ClearButton.IsEnabled = true;
				ClearButton.Visibility = Visibility.Visible;
				ItemToDrawLabel.Visibility = Visibility.Visible;
				InputField.IsReadOnly = true;
			}
		}

		public void RecievedWordToDraw(string wordToDraw)
		{
			ItemToDrawLabel.Dispatcher.Invoke(() => { 
				ItemToDrawLabel.Content = "Drawing: " + wordToDraw;
			});
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

		private void ComboColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			PropertyInfo[] props = typeof(Brushes).GetProperties(BindingFlags.Public |
												  BindingFlags.Static);

			SolidColorBrush temp = (SolidColorBrush)(props[(sender as ComboBox).SelectedIndex].GetValue(null, null));
			penColor = temp.Color;
			PaintCanvas.DefaultDrawingAttributes.Color = penColor;

		}

		private void ToolsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if((sender as ComboBox).SelectedIndex == 0)
			{
				paintTool = PaintTool.Pen;
				PaintCanvas.EditingMode = InkCanvasEditingMode.Ink;
			}
			if ((sender as ComboBox).SelectedIndex == 1)
			{
				paintTool = PaintTool.Eraser;
				PaintCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
			}
		}
	}
}

