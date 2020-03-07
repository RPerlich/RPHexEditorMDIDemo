using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		}
	}
}
