﻿namespace RPHexEditorMDIDemo
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
			this.rpHexEditorUC1 = new RPHexEditor.RPHexEditorUC();
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// rpHexEditorUC1
			// 
			this.rpHexEditorUC1.BackColor = System.Drawing.SystemColors.Control;
			this.rpHexEditorUC1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.rpHexEditorUC1.BytesPerLine = 8;
			this.rpHexEditorUC1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rpHexEditorUC1.Location = new System.Drawing.Point(11, 28);
			this.rpHexEditorUC1.Name = "rpHexEditorUC1";
			this.rpHexEditorUC1.Size = new System.Drawing.Size(370, 97);
			this.rpHexEditorUC1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
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
			// MDIDemoOption
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button1;
			this.ClientSize = new System.Drawing.Size(400, 216);
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
	}
}