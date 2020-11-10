namespace Client
{
	partial class MainWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ChatTextBox = new System.Windows.Forms.TextBox();
			this.InputMessageTextBox = new System.Windows.Forms.TextBox();
			this.SendMessageButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ChatTextBox
			// 
			this.ChatTextBox.Location = new System.Drawing.Point(12, 12);
			this.ChatTextBox.Multiline = true;
			this.ChatTextBox.Name = "ChatTextBox";
			this.ChatTextBox.Size = new System.Drawing.Size(608, 400);
			this.ChatTextBox.TabIndex = 0;
			// 
			// InputMessageTextBox
			// 
			this.InputMessageTextBox.Location = new System.Drawing.Point(12, 418);
			this.InputMessageTextBox.Name = "InputMessageTextBox";
			this.InputMessageTextBox.Size = new System.Drawing.Size(498, 20);
			this.InputMessageTextBox.TabIndex = 1;
			// 
			// SendMessageButton
			// 
			this.SendMessageButton.Location = new System.Drawing.Point(516, 416);
			this.SendMessageButton.Name = "SendMessageButton";
			this.SendMessageButton.Size = new System.Drawing.Size(104, 23);
			this.SendMessageButton.TabIndex = 2;
			this.SendMessageButton.Text = "Send Message";
			this.SendMessageButton.UseVisualStyleBackColor = true;
			this.SendMessageButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.SendMessageButton);
			this.Controls.Add(this.InputMessageTextBox);
			this.Controls.Add(this.ChatTextBox);
			this.Name = "MainWindow";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ChatTextBox;
		private System.Windows.Forms.TextBox InputMessageTextBox;
		private System.Windows.Forms.Button SendMessageButton;
	}
}

