// Copyright (c) 2012, Paul Groke
// For conditions of distribution and use, see copyright notice in LICENSE.txt

namespace FlushMark
{
	partial class FlushMarkForm
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
			this.m_goButton = new System.Windows.Forms.Button();
			this.m_driveComboBox = new System.Windows.Forms.ComboBox();
			this.m_driveLabel = new System.Windows.Forms.Label();
			this.m_messageTextBox = new System.Windows.Forms.TextBox();
			this.m_testSizeComboBox = new System.Windows.Forms.ComboBox();
			this.m_testSizeLabel = new System.Windows.Forms.Label();
			this.m_clearButton = new System.Windows.Forms.Button();
			this.m_testModeComboBox = new System.Windows.Forms.ComboBox();
			this.m_testModeLabel = new System.Windows.Forms.Label();
			this.m_flushFrequencyLabel = new System.Windows.Forms.Label();
			this.m_flushFrequencyComboBox = new System.Windows.Forms.ComboBox();
			this.m_pageSizeLabel = new System.Windows.Forms.Label();
			this.m_pageSizeComboBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// m_goButton
			// 
			this.m_goButton.Location = new System.Drawing.Point(82, 123);
			this.m_goButton.Name = "m_goButton";
			this.m_goButton.Size = new System.Drawing.Size(75, 25);
			this.m_goButton.TabIndex = 0;
			this.m_goButton.Text = "Go!";
			this.m_goButton.UseVisualStyleBackColor = true;
			this.m_goButton.Click += new System.EventHandler(this.m_goButton_Click);
			// 
			// m_driveComboBox
			// 
			this.m_driveComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_driveComboBox.FormattingEnabled = true;
			this.m_driveComboBox.Location = new System.Drawing.Point(82, 12);
			this.m_driveComboBox.Name = "m_driveComboBox";
			this.m_driveComboBox.Size = new System.Drawing.Size(50, 21);
			this.m_driveComboBox.TabIndex = 2;
			// 
			// m_driveLabel
			// 
			this.m_driveLabel.Location = new System.Drawing.Point(5, 12);
			this.m_driveLabel.Name = "m_driveLabel";
			this.m_driveLabel.Size = new System.Drawing.Size(71, 21);
			this.m_driveLabel.TabIndex = 3;
			this.m_driveLabel.Text = "Drive";
			this.m_driveLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_messageTextBox
			// 
			this.m_messageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_messageTextBox.Location = new System.Drawing.Point(12, 154);
			this.m_messageTextBox.Multiline = true;
			this.m_messageTextBox.Name = "m_messageTextBox";
			this.m_messageTextBox.ReadOnly = true;
			this.m_messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_messageTextBox.Size = new System.Drawing.Size(453, 209);
			this.m_messageTextBox.TabIndex = 4;
			// 
			// m_testSizeComboBox
			// 
			this.m_testSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_testSizeComboBox.FormattingEnabled = true;
			this.m_testSizeComboBox.Items.AddRange(new object[] {
            "256M",
            "512M",
            "1G (Default)",
            "2G"});
			this.m_testSizeComboBox.Location = new System.Drawing.Point(235, 12);
			this.m_testSizeComboBox.Name = "m_testSizeComboBox";
			this.m_testSizeComboBox.Size = new System.Drawing.Size(145, 21);
			this.m_testSizeComboBox.TabIndex = 6;
			// 
			// m_testSizeLabel
			// 
			this.m_testSizeLabel.Location = new System.Drawing.Point(138, 12);
			this.m_testSizeLabel.Name = "m_testSizeLabel";
			this.m_testSizeLabel.Size = new System.Drawing.Size(91, 21);
			this.m_testSizeLabel.TabIndex = 7;
			this.m_testSizeLabel.Text = "Test size";
			this.m_testSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_clearButton
			// 
			this.m_clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_clearButton.Location = new System.Drawing.Point(366, 123);
			this.m_clearButton.Name = "m_clearButton";
			this.m_clearButton.Size = new System.Drawing.Size(99, 25);
			this.m_clearButton.TabIndex = 8;
			this.m_clearButton.Text = "Clear log";
			this.m_clearButton.UseVisualStyleBackColor = true;
			this.m_clearButton.Click += new System.EventHandler(this.m_clearButton_Click);
			// 
			// m_testModeComboBox
			// 
			this.m_testModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_testModeComboBox.FormattingEnabled = true;
			this.m_testModeComboBox.Items.AddRange(new object[] {
            "Random (Default)",
            "Linear"});
			this.m_testModeComboBox.Location = new System.Drawing.Point(235, 66);
			this.m_testModeComboBox.Name = "m_testModeComboBox";
			this.m_testModeComboBox.Size = new System.Drawing.Size(145, 21);
			this.m_testModeComboBox.TabIndex = 9;
			// 
			// m_testModeLabel
			// 
			this.m_testModeLabel.Location = new System.Drawing.Point(121, 66);
			this.m_testModeLabel.Name = "m_testModeLabel";
			this.m_testModeLabel.Size = new System.Drawing.Size(108, 21);
			this.m_testModeLabel.TabIndex = 10;
			this.m_testModeLabel.Text = "Test mode";
			this.m_testModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_flushFrequencyLabel
			// 
			this.m_flushFrequencyLabel.Location = new System.Drawing.Point(121, 93);
			this.m_flushFrequencyLabel.Name = "m_flushFrequencyLabel";
			this.m_flushFrequencyLabel.Size = new System.Drawing.Size(108, 21);
			this.m_flushFrequencyLabel.TabIndex = 12;
			this.m_flushFrequencyLabel.Text = "Flush frequency";
			this.m_flushFrequencyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_flushFrequencyComboBox
			// 
			this.m_flushFrequencyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_flushFrequencyComboBox.FormattingEnabled = true;
			this.m_flushFrequencyComboBox.Items.AddRange(new object[] {
            "0 (None)",
            "1 (Default)",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
			this.m_flushFrequencyComboBox.Location = new System.Drawing.Point(235, 93);
			this.m_flushFrequencyComboBox.Name = "m_flushFrequencyComboBox";
			this.m_flushFrequencyComboBox.Size = new System.Drawing.Size(145, 21);
			this.m_flushFrequencyComboBox.TabIndex = 11;
			// 
			// m_pageSizeLabel
			// 
			this.m_pageSizeLabel.Location = new System.Drawing.Point(121, 39);
			this.m_pageSizeLabel.Name = "m_pageSizeLabel";
			this.m_pageSizeLabel.Size = new System.Drawing.Size(108, 21);
			this.m_pageSizeLabel.TabIndex = 14;
			this.m_pageSizeLabel.Text = "Page size";
			this.m_pageSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_pageSizeComboBox
			// 
			this.m_pageSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_pageSizeComboBox.FormattingEnabled = true;
			this.m_pageSizeComboBox.Items.AddRange(new object[] {
            "512B",
            "1K",
            "2K",
            "4K (Default)",
            "8K",
            "16K",
            "32K",
            "64K",
            "128K",
            "256K",
            "512K",
            "1M"});
			this.m_pageSizeComboBox.Location = new System.Drawing.Point(235, 39);
			this.m_pageSizeComboBox.Name = "m_pageSizeComboBox";
			this.m_pageSizeComboBox.Size = new System.Drawing.Size(145, 21);
			this.m_pageSizeComboBox.TabIndex = 13;
			// 
			// FlushMarkForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(477, 375);
			this.Controls.Add(this.m_pageSizeLabel);
			this.Controls.Add(this.m_pageSizeComboBox);
			this.Controls.Add(this.m_flushFrequencyLabel);
			this.Controls.Add(this.m_flushFrequencyComboBox);
			this.Controls.Add(this.m_testModeLabel);
			this.Controls.Add(this.m_testModeComboBox);
			this.Controls.Add(this.m_clearButton);
			this.Controls.Add(this.m_testSizeLabel);
			this.Controls.Add(this.m_testSizeComboBox);
			this.Controls.Add(this.m_messageTextBox);
			this.Controls.Add(this.m_driveLabel);
			this.Controls.Add(this.m_driveComboBox);
			this.Controls.Add(this.m_goButton);
			this.MinimumSize = new System.Drawing.Size(408, 304);
			this.Name = "FlushMarkForm";
			this.Text = "FlushMark";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FlushMarkForm_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_goButton;
		private System.Windows.Forms.ComboBox m_driveComboBox;
		private System.Windows.Forms.Label m_driveLabel;
		private System.Windows.Forms.TextBox m_messageTextBox;
		private System.Windows.Forms.ComboBox m_testSizeComboBox;
		private System.Windows.Forms.Label m_testSizeLabel;
		private System.Windows.Forms.Button m_clearButton;
		private System.Windows.Forms.ComboBox m_testModeComboBox;
		private System.Windows.Forms.Label m_testModeLabel;
		private System.Windows.Forms.Label m_flushFrequencyLabel;
		private System.Windows.Forms.ComboBox m_flushFrequencyComboBox;
		private System.Windows.Forms.Label m_pageSizeLabel;
		private System.Windows.Forms.ComboBox m_pageSizeComboBox;
	}
}

