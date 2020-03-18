using System;
using System.Windows.Forms;

namespace RPHexEditorMDIDemo
{
	public partial class RPQuickFind : Form
	{
		public RPQuickFind()
		{
			InitializeComponent();

			toolTipFind.SetToolTip(btnFind, "Find next (F3)");
			toolTipClose.SetToolTip(btnClose, "Close");
			toolTipMatchCase.SetToolTip(cbMatchCase, "Match case");
			toolTipFindPrevious.SetToolTip(cbFindPrev, "Find previous");
		}

		public event EventHandler FindNext;

		private void btnFind_Click(object sender, EventArgs e)
		{
			if (tbSearchText.Text != string.Empty)
			{
				SearchText = tbSearchText.Text;
				MatchCase = cbMatchCase.Checked;
				FindPrevious = cbFindPrev.Checked;
				FindNext?.Invoke(this, e);
			}
			else
				System.Media.SystemSounds.Beep.Play();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Hide();
		}

		public string SearchText { get; set; }

		public bool MatchCase { get; set; }

		public bool FindPrevious { get; set; }
	}
}
