namespace RPHexEditorMDIDemo
{
	partial class MDIDemoOption
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
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.cbLineCharacter = new System.Windows.Forms.CheckBox();
			this.cb_LineAddress = new System.Windows.Forms.CheckBox();
			this.button2 = new System.Windows.Forms.Button();
			this.fontDialog1 = new System.Windows.Forms.FontDialog();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.button3 = new System.Windows.Forms.Button();
			this.rpHexEditorUC1 = new RPHexEditor.RPHexEditorUC();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(111, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Test as Dialog Control";
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.Location = new System.Drawing.Point(313, 181);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "Cancel";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// cbLineCharacter
			// 
			this.cbLineCharacter.AutoSize = true;
			this.cbLineCharacter.Location = new System.Drawing.Point(153, 154);
			this.cbLineCharacter.Name = "cbLineCharacter";
			this.cbLineCharacter.Size = new System.Drawing.Size(121, 17);
			this.cbLineCharacter.TabIndex = 6;
			this.cbLineCharacter.Text = "View line characters";
			this.cbLineCharacter.UseVisualStyleBackColor = true;
			this.cbLineCharacter.CheckedChanged += new System.EventHandler(this.cbLineCharacter_CheckedChanged);
			// 
			// cb_LineAddress
			// 
			this.cb_LineAddress.AutoSize = true;
			this.cb_LineAddress.Location = new System.Drawing.Point(153, 131);
			this.cb_LineAddress.Name = "cb_LineAddress";
			this.cb_LineAddress.Size = new System.Drawing.Size(108, 17);
			this.cb_LineAddress.TabIndex = 5;
			this.cb_LineAddress.Text = "View line address";
			this.cb_LineAddress.UseVisualStyleBackColor = true;
			this.cb_LineAddress.CheckedChanged += new System.EventHandler(this.cb_LineAddress_CheckedChanged);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(11, 131);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(111, 23);
			this.button2.TabIndex = 4;
			this.button2.Text = "Set font";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// fontDialog1
			// 
			this.fontDialog1.FixedPitchOnly = true;
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(11, 160);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(111, 23);
			this.button3.TabIndex = 7;
			this.button3.Text = "Set color (Address)";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// rpHexEditorUC1
			// 
			this.rpHexEditorUC1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.rpHexEditorUC1.BytesPerLine = 8;
			this.rpHexEditorUC1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rpHexEditorUC1.Location = new System.Drawing.Point(11, 28);
			this.rpHexEditorUC1.Name = "rpHexEditorUC1";
			this.rpHexEditorUC1.Size = new System.Drawing.Size(377, 97);
			this.rpHexEditorUC1.TabIndex = 0;
			// 
			// MDIDemoOption
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button1;
			this.ClientSize = new System.Drawing.Size(400, 216);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.cbLineCharacter);
			this.Controls.Add(this.cb_LineAddress);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.rpHexEditorUC1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MDIDemoOption";
			this.Text = "MDIDemoOption";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private RPHexEditor.RPHexEditorUC rpHexEditorUC1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox cbLineCharacter;
		private System.Windows.Forms.CheckBox cb_LineAddress;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.FontDialog fontDialog1;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.Button button3;
	}
}