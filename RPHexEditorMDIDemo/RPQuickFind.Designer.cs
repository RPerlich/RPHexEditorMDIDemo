namespace RPHexEditorMDIDemo
{
	partial class RPQuickFind
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RPQuickFind));
			this.btnClose = new System.Windows.Forms.Button();
			this.tbSearchText = new System.Windows.Forms.TextBox();
			this.btnFind = new System.Windows.Forms.Button();
			this.toolTipFind = new System.Windows.Forms.ToolTip(this.components);
			this.toolTipClose = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// btnClose
			// 
			this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
			this.btnClose.Location = new System.Drawing.Point(152, 2);
			this.btnClose.Margin = new System.Windows.Forms.Padding(0);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(23, 22);
			this.btnClose.TabIndex = 0;
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// tbSearchText
			// 
			this.tbSearchText.Location = new System.Drawing.Point(5, 3);
			this.tbSearchText.Name = "tbSearchText";
			this.tbSearchText.Size = new System.Drawing.Size(121, 20);
			this.tbSearchText.TabIndex = 0;
			// 
			// btnFind
			// 
			this.btnFind.Image = ((System.Drawing.Image)(resources.GetObject("btnFind.Image")));
			this.btnFind.Location = new System.Drawing.Point(129, 2);
			this.btnFind.Margin = new System.Windows.Forms.Padding(0);
			this.btnFind.Name = "btnFind";
			this.btnFind.Size = new System.Drawing.Size(23, 22);
			this.btnFind.TabIndex = 1;
			this.btnFind.UseVisualStyleBackColor = true;
			this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
			// 
			// RPQuickFind
			// 
			this.AcceptButton = this.btnFind;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(181, 26);
			this.ControlBox = false;
			this.Controls.Add(this.btnFind);
			this.Controls.Add(this.tbSearchText);
			this.Controls.Add(this.btnClose);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(50, 16);
			this.Name = "RPQuickFind";
			this.Padding = new System.Windows.Forms.Padding(2);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.TextBox tbSearchText;
		private System.Windows.Forms.Button btnFind;
		private System.Windows.Forms.ToolTip toolTipFind;
		private System.Windows.Forms.ToolTip toolTipClose;
	}
}