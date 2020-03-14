using System;
using System.IO;
using System.Windows.Forms;

namespace RPHexEditorMDIDemo
{
    public partial class RPHexEditorForm : Form
    {		
		private string _fileFilter = "All Files (*.*)|*.*|Bin Files (*.bin)|*.bin|Text Files (*.txt)|*.txt";
		private RPHexEditor.FileByteData _fileByteData;
		private RPHexEditor.MemoryByteData _memoryByteData;
		
		private Timer _timer = null;

		public RPHexEditorForm()
        {
            InitializeComponent();
			this.Size = new System.Drawing.Size(640, 480);
			
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
				{
					components.Dispose();
				}

				this._timer.Stop();
				this._timer.Dispose();

				if (this._fileByteData != null)
					this._fileByteData.Dispose();
			}

			base.Dispose(disposing);
		}

		public string GetFileName()
		{
			if (rpHexEditor.ByteDataSource.GetType() == typeof(RPHexEditor.MemoryByteData))
				return "Untitled";
			else
			{
				RPHexEditor.FileByteData fd = rpHexEditor.ByteDataSource as RPHexEditor.FileByteData;
				return fd.FileName;
			}
		}

		public bool LoadFile(string fileName, bool readOnly = false)
		{
			bool bRet = false;

			try
			{
				this._fileByteData = new RPHexEditor.FileByteData(fileName, readOnly);
				rpHexEditor.ByteDataSource = this._fileByteData;

				_fileByteData.DataChanged += new EventHandler(OnFileByteDataDataChanged);
				_fileByteData.DataLengthChanged += new EventHandler(OnFileByteDataDataLengthChanged);

				bRet = true;
			}
			catch (Exception ex)
			{
				string msg = string.Format("Failed to open file '{0}'.\n{1}", fileName, ex.Message);
				MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return bRet;
		}
		
		public bool NewFile()
		{
			bool bRet = false;

			this._memoryByteData = new RPHexEditor.MemoryByteData(new byte[] { 0 });
			rpHexEditor.ByteDataSource = this._memoryByteData;

			this._memoryByteData.DataChanged += new EventHandler(OnFileByteDataDataChanged);
			this._memoryByteData.DataLengthChanged += new EventHandler(OnFileByteDataDataLengthChanged);
			rpHexEditor.ReadOnly = false;

			return bRet;
		}

		private void RPHexEditorForm_Activated(object sender, System.EventArgs e)
		{
			RPHexEditor_ReadOnlyChanged(sender, e);
			RPHexEditor_InsertModeChanged(sender, e);
		}

		private void RPHexEditorForm_Deactivate(object sender, System.EventArgs e)
		{
			((MDIDemo)MdiParent).GetStatusBarControl_RW.Text = "";
			((MDIDemo)MdiParent).GetStatusBarControl_INS.Text = "";
			((MDIDemo)MdiParent).GetStatusBarControl_Position.Text = "";
		}

		public void SetSelection(long selStart, long selEnd)
		{
			rpHexEditor.SetSelection(selStart, selEnd);
		}

		public void RemoveSelection()
		{
			rpHexEditor.RemoveSelection();
		}

		public void SelectAll()
		{
			rpHexEditor.SelectAll();
		}

		public void Copy()
		{
			rpHexEditor.Copy();
		}

		public void Paste()
		{
			rpHexEditor.Paste();
		}

		public void Cut()
		{
			rpHexEditor.Cut();
		}

		public void Undo()
		{
			this.rpHexEditor.Undo();
		}

		public bool IsChanged()
		{
			return rpHexEditor.ByteDataSource.IsChanged();
		}

		/// <summary>
		/// Save the current file.
		/// </summary>
		public void CommitChanges()
		{
			if (rpHexEditor.ByteDataSource.GetType() == typeof(RPHexEditor.MemoryByteData))
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				saveFileDialog.Filter = _fileFilter;

				if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
					return;

				FileStream _fileDataStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite);
				RPHexEditor.MemoryByteData md = rpHexEditor.ByteDataSource as RPHexEditor.MemoryByteData;
				_fileDataStream.Write(md.Bytes.ToArray(), 0, md.Bytes.Count);
				_fileDataStream.Dispose();

				this._fileByteData = new RPHexEditor.FileByteData(saveFileDialog.FileName);

				this._fileByteData.DataChanged += new EventHandler(OnFileByteDataDataChanged);
				this._fileByteData.DataLengthChanged += new EventHandler(OnFileByteDataDataLengthChanged);

				rpHexEditor.ByteDataSource = _fileByteData;
				this.Text = saveFileDialog.FileName;
			}
			
