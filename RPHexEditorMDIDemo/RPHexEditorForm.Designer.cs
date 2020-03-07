namespace RPHexEditorMDIDemo
{
    partial class RPHexEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RPHexEditorForm));
			this.rpHexEditor = new RPHexEditor.RPHexEditorUC();
			this.SuspendLayout();
			// 
			// rpHexEditor
			// 
			this.rpHexEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rpHexEditor.BackColor = System.Drawing.SystemColors.Control;
			this.rpHexEditor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.rpHexEditor.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rpHexEditor.ImeMode = System.Windows.Forms.ImeMode.On;
			this.rpHexEditor.Location = new System.Drawing.Point(0, 0);
			this.rpHexEditor.MinimumSize = new System.Drawing.Size(5, 5);
			this.rpHexEditor.Name = "rpHexEditor";
			this.rpHexEditor.Size = new System.Drawing.Size(477, 365);
			this.rpHexEditor.TabIndex = 0;
			this.rpHexEditor.ReadOnlyChanged += new System.EventHandler(this.rpHexEditor_ReadOnlyChanged);
			this.rpHexEditor.SelectionChanged += new System.EventHandler(this.rpHexEditor_SelectionChanged);
			this.rpHexEditor.InsertModeChanged += new System.EventHandler(this.rpHexEditor_InsertModeChanged);
			this.rpHexEditor.BytePositionChanged += new System.EventHandler(this.rpHexEditor_BytePositionChanged);
			// 
			// RPHexEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(477, 365);
			this.Controls.Add(this.rpHexEditor);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "RPHexEditorForm";
			this.Text = "RPForm";
			this.Activated += new System.EventHandler(this.RPHexEditorForm_Activated);
			this.Deactivate += new System.EventHandler(this.RPHexEditorForm_Deactivate);
			this.ResumeLayout(false);

        }

        #endregion

		private RPHexEditor.RPHexEditorUC rpHexEditor;

    }
}