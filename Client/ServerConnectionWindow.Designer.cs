namespace Client
{
	partial class ServerConnectionWindow
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
			this.ConnectButton = new System.Windows.Forms.Button();
			this.NicknameInputBox = new System.Windows.Forms.TextBox();
			this.IpInputBox = new System.Windows.Forms.TextBox();
			this.PortInputBox = new System.Windows.Forms.TextBox();
			this.IpLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.ErrorLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ConnectButton
			// 
			this.ConnectButton.Location = new System.Drawing.Point(99, 133);
			this.ConnectButton.Name = "ConnectButton";
			this.ConnectButton.Size = new System.Drawing.Size(75, 23);
			this.ConnectButton.TabIndex = 0;
			this.ConnectButton.Text = "Connect";
			this.ConnectButton.UseVisualStyleBackColor = true;
			this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
			// 
			// NicknameInputBox
			// 
			this.NicknameInputBox.Location = new System.Drawing.Point(63, 55);
			this.NicknameInputBox.Name = "NicknameInputBox";
			this.NicknameInputBox.Size = new System.Drawing.Size(199, 20);
			this.NicknameInputBox.TabIndex = 1;
			// 
			// IpInputBox
			// 
			this.IpInputBox.Location = new System.Drawing.Point(63, 81);
			this.IpInputBox.Name = "IpInputBox";
			this.IpInputBox.Size = new System.Drawing.Size(199, 20);
			this.IpInputBox.TabIndex = 2;
			// 
			// PortInputBox
			// 
			this.PortInputBox.Location = new System.Drawing.Point(63, 107);
			this.PortInputBox.Name = "PortInputBox";
			this.PortInputBox.Size = new System.Drawing.Size(199, 20);
			this.PortInputBox.TabIndex = 3;
			// 
			// IpLabel
			// 
			this.IpLabel.AutoSize = true;
			this.IpLabel.Location = new System.Drawing.Point(2, 84);
			this.IpLabel.Name = "IpLabel";
			this.IpLabel.Size = new System.Drawing.Size(17, 13);
			this.IpLabel.TabIndex = 4;
			this.IpLabel.Text = "IP";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(80, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(119, 25);
			this.label1.TabIndex = 5;
			this.label1.Text = "Chat Room";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(2, 110);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(26, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Port";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(2, 58);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(55, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Nickname";
			// 
			// ErrorLabel
			// 
			this.ErrorLabel.AutoSize = true;
			this.ErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.ErrorLabel.Location = new System.Drawing.Point(60, 159);
			this.ErrorLabel.Name = "ErrorLabel";
			this.ErrorLabel.Size = new System.Drawing.Size(0, 13);
			this.ErrorLabel.TabIndex = 8;
			// 
			// ServerConnectionWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(274, 181);
			this.Controls.Add(this.ErrorLabel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.IpLabel);
			this.Controls.Add(this.PortInputBox);
			this.Controls.Add(this.IpInputBox);
			this.Controls.Add(this.NicknameInputBox);
			this.Controls.Add(this.ConnectButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "ServerConnectionWindow";
			this.Text = "ConnectionForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ConnectButton;
		private System.Windows.Forms.TextBox NicknameInputBox;
		private System.Windows.Forms.TextBox IpInputBox;
		private System.Windows.Forms.TextBox PortInputBox;
		private System.Windows.Forms.Label IpLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label ErrorLabel;
	}
}