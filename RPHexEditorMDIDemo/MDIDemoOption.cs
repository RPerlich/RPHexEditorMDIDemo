using System;
using System.Windows.Forms;

namespace RPHexEditorMDIDemo
{
	public partial class MDIDemoOption : Form
	{
		private RPHexEditor.MemoryByteData _memoryByteData;

		public MDIDemoOption()
		{
			InitializeComponent();

			this._memoryByteData = new RPHexEditor.MemoryByteData(new byte[] { 0 });
			rpHexEditorUC1.ByteDataSource = this._memoryByteData;
			rpHexEditorUC1.ViewLineAddress = cb_LineAddress.Checked;
			rpHexEditorUC1.ViewLineCharacters = cbLineCharacter.Checked;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				rpHexEditorUC1.Font = fontDialog1.Font;
		}

		private void cb_LineAddress_CheckedChanged(object sender, EventArgs e)
		{
			rpHexEditorUC1.ViewLineAddress = cb_LineAddress.Checked;
		}

		private void cbLineCharacter_CheckedChanged(object sender, EventArgs e)
		{
			rpHexEditorUC1.ViewLineCharacters = cbLineCharacter.Checked;
		}
	}
}