			rpHexEditor.CommitChanges();
		}

		/// <summary>
		/// Save the current file under a different name.
		/// ! NOT really efficient, should only be used for small files !
		/// </summary>
		public void CommitChangesAs()
		{
			if (rpHexEditor.ByteDataSource.GetType() == typeof(RPHexEditor.MemoryByteData))
			{
				this.CommitChanges();
				return;
			}

			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			saveFileDialog.Filter = _fileFilter;

			if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
				return;

			RPHexEditor.FileByteData fileByteData = rpHexEditor.ByteDataSource as RPHexEditor.FileByteData;
			FileStream _fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite);

			for (int i = 0; i < fileByteData.Length; i++)
				_fileStream.WriteByte(fileByteData.ReadByte(i));

			_fileStream.Dispose();

			RPHexEditorForm childForm = new RPHexEditorForm();
			childForm.MdiParent = this.MdiParent;
			childForm.Text = saveFileDialog.FileName;
			childForm.LoadFile(saveFileDialog.FileName);
			childForm.Show();
		}

		public InsertKeyMode GetInsertMode
		{
			get { return this.rpHexEditor.InsertMode; }
		}

		public void SetInsertMode(InsertKeyMode mode)
		{
			this.rpHexEditor.InsertMode = mode;
		}

		protected void OnFileByteDataDataChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("OnFileByteDataDataChanged fired");
		}

		protected void OnFileByteDataDataLengthChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("OnFileByteDataDataLengthChanged fired");
		}

		private void RPHexEditor_ReadOnlyChanged(object sender, System.EventArgs e)
		{
			ToolStripStatusLabel tsl = ((MDIDemo)MdiParent).GetStatusBarControl_RW;
			tsl.Text = rpHexEditor.ReadOnly ? "RO" : "R/W";	
		}

		private void RPHexEditor_InsertModeChanged(object sender, System.EventArgs e)
		{
			ToolStripStatusLabel tsl = ((MDIDemo)MdiParent).GetStatusBarControl_INS;
			tsl.Text = (rpHexEditor.InsertMode == InsertKeyMode.Insert) ? "INS" : "OVR";
		}

		private void RPHexEditor_BytePositionChanged(object sender, System.EventArgs e)
		{
			ToolStripStatusLabel tsl = ((MDIDemo)MdiParent).GetStatusBarControl_Position;
			tsl.Text = "Ln " + rpHexEditor.BytePositionLine.ToString() + ", Col " + rpHexEditor.BytePositionColumn.ToString();
		}

		private void RPHexEditor_SelectionChanged(object sender, System.EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("rpHexEditor_SelectionChanged fired");
		}

		private void OnClientStateUpdate(Object myObject, EventArgs myEventArgs)
		{
			if (MdiParent == null)
				return;

			ToolStripMenuItem tsmCopy = ((MDIDemo)MdiParent).GetTSM_Copy;
			ToolStripButton tsbCopy = ((MDIDemo)MdiParent).GetTSB_Copy;
			
			if (tsmCopy.Enabled != rpHexEditor.HasSelection() && rpHexEditor.ByteDataSource != null)
			{
				tsmCopy.Enabled = rpHexEditor.HasSelection() && rpHexEditor.ByteDataSource != null;
				tsbCopy.Enabled = tsmCopy.Enabled;
			}

			ToolStripMenuItem tsmCut = ((MDIDemo)MdiParent).GetTSM_Cut;
			ToolStripButton tsbCut = ((MDIDemo)MdiParent).GetTSB_Cut;
			
			if (tsmCut.Enabled != rpHexEditor.HasSelection() && rpHexEditor.ByteDataSource != null && !rpHexEditor.ReadOnly && rpHexEditor.Enabled)
			{
				tsmCut.Enabled = rpHexEditor.HasSelection() && rpHexEditor.ByteDataSource != null && !rpHexEditor.ReadOnly && rpHexEditor.Enabled;
				tsbCut.Enabled = tsmCut.Enabled;
			}

			ToolStripMenuItem tsmPaste = ((MDIDemo)MdiParent).GetTSM_Paste;
			ToolStripButton tsbPaste = ((MDIDemo)MdiParent).GetTSB_Paste;
			DataObject clip_DO = Clipboard.GetDataObject() as DataObject;

			if (clip_DO == null || rpHexEditor.ByteDataSource == null || rpHexEditor.ReadOnly || !rpHexEditor.Enabled)
			{
				tsmPaste.Enabled = false;
				tsbPaste.Enabled = false;
			}
			else
			{
				if (tsmPaste.Enabled != clip_DO.GetDataPresent("rawbinary") || clip_DO.GetDataPresent(typeof(string)))
				{
					tsmPaste.Enabled = clip_DO.GetDataPresent("rawbinary") || clip_DO.GetDataPresent(typeof(string));
					tsbPaste.Enabled = tsmPaste.Enabled;
				}
			}

			ToolStripMenuItem tsmSelectAll = ((MDIDemo)MdiParent).GetTSM_SelectAll;

			if (tsmSelectAll.Enabled != (rpHexEditor.ByteDataSource != null) && rpHexEditor.Enabled)
				tsmSelectAll.Enabled = (rpHexEditor.ByteDataSource != null) && rpHexEditor.Enabled;

			ToolStripMenuItem tsmUndo = ((MDIDemo)MdiParent).GetTSM_Undo;

			if (tsmUndo.Enabled != rpHexEditor.IsUndoAvailable)
				tsmUndo.Enabled = rpHexEditor.IsUndoAvailable;
		}
    }
}
