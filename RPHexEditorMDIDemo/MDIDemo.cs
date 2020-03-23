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
			Size = new System.Drawing.Size(1024, 786);

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

				_timer.Stop();
				_timer.Dispose();
			}

			base.Dispose(disposing);
		}

		public ToolStripStatusLabel GetStatusBarControl_INS
		{
			get { return toolStripStatusINS; }
		}

		public ToolStripStatusLabel GetStatusBarControl_RW
		{
			get { return toolStripStatusRW; }
		}

		public ToolStripStatusLabel GetStatusBarControl_Position
		{
			get { return toolStripStatusPosition; }
		}

		public ToolStripMenuItem GetTSM_Undo
		{
			get { return undoToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_Copy
		{
			get { return copyToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_Cut
		{
			get { return cutToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_Paste
		{
			get { return pasteToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_Find
		{
			get { return findToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_FindNext
		{
			get { return findNextToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_SelectAll
		{
			get { return selectAllToolStripMenuItem; }
		}

		public ToolStripMenuItem GetTSM_GoTo
		{
			get { return goToToolStripMenuItem; }
		}

		public ToolStripButton GetTSB_Undo
		{
			get { return undoToolStripButton; }
		}

		public ToolStripButton GetTSB_Copy
		{
			get { return copyToolStripButton; }
		}

		public ToolStripButton GetTSB_Cut
		{
			get { return cutToolStripButton; }
		}

		public ToolStripButton GetTSB_Paste
		{
			get { return pasteToolStripButton; }
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

		private void PrintToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
				child.Print();
		}

		private void PrintPreviewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
				child.PrintPreview();
		}

		private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
			{
				child.Undo();
			}
		}

		private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
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

		private void FindToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
			{
				child.Find();
			}
		}

		private void FindNextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
			{
				child.FindNext();
			}
		}

		private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
				child.SelectAll();
		}

		private void GoToToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RPHexEditorForm child = (RPHexEditorForm)this.ActiveMdiChild;

			if (child != null)
				child.GoTo();
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

		private void ToolStripStatusINS_DoubleClick(object sender, EventArgs e)
		{
			ToggleStatusBarControl_INS();
		}

		private void OptionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MDIDemoOption frm = new MDIDemoOption();
			frm.ShowDialog();
		}

		private void OnClientStateUpdate(object myObject, EventArgs myEventArgs)
		{
			if (MdiChildren.Length == 0)
			{
				undoToolStripMenuItem.Enabled = false;
				redoToolStripMenuItem.Enabled = false;
				copyToolStripMenuItem.Enabled = false;
				cutToolStripMenuItem.Enabled = false;
				pasteToolStripMenuItem.Enabled = false;
				findToolStripMenuItem.Enabled = false;
				findNextToolStripMenuItem.Enabled = false;
				selectAllToolStripMenuItem.Enabled = false;
				goToToolStripMenuItem.Enabled = false;

				copyToolStripButton.Enabled = false;
				cutToolStripButton.Enabled = false;
				pasteToolStripButton.Enabled = false;
				undoToolStripButton.Enabled = false;
			}

			saveToolStripMenuItem.Enabled = MdiChildren.Length > 0;
			saveAsToolStripMenuItem.Enabled = MdiChildren.Length > 0;
			saveToolStripButton.Enabled = saveAsToolStripMenuItem.Enabled;
			printToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled;
			printPreviewToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled;
			printToolStripButton.Enabled = saveAsToolStripMenuItem.Enabled;
		}
	}
}
