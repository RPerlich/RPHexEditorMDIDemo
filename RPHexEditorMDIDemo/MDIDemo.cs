using System;
using System.Windows.Forms;

namespace RPHexEditorMDIDemo
{
	public partial class MDIDemo : Form
	{
		private Timer _timer = null;

		public MDIDemo()
		{
			InitializeComponent();
			this.Size = new System.Drawing.Size(1024, 786);

			_timer = new Timer();
			_timer.Tick += new EventHandler(OnClientStateUpdate);
			_timer.Interval = 500;
			_timer.Start();

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				this._timer.Stop();
				this._timer.Dispose();
			}

			base.Dispose(disposing);
		}

		public ToolStripStatusLabel GetStatusBarControl_INS
		{
			get { return this.toolStripStatusINS; }
		}

		public ToolStripStatusLabel GetStatusBarControl_RW
		{
			get { return this.toolStripStatusRW; }
		}

		public ToolStripStatusLabel GetStatusBarControl_Position
		{
			get { return this.toolStripStatusPosition; }
		}

		public ToolStripMenuItem GetTSM_Undo
		{
			get { return this.undoToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_Copy
		{
			get { return this.copyToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_Cut
		{
			get { return this.cutToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_Paste
		{
			get { return this.pasteToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_SelectAll
		{
			get { return this.selectAllToolStripMenuItem; }
		}

		public ToolStripButton GetTSB_Copy
		{
			get { return this.copyToolStripButton; }
		}

		public ToolStripButton GetTSB_Cut
		{
			get { return this.cutToolStripButton; }
		}

		public ToolStripButton GetTSB_Paste
		{
			get { return this.pasteToolStripButton; }
		}

		private void ShowNewForm(object sender, EventArgs e)
		{
			RPHexEditorForm childForm = new RPHexEditorForm();
			childForm.MdiParent = this;
			childForm.NewFile();
			childForm.Text = childForm.GetFileName();
			childForm.Show();
		}

		private void OpenFile(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			openFileDialog.ShowReadOnly = true;
			openFileDialog.Filter = "All Files (*.*)|*.*|Bin Files (*.bin)|*.bin|Text Files (*.txt)|*.txt";

			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				RPHexEditorForm childForm = new RPHexEditorForm();
				childForm.MdiParent = this;
				childForm.Text = openFileDialog.FileName;
				// TODO: Check result
				childForm.LoadFile(openFileDialog.FileName, openFileDialog.ReadOnlyChecked);
				childForm.Show();
			}
		}

		private void SaveFile(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;
			if (child != null)
			{
				if (child.IsChanged())
					child.CommitChanges();
			}
		}

		private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
				child.CommitChangesAs();
		}

		private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
			{
				child.Undo();
			}
		}

		private void redoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void CutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;
			child.Cut();
		}

		private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;
			child.Copy();
		}

		private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;
			child.Paste();
		}

		private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			toolStrip.Visible = toolBarToolStripMenuItem.Checked;
		}

		private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			statusStrip.Visible = statusBarToolStripMenuItem.Checked;
		}

		private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.Cascade);
		}

		private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.TileVertical);
		}

		private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.TileHorizontal);
		}

		private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.ArrangeIcons);
		}

		private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (Form childForm in MdiChildren)
			{
				childForm.Close();
			}
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
				child.SelectAll();
		}

		private void ToggleStatusBarControl_INS()
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
			{
				InsertKeyMode ikm = child.GetInsertMode;

				if (ikm == InsertKeyMode.Insert)
					child.SetInsertMode(InsertKeyMode.Overwrite);
				else
					child.SetInsertMode(InsertKeyMode.Insert);
			}
		}

		private void toolStripStatusINS_DoubleClick(object sender, EventArgs e)
		{
			ToggleStatusBarControl_INS();
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MDIDemoOption frm = new MDIDemoOption();
			frm.ShowDialog();
		}

		private void OnClientStateUpdate(Object myObject, EventArgs myEventArgs)
		{
			if (MdiChildren.Length == 0)
			{
				this.copyToolStripMenuItem.Enabled = false;
				this.cutToolStripMenuItem.Enabled = false;
				this.pasteToolStripMenuItem.Enabled = false;
				this.selectAllToolStripMenuItem.Enabled = false;
				this.copyToolStripButton.Enabled = false;
				this.cutToolStripButton.Enabled = false;
				this.pasteToolStripButton.Enabled = false;
				this.undoToolStripMenuItem.Enabled = false;
				this.redoToolStripMenuItem.Enabled = false;
			}

			this.saveToolStripMenuItem.Enabled = MdiChildren.Length > 0;
			this.saveAsToolStripMenuItem.Enabled = MdiChildren.Length > 0;
			this.saveToolStripButton.Enabled = this.saveAsToolStripMenuItem.Enabled;
		}

	}
}
