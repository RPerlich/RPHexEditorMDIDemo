using System;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RPHexEditorMDIDemo
{
	public partial class RPHexEditorForm : Form
	{
		private string _fileFilter = "All Files (*.*)|*.*|Bin Files (*.bin)|*.bin|Text Files (*.txt)|*.txt";
		private RPHexEditor.FileByteData _fileByteData;
		private RPHexEditor.MemoryByteData _memoryByteData;
		private RPQuickFind _quickFindWnd;
		private PrintDocument _PrintDoc;
		private int iPrintPage = 1;
		private Timer _timer = null;

		public RPHexEditorForm()
		{
			InitializeComponent();
			Size = new Size(640, 480);

			_quickFindWnd = new RPQuickFind();
			_quickFindWnd.TopLevel = false;
			_quickFindWnd.TopMost = true;
			_quickFindWnd.Parent = this;
			_quickFindWnd.Left = ClientRectangle.Width - _quickFindWnd.Width - 20; // ? no scrollbar width available, use default 20px
			_quickFindWnd.Top = rpHexEditor.Margin.Top;
			_quickFindWnd.FindNext += new EventHandler(OnFindNext);
			Controls.Add(_quickFindWnd);

			_timer = new Timer();
			_timer.Tick += new EventHandler(OnClientStateUpdate);
			_timer.Interval = 500;
			_timer.Start();

			_PrintDoc = new PrintDocument();
			_PrintDoc.BeginPrint += OnPrintDoc_BeginPrint;
			_PrintDoc.EndPrint += OnPrintDoc_EndPrint;
			_PrintDoc.PrintPage += OnPrintDoc_PrintPage;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				_timer.Stop();
				_timer.Dispose();

				if (_fileByteData != null)
					_fileByteData.Dispose();

				if (_PrintDoc != null)
					_PrintDoc.Dispose();
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
			rpHexEditor.Undo();
		}

		public void Find()
		{
			if (!_quickFindWnd.Visible)
			{
				_quickFindWnd.Show();
				_quickFindWnd.BringToFront();
				RPHexEditorForm_ResizeEnd(this, EventArgs.Empty);
			}
		}

		public void FindNext()
		{
			if (_quickFindWnd.SearchText == null || _quickFindWnd.SearchText == string.Empty)
				Find();
			else
				OnFindNext(this, EventArgs.Empty);
		}

		private void OnFindNext(object sender, EventArgs e)
		{
			if (IsSearching)
			{
				System.Media.SystemSounds.Beep.Play();
				return;
			}

			IsSearching = true;

			RPHexEditor.FindByteDataOption fbdo = rpHexEditor.FindDataOption;
			fbdo.SearchText = _quickFindWnd.SearchText; ;
			fbdo.SearchDirection = _quickFindWnd.FindPrevious ? RPHexEditor.SearchDirection.Direction_Up : RPHexEditor.SearchDirection.Direction_Down;
			fbdo.SearchStartIndex = rpHexEditor.BytePosition;
			fbdo.MatchCase = _quickFindWnd.MatchCase;

			rpHexEditor.Find();
		}

		public bool IsSearching { get; set; }

		public void GoTo()
		{
			rpHexEditor.BytePosition = 5;	// very simple test implementation
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
			rpHexEditor.InsertMode = mode;
		}

		public void Print()
		{
			Margins margins = _PrintDoc.DefaultPageSettings.Margins;
			Rectangle marginBounds = _PrintDoc.DefaultPageSettings.Bounds;
			marginBounds.X += margins.Left;
			marginBounds.Y += margins.Top;
			marginBounds.Width -= margins.Right;
			marginBounds.Width -= margins.Left;
			marginBounds.Height -= margins.Bottom;
			marginBounds.Height -= margins.Top;

			using (var printDlg = new PrintDialog())
			{
				printDlg.Document = _PrintDoc;
				printDlg.AllowSomePages = true;
				printDlg.PrinterSettings.FromPage = 1;
				printDlg.PrinterSettings.ToPage = 1;
				printDlg.PrinterSettings.MinimumPage = 1;
				printDlg.PrinterSettings.MaximumPage = rpHexEditor.PrintGetMaxPrintPages(marginBounds);

				if (printDlg.ShowDialog(this) == DialogResult.OK)
				{
					_PrintDoc.PrinterSettings = printDlg.PrinterSettings;

					iPrintPage = 1;

					if (_PrintDoc.PrinterSettings.PrintRange == PrintRange.SomePages)
						iPrintPage = _PrintDoc.PrinterSettings.FromPage;

					_PrintDoc.Print();
				}
			}
		}

		public void PrintPreview()
		{
			using (var printPreview = new PrintPreviewDialog())
			{
				printPreview.Size = new Size(640, 480);
				printPreview.Document = _PrintDoc;
				printPreview.ShowDialog(this);
			}
		}

		protected void OnFileByteDataDataChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("OnFileByteDataDataChanged fired");
		}

		protected void OnFileByteDataDataLengthChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("OnFileByteDataDataLengthChanged fired");
		}

		private void RPHexEditor_ReadOnlyChanged(object sender, EventArgs e)
		{
			ToolStripStatusLabel tsl = ((MDIDemo)MdiParent).GetStatusBarControl_RW;
			tsl.Text = rpHexEditor.ReadOnly ? "RO" : "R/W";
		}

		private void RPHexEditor_InsertModeChanged(object sender, EventArgs e)
		{
			ToolStripStatusLabel tsl = ((MDIDemo)MdiParent).GetStatusBarControl_INS;
			tsl.Text = (rpHexEditor.InsertMode == InsertKeyMode.Insert) ? "INS" : "OVR";
		}

		private void RPHexEditor_BytePositionChanged(object sender, EventArgs e)
		{
			ToolStripStatusLabel tsl = ((MDIDemo)MdiParent).GetStatusBarControl_Position;
			tsl.Text = "Ln " + rpHexEditor.BytePositionLine.ToString() + ", Col " + rpHexEditor.BytePositionColumn.ToString();
		}

		private void RPHexEditor_SelectionChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("rpHexEditor_SelectionChanged fired");
		}

		private void RPHexEditor_FindPositionFound(object sender, EventArgs e)
		{
			RPHexEditor.FindByteDataEventArgs e1 = e as RPHexEditor.FindByteDataEventArgs;
			IsSearching = false;

			if (e1.FoundPosition == -1)
				MessageBox.Show(
					"Find reached the end point of the search or the specified text was not found.",
					"Information", 
					MessageBoxButtons.OK, 
					MessageBoxIcon.Information);
			
			System.Diagnostics.Debug.WriteLine("Find: Found position at {0:X}", e1.FoundPosition);
		}

		private void OnClientStateUpdate(object myObject, EventArgs myEventArgs)
		{
			if (MdiParent == null)
				return;

			ToolStripMenuItem tsmCopy = ((MDIDemo)MdiParent).GetTSM_Copy;
			ToolStripButton tsbCopy = ((MDIDemo)MdiParent).GetTSB_Copy;

			if (tsmCopy.Enabled != rpHexEditor.IsCmdCopyAvailable)
			{
				tsmCopy.Enabled = rpHexEditor.IsCmdCopyAvailable;
				tsbCopy.Enabled = tsmCopy.Enabled;
			}

			ToolStripMenuItem tsmCut = ((MDIDemo)MdiParent).GetTSM_Cut;
			ToolStripButton tsbCut = ((MDIDemo)MdiParent).GetTSB_Cut;

			if (tsmCut.Enabled != rpHexEditor.IsCmdCutAvailable)
			{
				tsmCut.Enabled = rpHexEditor.IsCmdCutAvailable;
				tsbCut.Enabled = tsmCut.Enabled;
			}

			ToolStripMenuItem tsmPaste = ((MDIDemo)MdiParent).GetTSM_Paste;
			ToolStripButton tsbPaste = ((MDIDemo)MdiParent).GetTSB_Paste;

			if (tsmPaste.Enabled != rpHexEditor.IsCmdPasteAvailable)
			{
				tsmPaste.Enabled = rpHexEditor.IsCmdPasteAvailable;
				tsbPaste.Enabled = tsmPaste.Enabled;
			}

			ToolStripMenuItem tsmSelectAll = ((MDIDemo)MdiParent).GetTSM_SelectAll;

			if (tsmSelectAll.Enabled != rpHexEditor.IsCmdSelectAvailable)
				tsmSelectAll.Enabled = rpHexEditor.IsCmdSelectAvailable;

			ToolStripMenuItem tsmUndo = ((MDIDemo)MdiParent).GetTSM_Undo;
			ToolStripButton tsbUndo = ((MDIDemo)MdiParent).GetTSB_Undo;

			if (tsmUndo.Enabled != rpHexEditor.IsCmdUndoAvailable)
			{
				tsmUndo.Enabled = rpHexEditor.IsCmdUndoAvailable;
				tsbUndo.Enabled = tsmUndo.Enabled;
			}

			ToolStripMenuItem tsmFind = ((MDIDemo)MdiParent).GetTSM_Find;
			if (tsmFind.Enabled != rpHexEditor.IsCmdFindAvailable)
				tsmFind.Enabled = rpHexEditor.IsCmdFindAvailable;

			ToolStripMenuItem tsmFindNext = ((MDIDemo)MdiParent).GetTSM_FindNext;
			if (tsmFindNext.Enabled != rpHexEditor.IsCmdFindAvailable)
				tsmFindNext.Enabled = rpHexEditor.IsCmdFindAvailable;

			ToolStripMenuItem tsmGoTo = ((MDIDemo)MdiParent).GetTSM_GoTo;
			if (tsmGoTo.Enabled != rpHexEditor.IsCmdGoToAvailable)
				tsmGoTo.Enabled = rpHexEditor.IsCmdGoToAvailable;
		}

		private void RPHexEditorForm_ResizeEnd(object sender, EventArgs e)
		{
			if (_quickFindWnd != null && _quickFindWnd.Visible)
			{
				//looks like a MS Bug, need to use native methods
				NativeMethods.MoveWindow(
					_quickFindWnd.Handle, 
					ClientRectangle.Width - _quickFindWnd.Width - 20, 
					_quickFindWnd.Top, _quickFindWnd.Width, 
					_quickFindWnd.Height, true);
			}
		}

		private void OnPrintDoc_BeginPrint(object sender, PrintEventArgs e)
		{
			_PrintDoc.DocumentName = GetFileName();
			System.Diagnostics.Debug.WriteLine("Start printing...");
		}

		private void OnPrintDoc_EndPrint(object sender, PrintEventArgs e)
		{
			iPrintPage = 1;
			System.Diagnostics.Debug.WriteLine("End printing...");
		}

		private void OnPrintDoc_PrintPage(object sender, PrintPageEventArgs e)
		{
			bool bHasMorePages = false;

			if (_PrintDoc.PrinterSettings.PrintRange == PrintRange.SomePages)
			{
				if (iPrintPage > _PrintDoc.PrinterSettings.ToPage)
					return;
			}

			System.Diagnostics.Debug.WriteLine("Print page -> {0}", iPrintPage);
			rpHexEditor.Print(iPrintPage, e, ref bHasMorePages);
			e.HasMorePages = bHasMorePages;
			if (bHasMorePages) iPrintPage++;
		}
	}

	internal static class NativeMethods
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
	}
}
