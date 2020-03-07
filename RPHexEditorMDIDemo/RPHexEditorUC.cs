using System;
using System.IO;	// used for MemoryStream
using System.Text;  // used for Encoding
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RPHexEditor
{
	partial class RPHexEditorUC : UserControl
    {
		public RPHexEditorUC()
		{
			vScrollBar = new VScrollBar();
			this.Controls.Add(this.vScrollBar);
			hScrollBar = new HScrollBar();
			this.Controls.Add(this.hScrollBar);
			
			vScrollBar.Scroll += new ScrollEventHandler(VScrollBar_Scroll);
			hScrollBar.Scroll += new ScrollEventHandler(HScrollBar_Scroll);

			InitializeComponent();
			
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			_thumbTrackVTimer = new Timer();
			_thumbTrackVTimer.Interval = 50;
			_thumbTrackVTimer.Tick += new EventHandler(VScrollThumbTrack);

			_thumbTrackHTimer = new Timer();
			_thumbTrackHTimer.Interval = 50;
			_thumbTrackHTimer.Tick += new EventHandler(HScrollThumbTrack);

			_stringFormat = new StringFormat(StringFormat.GenericTypographic);
			_stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

			_currEncoder = _encoderANSI;
		}

		private VScrollBar vScrollBar;
		private HScrollBar hScrollBar;

		private Timer _thumbTrackVTimer = null;
		private Timer _thumbTrackHTimer = null;

		ClickAreas _clickArea = ClickAreas.AREA_BYTES;
		EnterMode _enterMode = EnterMode.BYTES;
		InsertKeyMode _insertMode = InsertKeyMode.Overwrite;

		bool _readOnly = false;
		bool _autoBytesPerLine = false;
		bool _drawCharacters = true;
		bool _drawAddressLine = true;
		bool _caretVisible = false;
		string _lineAddressFormat = "X8";
		string _lineByteFormat = "X2";
		int _iHexMaxHBytes;
		int _iHexMaxVBytes;
		int _iHexMaxBytes;
		int _bytesPerLine = 16;
		long _scrollVmin;
		long _scrollVmax;
		long _scrollVpos;
		long _scrollHmin;
		long _scrollHmax;
		long _scrollHpos;

		StringFormat _stringFormat;
		Rectangle _rectContent;
		Rectangle _recAddressInfo;
		Rectangle _recHex;
		Rectangle _recChars;
		SizeF _charSize;
		Color _selectionBackColor = SystemColors.Highlight;
		Color _selectionForeColor = SystemColors.HighlightText;
		IByteData _byteData;

		long _bytePos = 0;
        bool _isNibble = false;
		bool _lMouseDown = false;
        long _startByte = 0;
        long _endByte = 0;
		bool _isShiftActive = false;
		long _startSelection = -1;
		long _endSelection = -1;        		        
		long _thumbTrackVPosition = 0;
		long _thumbTrackHPosition = 0;
		const int ThumbTrackMS = 50;
        int _lastThumbTrackMS = 0;
		
		Encoder _encoding = Encoder.ANSI;
		IRPHexEditorCharEncoder _encoderANSI = new RPHexEditorCharANSIEncoder();
		IRPHexEditorCharEncoder _encoderEBCDIC = new RPHexEditorCharEBCDICEncoder();
		IRPHexEditorCharEncoder _currEncoder;

		ContextMenuStrip _internalContextMenu = null;
		ToolStripMenuItem _internalCutMenuItem = null;
		ToolStripMenuItem _internalCopyMenuItem = null;
		ToolStripMenuItem _internalPasteMenuItem = null;
		ToolStripSeparator _internalSeparatorMenuItem_1 = null;
		ToolStripMenuItem _internalSelectAllMenuItem = null;

		#region Overridden properties
		[DefaultValue(typeof(Color), "White")]
		public override Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		/*public override Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				if (value == null)
					return;

				base.Font = value;
				this.AdjustWindowSize();
				this.Invalidate();
			}
		}*/

		/// <summary>
		/// Not used.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
		public override RightToLeft RightToLeft
		{
			get
			{
				return base.RightToLeft;
			}
			set
			{
				base.RightToLeft = value;
			}
		}
		#endregion
        
		#region Properties

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public long BytePosition
		{
			get { return _bytePos; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public long BytePositionLine
		{
			get { return PosToLogPoint(_bytePos).Y + 1; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public long BytePositionColumn
		{
			get { return PosToLogPoint(_bytePos).X + 1; }
		}

		[DefaultValue(false), Category("HexEditor"), Description("Get or set whether the data can be changed.")]
		public bool ReadOnly
		{
			get { return _readOnly; }
			set
			{
				if (_readOnly == value)
					return;

				_readOnly = value;
				OnReadOnlyChanged(EventArgs.Empty);
				Invalidate();
			}
		}

		[DefaultValue(false), Category("HexEditor"), Description("Get or set whether the number of bytes per line is fixed.")]
		public bool AutomaticBytesPerLine
		{
			get { return _autoBytesPerLine; }
			set
			{
				if (_autoBytesPerLine == value)
					return;

				_autoBytesPerLine = value;
				OnAutomaticBytesPerLine(EventArgs.Empty);
				Invalidate();
			}
		}

		[DefaultValue(16), Category("HexEditor"), Description("Get or set the maximum number of bytes per line.")]
		public int BytesPerLine
		{
			get { return _bytesPerLine; }
			set
			{
				if (_bytesPerLine == value)
					return;

				_bytesPerLine = value;
				OnBytesPerLineChanged(EventArgs.Empty);

				this.AdjustWindowSize();
				Invalidate();
			}
		}

		[DefaultValue(typeof(Color), "Highlight"), Category("HexEditor"), Description("Get or set the selection background color.")]
		public Color SelectionBackColor
		{
			get { return _selectionBackColor; }
			set { _selectionBackColor = value; Invalidate(); }
		}

		[DefaultValue(typeof(Color), "HighlightText"), Category("HexEditor"), Description("Get or set the selection foreground color.")]
		public Color SelectionForeColor
		{
			get { return _selectionForeColor; }
			set { _selectionForeColor = value; Invalidate(); }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public long SelectionStart
		{
			get { return _startSelection; }			
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public long SelectionEnd
		{
			get { return _endSelection; }
		}

		[DefaultValue(Encoder.ANSI), Category("HexEditor"), Description("Get or set the encoding used for text display.")]
		public Encoder Encoding
		{
			get { return _encoding; }
			set 
			{ 
				_encoding = value;
				_currEncoder = (value == Encoder.ANSI) ? _encoderANSI : _encoderEBCDIC;
				
				Invalidate(); 
			}
		}

		[DefaultValue(InsertKeyMode.Overwrite), Category("HexEditor"), Description("Get the current insert key mode.")]
		public InsertKeyMode InsertMode
		{
			get { return _insertMode; }
			set 
			{
				_insertMode = value;
				OnInsertModeChanged(EventArgs.Empty);
				DestroyCaret();
				CreateCaret();
			}
		}


        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IByteData ByteDataSource
		{
            get { return _byteData; }
            set
            {
				if (_byteData == value)
                    return;

				if (_byteData != null)
					_byteData.DataLengthChanged -= new EventHandler(OnByteDataLengthChanged);

				_byteData = value;
				
				if (_byteData != null)
					_byteData.DataLengthChanged += new EventHandler(OnByteDataLengthChanged);

				OnByteDataSourceChanged(EventArgs.Empty);

                if (value == null)
                {
                    _bytePos = -1;
                    _isNibble = false;
					_startSelection = -1;
					_startSelection = -1;

                    DestroyCaret();
                }

                _scrollVpos = 0;
				_scrollHpos = 0;

                UpdateVisibilityBytes();
				this.AdjustWindowSize();

                Invalidate();
				SetInternalContextMenu();
            }
        }
	        
        #endregion

        #region Events
		
		[Description("Fired if the value of the ReadOnly property has changed.")]
		public event EventHandler ReadOnlyChanged;

		[Description("Fired if the value of the AutomaticBytesPerLine property has changed.")]
		public event EventHandler AutomaticBytesPerLineChanged;

		[Description("Fired if the value of the BytesPerLine property has changed.")]
		public event EventHandler BytesPerLineChanged;
			
		[Description("Fired when the selected values have changed.")]
		public event EventHandler SelectionChanged;

		[Description("Fired when the insert / override mode have changed.")]
		public event EventHandler InsertModeChanged;

		[Description("Fired when the caret position have changed.")]
		public event EventHandler BytePositionChanged;

		[Description("Fired when the ByteData source have changed.")]
		public event EventHandler ByteDataSourceChanged;
                        
        #endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this._thumbTrackVTimer.Enabled = false;
				this._thumbTrackVTimer.Dispose();
				this._thumbTrackHTimer.Enabled = false;
				this._thumbTrackHTimer.Dispose();
			}

			base.Dispose(disposing);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
			e.Graphics.FillRectangle(new SolidBrush(Color.FloralWhite), _recAddressInfo);
			e.Graphics.FillRectangle(new SolidBrush(Color.AliceBlue), _recHex);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (_byteData == null)
				return;

			Region r = new Region(ClientRectangle);
			r.Exclude(_rectContent);
			e.Graphics.ExcludeClip(r);

			UpdateVisibilityBytes();

			if (_drawAddressLine) DrawAddressLine(e.Graphics, _startByte, _endByte);

			DrawLines(e.Graphics, _startByte, _endByte);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.AdjustWindowSize();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!this.Focused) this.Focus();
			if (!this._isShiftActive && e.Button == MouseButtons.Left) this.RemoveSelection();

			if (e.Button == MouseButtons.Left)
			{
				_isNibble = false;
				_lMouseDown = true;

				Point p = new Point(e.X, e.Y);

				if (_recAddressInfo.Contains(p))
					_clickArea = ClickAreas.AREA_ADDRESS;

				if (_recHex.Contains(p))
					_clickArea = ClickAreas.AREA_BYTES;

				if (_recChars.Contains(p))
					_clickArea = ClickAreas.AREA_CHARS;

				if (_clickArea == ClickAreas.AREA_BYTES || _clickArea == ClickAreas.AREA_CHARS)
					UpdateCaretPosition(p, true);
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_lMouseDown = false;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			Point p = new Point(e.X, e.Y);
			
			if (_lMouseDown)
			{
				long oldBytePos = _bytePos;
				
				GetBytePosFromPoint(p, ref _bytePos, ref this._isNibble);

				_bytePos = (_bytePos > _byteData.Length - 1) ? _byteData.Length - 1 : _bytePos;
				
				if (_startSelection == -1)
				{
					_startSelection = _bytePos;
					_endSelection = _bytePos;
				}
				else
					_endSelection = _bytePos;

				if (_bytePos != oldBytePos && _caretVisible)
				{
					DestroyCaret();
					CreateCaret();
				}

				Invalidate();
				OnSelectionChanged(EventArgs.Empty);
				
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{				
			base.OnGotFocus(e);
			this.CreateCaret();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			DestroyCaret();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			int linesToScroll = -(e.Delta * SystemInformation.MouseWheelScrollLines / 120);
			this.VScrollLines(linesToScroll);

			base.OnMouseWheel(e);
		}

		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			if (ReadOnlyChanged != null)
				ReadOnlyChanged(this, e);
		}

		protected virtual void OnAutomaticBytesPerLine(EventArgs e)
		{
			if (AutomaticBytesPerLineChanged != null)
				AutomaticBytesPerLineChanged(this, e);
		}

		protected virtual void OnBytesPerLineChanged(EventArgs e)
		{
			if (BytesPerLineChanged != null)
				BytesPerLineChanged(this, e);
		}

		protected virtual void OnSelectionChanged(EventArgs e)
		{
			if (SelectionChanged != null)
				SelectionChanged(this, e);
		}

		protected virtual void OnInsertModeChanged(EventArgs e)
		{
			if (InsertModeChanged != null)
				InsertModeChanged(this, e);
		}

		protected virtual void OnBytePositionChanged(EventArgs e)
		{
			if (BytePositionChanged != null)
				BytePositionChanged(this, e);
		}

		protected virtual void OnByteDataSourceChanged(EventArgs e)
		{
			if (ByteDataSourceChanged != null)
				ByteDataSourceChanged(this, e);
		}

		protected void OnByteDataLengthChanged(object sender, EventArgs e)
		{
			UpdateScrollBars();
		}

		void SetHorizontalByteCount(int value)
		{
			if (_iHexMaxHBytes == value)
				return;

			_iHexMaxHBytes = value;
			OnBytesPerLineChanged(EventArgs.Empty);
		}

		void AdjustWindowSize()
		{
			bool vScrollNeeded = false;
			bool hScrollNeeded = false;

			_rectContent = ClientRectangle;

			using (var graphics = this.CreateGraphics())
			{
				this._charSize = graphics.MeasureString("W", this.Font, 100, _stringFormat);
			}

			if (_byteData != null)
			{
				int iVisibleRows = (int)Math.Floor((double)_rectContent.Height / (double)this._charSize.Height);
				vScrollNeeded = (_byteData.Length / _iHexMaxHBytes - iVisibleRows) >= 0;

				double clientSize = _rectContent.Width - this._charSize.Width * 9;
				clientSize -= _iHexMaxHBytes * this._charSize.Width * 3 + this._charSize.Width;
				if (_drawCharacters) clientSize -= _iHexMaxHBytes * this._charSize.Width;
				if (vScrollNeeded) clientSize -= vScrollBar.Width;
				hScrollNeeded = clientSize <= 0;
			}

			if (vScrollNeeded) _rectContent.Width -= vScrollBar.Width;
			if (hScrollNeeded) _rectContent.Height -= hScrollBar.Height;

			vScrollBar.Left = _rectContent.X + _rectContent.Width;
			vScrollBar.Top = _rectContent.Y;
			vScrollBar.Height = _rectContent.Height;

			hScrollBar.Left = _rectContent.X;
			hScrollBar.Top = _rectContent.Height;
			hScrollBar.Width = _rectContent.Width;
			System.Diagnostics.Debug.WriteLine("Left: " + hScrollBar.Left);

			if (_drawAddressLine)
			{
				_recAddressInfo = new Rectangle(_rectContent.X - (int)this._charSize.Width * (int)_scrollHpos,
											_rectContent.Y,
											(int)(this._charSize.Width * 9), _rectContent.Height);
			}
			else
			{
				_recAddressInfo = Rectangle.Empty;
				_recAddressInfo.X = _rectContent.X - (int)this._charSize.Width * (int)_scrollHpos;
			}

			_recHex = new Rectangle(_recAddressInfo.X + _recAddressInfo.Width,
									_recAddressInfo.Y,
									_rectContent.Width - _recAddressInfo.Width,
									_rectContent.Height);

			if (_autoBytesPerLine)
			{
				int hmax = (int)Math.Floor((double)_recHex.Width / (double)this._charSize.Width);

				if (_drawCharacters)
				{
					hmax -= 2;
					if (hmax > 1)
						SetHorizontalByteCount((int)Math.Floor((double)hmax / 4));
					else
						SetHorizontalByteCount(1);
				}
				else
				{
					if (hmax > 1)
						SetHorizontalByteCount((int)Math.Floor((double)hmax / 3));
					else
						SetHorizontalByteCount(1);
				}
				_recHex.Width = (int)Math.Floor(((double)_iHexMaxHBytes) * this._charSize.Width * 3 + (2 * this._charSize.Width));
			}
			else
			{
				SetHorizontalByteCount(_bytesPerLine);
				_recHex.Width = (int)Math.Floor(((double)_iHexMaxHBytes) * this._charSize.Width * 3 + this._charSize.Width);
			}


			_recChars = (_drawCharacters) ?
				new Rectangle(_recHex.X + _recHex.Width, _recHex.Y, (int)(this._charSize.Width * _iHexMaxHBytes), _recHex.Height) :
				Rectangle.Empty;

			_iHexMaxVBytes = (int)Math.Floor((double)_recHex.Height / (double)this._charSize.Height);
			_iHexMaxBytes = _iHexMaxHBytes * _iHexMaxVBytes;

			UpdateScrollBars();
		}

		string ByteToHexString(byte b)
		{
			return b.ToString(_lineByteFormat, System.Threading.Thread.CurrentThread.CurrentCulture);			
		}
		
		string GetCharacterFromByte(byte b)
		{
			return _currEncoder.ToChar(b).ToString();			
		}

		Color GetDefaultForeColor()
		{
			if (Enabled)
				return ForeColor;
			else
				return Color.Gray;
		}

        void UpdateVisibilityBytes()
        {
			if (_byteData == null || _byteData.Length == 0)
                return;

            _startByte = (_scrollVpos + 1) * _iHexMaxHBytes - _iHexMaxHBytes;
			_endByte = (long)Math.Min(_byteData.Length - 1, _startByte + _iHexMaxBytes - 1);
        }

		void DrawAddressLine(Graphics g, long startByte, long endByte)
        {
			Brush lineBrush = new SolidBrush(System.Drawing.SystemColors.GrayText);
			long lineAddress = 0;
			string sLineAddress = string.Empty;			

			long lines2Draw = PosToLogPoint(endByte + 1 - startByte).Y + 1;

			for (long i = 0; i < lines2Draw; i++)
            {
				lineAddress = (_iHexMaxHBytes * i + startByte);

				PointF physBytePos = LogToPhyPoint(new PointL(0, i));
				sLineAddress = lineAddress.ToString(_lineAddressFormat, System.Threading.Thread.CurrentThread.CurrentCulture);

				g.DrawString(sLineAddress, this.Font, lineBrush, new PointF(_recAddressInfo.X, physBytePos.Y), _stringFormat);								
            }
        }

		void DrawHexByte(Graphics g, byte b, Brush brush, PointL pt)
		{
			PointF bytePointF = LogToPhyPoint(pt);
			string sB = ByteToHexString(b);
			g.DrawString(sB, this.Font, brush, bytePointF, _stringFormat);
		}

		void DrawSelectedHexByte(Graphics g, byte b, Brush brush, Brush brushBack, PointL pt, bool isLastSelected)
		{
			PointF bytePointF = LogToPhyPoint(pt);
			string sB = ByteToHexString(b);			

			float rectWidth = _charSize.Width * 2;
			if (!isLastSelected) rectWidth += _charSize.Width;

			g.FillRectangle(brushBack, bytePointF.X, bytePointF.Y, rectWidth, _charSize.Height);
			g.DrawString(sB, this.Font, brush, bytePointF, _stringFormat);
		}
		
		void DrawLines(Graphics g, long startByte, long endByte)
		{
			Brush brush = new SolidBrush(GetDefaultForeColor());
			Brush selBrush = new SolidBrush(_selectionForeColor);
			Brush selBrushBack = new SolidBrush(_selectionBackColor);

			long counter = 0;
			long tmpEndByte = Math.Min(_byteData.Length - 1, endByte + _iHexMaxHBytes);

			for (long i = startByte; i < tmpEndByte + 1; i++)
			{
				byte theByte = _byteData.ReadByte(i);

				PointL gridPoint = PosToLogPoint(counter);
				PointF byteStringPointF = LogToPhyPointASCII(gridPoint);
				
				long tmpStartSelection = _startSelection;
				long tmpEndSelection = _endSelection;

				if (tmpStartSelection > tmpEndSelection)
				{
					long tmp = tmpStartSelection;
					tmpStartSelection = tmpEndSelection;
					tmpEndSelection = tmp;
				}

				bool isByteSelected = i >= tmpStartSelection && i <= tmpEndSelection;
				if (tmpStartSelection < 0 || tmpEndSelection < 0)
					isByteSelected = false;

				bool isLastBytePos = (gridPoint.X + 1 == _iHexMaxHBytes);

				if (isByteSelected)
					DrawSelectedHexByte(g, theByte, selBrush, selBrushBack, gridPoint, (i == tmpEndSelection || isLastBytePos));
				else
					DrawHexByte(g, theByte, brush, gridPoint);

				if (!_drawCharacters) continue;

				if (isByteSelected)
				{
					g.FillRectangle(selBrushBack, byteStringPointF.X, byteStringPointF.Y, _charSize.Width, _charSize.Height);
					g.DrawString(GetCharacterFromByte(theByte), Font, selBrush, byteStringPointF, _stringFormat);
				}
				else
					g.DrawString(GetCharacterFromByte(theByte), Font, brush, byteStringPointF, _stringFormat);

				counter++;
			}
		}

		PointL PosToLogPoint(long bytePosition)
        {
			long row = (long)Math.Floor((double)bytePosition / (double)_iHexMaxHBytes);
			long column = (bytePosition + _iHexMaxHBytes - _iHexMaxHBytes * (row + 1));

			return new PointL(column, row);
        }

		PointF PosToPhyPoint(long bytePosition)
        {
			PointL gp = PosToLogPoint(bytePosition);

			return LogToPhyPoint(gp);
        }

		PointF PosToPhyPointASCII(long bytePosition)
		{
			PointL gp = PosToLogPoint(bytePosition);

			return LogToPhyPointASCII(gp);
		}

		PointF LogToPhyPoint(PointL pt)
        {
            float x = (3 * _charSize.Width) * pt.X + _recHex.X;
            float y = (pt.Y + 1) * _charSize.Height - _charSize.Height + _recHex.Y;

            return new PointF(x, y);
        }		

		PointF LogToPhyPointASCII(PointL pt)
		{
			float x = (_charSize.Width) * pt.X + _recChars.X;
			float y = (pt.Y + 1) * _charSize.Height - _charSize.Height + _recChars.Y;

			return new PointF(x, y);
		}
	

		int ToScrollMax(long value)
		{
			long max = 65535;
			return (value > max) ? (int)max : (int)value;
		}

		void VScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			switch (e.Type)
			{
				case ScrollEventType.SmallIncrement:
					VScrollLineDown();
					break;
				case ScrollEventType.SmallDecrement:
					VScrollLineUp();
					break;
				case ScrollEventType.LargeIncrement:
					VScrollPageDown();
					break;
				case ScrollEventType.LargeDecrement:
					VScrollPageUp();
					break;
				case ScrollEventType.ThumbPosition:
					VScrollSetThumpPosition(FromScrollPos(e.NewValue));
					break;
				case ScrollEventType.ThumbTrack:
					if (_thumbTrackVTimer.Enabled)
						_thumbTrackVTimer.Enabled = false;

					int currentThumbTrack = Environment.TickCount;

					if (currentThumbTrack - _lastThumbTrackMS > ThumbTrackMS)
					{
						_lastThumbTrackMS = currentThumbTrack;
						VScrollThumbTrack(null, null);
					}
					else
					{
						_thumbTrackVPosition = FromScrollPos(e.NewValue);
						_thumbTrackVTimer.Enabled = true;
					}

					break;
				default:
					break;
			}

			e.NewValue = ToScrollVPos(_scrollVpos);
		}

		void HScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			switch (e.Type)
			{
				case ScrollEventType.SmallIncrement:
					HScrollColumnRight();
					break;
				case ScrollEventType.SmallDecrement:
					HScrollColumnLeft();
					break;
				case ScrollEventType.LargeIncrement:
					HScrollLargeRight();
					break;
				case ScrollEventType.LargeDecrement:
					HScrollLargeLeft();
					break;
				case ScrollEventType.ThumbPosition:
					HScrollSetThumpPosition(FromScrollPos(e.NewValue));
					break;
				case ScrollEventType.ThumbTrack:
					if (_thumbTrackHTimer.Enabled)
						_thumbTrackHTimer.Enabled = false;

					int currentThumbTrack = Environment.TickCount;

					if (currentThumbTrack - _lastThumbTrackMS > ThumbTrackMS)
					{
						_lastThumbTrackMS = currentThumbTrack;
						HScrollThumbTrack(null, null);
					}
					else
					{
						_thumbTrackHPosition = FromScrollPos(e.NewValue);
						_thumbTrackHTimer.Enabled = true;
					}

					break;
				default:
					break;
			}

			e.NewValue = ToScrollVPos(_scrollHpos);
		}

		void UpdateScrollBars()
		{
			if (_byteData == null || _byteData.Length == 0)
			{
				_scrollVmin = 0;
				_scrollVmax = 0;
				_scrollVpos = 0;
				_scrollHmin = 0;
				_scrollHmax = 0;
				_scrollHpos = 0;
			}
			else
			{
				long scrollVmax = (long)Math.Ceiling((double)(_byteData.Length + 1) / (double)_iHexMaxHBytes - (double)_iHexMaxVBytes);
				scrollVmax = Math.Max(0, scrollVmax);

				long scrollpos = _startByte / _iHexMaxHBytes;

				if (scrollVmax < _scrollVmax)
				{
					/* Data size has been decreased. */
					if (_scrollVpos == _scrollVmax)
						/* Scroll one line up if we at bottom. */
						VScrollLineUp();
				}

				if (scrollVmax != _scrollVmax || scrollpos != _scrollVpos)
				{
					_scrollVmin = 0;
					_scrollVmax = scrollVmax;
					_scrollVpos = Math.Min(scrollpos, scrollVmax);
				}

				int iMinVisibleChars = 9 + _iHexMaxHBytes * 3 + 1;
				if (_drawCharacters) iMinVisibleChars += _iHexMaxHBytes;
				int iMaxVisibleChars = (int)Math.Floor((double)ClientRectangle.Width / this._charSize.Width);
				if (_scrollVmax > 0) iMaxVisibleChars -= (int)Math.Ceiling(vScrollBar.Width / this._charSize.Width);
				int iScrollHmax = Math.Max(0, iMinVisibleChars - iMaxVisibleChars);

				_scrollHmin = 0;
				_scrollHmax = iScrollHmax;
				_scrollHpos = Math.Min(_scrollHpos, iScrollHmax);
			}

			UpdateVScroll();
			UpdateHScroll();
		}

		void UpdateVScroll()
		{
			int max = ToScrollMax(_scrollVmax);
			vScrollBar.Visible = (max > 0 ? true : false);

			if (max > 0)
			{
				vScrollBar.Minimum = 0;
				vScrollBar.Maximum = max;
				vScrollBar.Value = Math.Min(ToScrollVPos(_scrollVpos), max);
				_thumbTrackVPosition = vScrollBar.Value; 
			}
		}

		void UpdateHScroll()
		{
			int max = ToScrollMax(_scrollHmax);
			hScrollBar.Visible = (max > 0 ? true : false);

			if (max > 0)
			{
				hScrollBar.Minimum = 0;
				hScrollBar.Maximum = max;
				hScrollBar.Value = ToScrollHPos(_scrollHpos);
				_thumbTrackHPosition = hScrollBar.Value;
			}
		}

		int ToScrollVPos(long value)
		{
			int max = 65535;

			if (_scrollVmax < max)
				return (int)value;
			else
			{
				double valperc = (double)value / (double)_scrollVmax * (double)100;
				int res = (int)Math.Floor((double)max / (double)100 * valperc);
				res = (int)Math.Max(_scrollVmin, res);
				res = (int)Math.Min(_scrollVmax, res);
				return res;
			}
		}

		int ToScrollHPos(long value)
		{
			int max = 65535;

			if (_scrollHmax < max)
				return (int)value;
			else
			{
				double valperc = (double)value / (double)_scrollHmax * (double)100;
				int res = (int)Math.Floor((double)max / (double)100 * valperc);
				res = (int)Math.Max(_scrollHmin, res);
				res = (int)Math.Min(_scrollHmax, res);
				return res;
			}
		}
		
		long FromScrollPos(int value)
		{
			int max = 65535;
			if (_scrollVmax < max)
			{
				return (long)value;
			}
			else
			{
				double valperc = (double)value / (double)max * (double)100;
				long res = (int)Math.Floor((double)_scrollVmax / (double)100 * valperc);
				return res;
			}
		}

		void VScrollToLine(long newLineNumber)
		{
			_scrollVpos = Math.Min(newLineNumber, _scrollVmax);

			UpdateVScroll();
			UpdateVisibilityBytes();
			UpdateCaret();
			Invalidate();
		}

		void VScrollLines(int lines)
		{
			long pos;
			if (lines > 0)
			{
				pos = Math.Min(_scrollVmax, _scrollVpos + lines);
			}
			else if (lines < 0)
			{
				pos = Math.Max(_scrollVmin, _scrollVpos + lines);
			}
			else
			{
				return;
			}

			VScrollToLine(pos);
		}

		void VScrollLineDown()
		{
			this.VScrollLines(1);
		}

		void VScrollLineUp()
		{
			this.VScrollLines(-1);
		}

		void VScrollPageDown()
		{
			this.VScrollLines(_iHexMaxVBytes);
		}

		void VScrollPageUp()
		{
			this.VScrollLines(-_iHexMaxVBytes);
		}

		void VScrollSetThumpPosition(long scrollPos)
		{
			VScrollToLine(ToScrollVPos(scrollPos));
		}

		void VScrollThumbTrack(object sender, EventArgs e)
		{
			_thumbTrackVTimer.Enabled = false;
			VScrollSetThumpPosition(_thumbTrackVPosition);
			_lastThumbTrackMS = Environment.TickCount;
		}

		void HScrollToColumn(long newColumnNumber)
		{
			if (newColumnNumber < _scrollHmin || newColumnNumber > _scrollHmax || newColumnNumber == _scrollHpos)
				return;

			_scrollHpos = newColumnNumber;

			this.AdjustWindowSize();
			UpdateCaret();
			Invalidate();
		}
	
		void HScrollColumns(int columns)
		{
			long pos;
			if (columns > 0)
			{
				pos = Math.Min(_scrollHmax, _scrollHpos + columns);
			}
			else if (columns < 0)
			{
				pos = Math.Max(_scrollHmin, _scrollHpos + columns);
			}
			else
			{
				return;
			}

			HScrollToColumn(pos);
		}

		void HScrollColumnRight()
		{
			this.HScrollColumns(1);
		}

		void HScrollColumnLeft()
		{
			this.HScrollColumns(-1);
		}

		void HScrollLargeRight()
		{
			this.HScrollColumns(hScrollBar.LargeChange);
		}

		void HScrollLargeLeft()
		{
			this.HScrollColumns(-hScrollBar.LargeChange);
		}

		void HScrollSetThumpPosition(long scrollPos)
		{
			HScrollToColumn(ToScrollHPos(scrollPos));
		}

		void HScrollThumbTrack(object sender, EventArgs e)
		{
			_thumbTrackHTimer.Enabled = false;
			HScrollSetThumpPosition(_thumbTrackHPosition);
			_lastThumbTrackMS = Environment.TickCount;			
		}

		void ScrollByteIntoView()
		{
			ScrollByteIntoView(_bytePos);
		}

		void ScrollByteIntoView(long bytePos)
		{
			if (_byteData == null)
				return;

			if (bytePos >= _startByte && bytePos <= _endByte)
			{
				UpdateCaret();
				return;
			}

			long row = (long)Math.Floor((double)bytePos / (double)_iHexMaxHBytes);
			int column = (int)(bytePos + _iHexMaxHBytes - _iHexMaxHBytes * (row + 1));

			VScrollToLine(row);
			HScrollToColumn(column);
		}

		void CreateCaret()
		{
			if (_byteData == null || this._caretVisible || !this.Focused)
				return;

			int caretWidth = (this._insertMode == InsertKeyMode.Insert) ? 1 : (int)_charSize.Width;
			int caretHeight = (int)_charSize.Height;
			//if (Environment.OSVersion.Platform != PlatformID.Unix &&
            //    Environment.OSVersion.Platform != PlatformID.MacOSX)
			NativeMethods.CreateCaret(Handle, IntPtr.Zero, caretWidth, caretHeight);

			UpdateCaret();

			NativeMethods.ShowCaret(Handle);

			this._caretVisible = true;
		}

		void UpdateCaret()
		{
			if (_byteData == null)
				return;			

			PointF p = new PointF();

			long bytePosition = _bytePos - _startByte;

			switch (_clickArea)
			{
				case ClickAreas.AREA_NONE:
				case ClickAreas.AREA_ADDRESS:
				case ClickAreas.AREA_BYTES:
					{
						p = this.PosToPhyPoint(bytePosition);
						if (_isNibble && !_lMouseDown)
							p.X += _charSize.Width;
						NativeMethods.SetCaretPos((int)p.X, (int)p.Y);
						_enterMode = EnterMode.BYTES;
						break;
					}
				case ClickAreas.AREA_CHARS:
					{
						p = this.PosToPhyPointASCII(bytePosition);
						NativeMethods.SetCaretPos((int)p.X, (int)p.Y);
						_enterMode = EnterMode.CHARS;
						break;
					}
				default: break;
			}

			OnBytePositionChanged(EventArgs.Empty);
		}

		void DestroyCaret()
		{
			if (!_caretVisible)
				return;

			NativeMethods.DestroyCaret();
			_caretVisible = false;
		}

		void UpdateCaretPosition(Point p, bool bSnap = false )
		{
			if (_byteData == null)
				return;

			GetBytePosFromPoint(p, ref _bytePos, ref this._isNibble);
			if (bSnap) this._isNibble = false;
			UpdateCaret();
			Invalidate();
		}

		void GetBytePosFromPoint(Point p, ref long _bytePos, ref bool _isNibble)
		{
			int xPos = 0;
			int yPos = 0;

			if (_recHex.Contains(p))
			{
				_isNibble = (((int)((p.X - _recHex.X) / _charSize.Width)) % 3) > 0;

				xPos = (int)((p.X - _recHex.X) / _charSize.Width / 3);
				yPos = (int)((p.Y - _recHex.Y) / _charSize.Height) + 1;

				_bytePos = _startByte + _iHexMaxHBytes * yPos - _iHexMaxHBytes + xPos;

				if (_bytePos < 0)
					_bytePos = 0;

				if (_bytePos > _byteData.Length)
					_isNibble = false;
			}

			if (_recChars.Contains(p))
			{
				_isNibble = false;

				xPos = (int)((p.X - _recChars.X) / _charSize.Width);
				yPos = (int)((p.Y - _recChars.Y) / _charSize.Height) + 1;

				_bytePos = _startByte + (_iHexMaxHBytes * yPos - _iHexMaxHBytes) + xPos;
			
				if (_bytePos < 0)
					_bytePos = 0;
			}

			_bytePos = Math.Min(_bytePos, this._readOnly ? _byteData.Length - 1 : _byteData.Length);

			System.Diagnostics.Debug.WriteLine("BytePos: " + _bytePos + " IsNibble: " + _isNibble);
		}

		void SetCaretPosition(long newBytePosition, bool isNibble)
		{
			if (isNibble != _isNibble)
				_isNibble = isNibble;

			newBytePosition = Math.Max(newBytePosition, 0);
			newBytePosition = Math.Min(newBytePosition, _byteData.Length);
			
			if (_readOnly)
				newBytePosition = Math.Min(newBytePosition, _byteData.Length - 1);

			if (newBytePosition != _bytePos)
				_bytePos = newBytePosition;
		}

		public bool HasSelection()
		{
			return (_startSelection >= 0 && _endSelection >= 0);
		}

		public void RemoveSelection()
		{
			if (this.HasSelection())
				OnSelectionChanged(EventArgs.Empty);

			_startSelection = -1;
			_endSelection = -1;			
			
			Invalidate();
		}

		public void SetSelection(long selStart, long selEnd)
		{
			RemoveSelection();

			long tmpStartSelection = Math.Min(selStart, _byteData.Length - 1);
			long tmpEndSelection = Math.Min(selEnd, _byteData.Length - 1);

			if (tmpStartSelection > tmpEndSelection)
			{
				long tmp = tmpStartSelection;
				tmpStartSelection = tmpEndSelection;
				tmpEndSelection = tmp;
			}

			_startSelection = Math.Max(0, tmpStartSelection);
			_bytePos = _endSelection = Math.Min(tmpEndSelection, _byteData.Length - 1);
			
			UpdateCaret();
			Invalidate();

			if (this.HasSelection())
				OnSelectionChanged(EventArgs.Empty);
		}

		public void SelectAll()
		{
			RemoveSelection();

			_startSelection = 0;
			_bytePos = _endSelection = _byteData.Length - 1;

			UpdateCaret();
			Invalidate();

			if (this.HasSelection())
				OnSelectionChanged(EventArgs.Empty);			
		}

		public long GetSelectionLength()
		{
			if (_endSelection > _startSelection)
				return _endSelection - _startSelection + 1;
			else
				return _startSelection - _endSelection + 1;
		}

		public override bool PreProcessMessage(ref System.Windows.Forms.Message msg)
		{
			const int WM_KEYDOWN = 0x100;
			const int WM_KEYUP = 0x101;
			const int WM_CHAR = 0x102;

			switch (msg.Msg)
			{
				case WM_KEYDOWN:				
					return PreProcessWmKeyDown(ref msg);
				case WM_KEYUP:
					return PreProcessWmKeyUp(ref msg);
				case WM_CHAR:
					return PreProcessWmChar(ref msg);
				default:
					return base.PreProcessMessage(ref msg);
			}
		}
		
		protected bool PreProcessWmKeyDown(ref Message msg)
		{
			Keys key = (Keys)msg.WParam.ToInt32();
			Keys keyData = key | Control.ModifierKeys;

			KeyEventArgs e = new KeyEventArgs(keyData);
			this.OnKeyDown(e);
			if (e.Handled) return true;

			switch (keyData)
			{
				case Keys.C | Keys.Control:
				{
					this.Copy();
					return true;
				}
				case Keys.V | Keys.Control:
				{
					this.Paste();
					return true;
				}
				case Keys.X | Keys.Control:
				{
					this.Cut();
					return true;
				}
			}

			switch (key) 
			{
				case Keys.Up:
				{
					long currPos = _bytePos;

					currPos = currPos - _iHexMaxHBytes;

					if (_isShiftActive)
					{
						currPos = Math.Max(0, currPos);
						_endSelection = currPos;
						OnSelectionChanged(EventArgs.Empty);
					}

					SetCaretPosition(currPos, _isNibble);

					if (currPos < _startByte)
						VScrollLineUp();					

					_isNibble = false;
					UpdateCaret();
					Invalidate();						

					ScrollByteIntoView();

					if (!_isShiftActive)
						this.RemoveSelection();						

					return true;
				}
				case Keys.Down:
				{
					long currPos = _bytePos;
					
					currPos = currPos + _iHexMaxHBytes;

					if (_isShiftActive)
					{
						currPos = Math.Min(_byteData.Length - 1, currPos);
						_endSelection = currPos;
						OnSelectionChanged(EventArgs.Empty);
					}

					SetCaretPosition(currPos, _isNibble);

					if (currPos > _endByte)
						VScrollLineDown();				

					_isNibble = false; 
					UpdateCaret();
					Invalidate();

					ScrollByteIntoView();

					if (!_isShiftActive)
						this.RemoveSelection();

					return true;
				}
				case Keys.Right:
				{
					long currPos = _bytePos;

					currPos++;

					if (_isShiftActive)
					{
						currPos = Math.Min(_byteData.Length - 1, currPos);
						_endSelection = currPos;
						OnSelectionChanged(EventArgs.Empty);
					}
						
					SetCaretPosition(currPos, _isNibble);

					if (currPos > _endByte)
						VScrollLineDown();					

					_isNibble = false;
					UpdateCaret();
					Invalidate();

					ScrollByteIntoView();

					if (!_isShiftActive)
						this.RemoveSelection();
						
					return true;
				}
				case Keys.Left:
				{
					long currPos = _bytePos;

					currPos--;

					if (_isShiftActive)
					{
						currPos = Math.Max(0, currPos);
						_endSelection = currPos;
						OnSelectionChanged(EventArgs.Empty);
					}

					SetCaretPosition(currPos, _isNibble);

					if (currPos < _startByte)
						VScrollLineUp();

					_isNibble = false;
					UpdateCaret();
					Invalidate();

					ScrollByteIntoView();

					if (!_isShiftActive)
						this.RemoveSelection();

					return true;
				}
				case Keys.PageDown:
				{
					long currPos = _bytePos;
					currPos += _iHexMaxBytes;					

					_isNibble = false;

					if (_isShiftActive)
					{
						currPos = Math.Min(_byteData.Length - 1, currPos);
						_endSelection = currPos;
						OnSelectionChanged(EventArgs.Empty);
					}

					SetCaretPosition(currPos, _isNibble);

					if (currPos > _endByte)
						VScrollPageDown();

					if (!_isShiftActive)
						this.RemoveSelection();

					UpdateCaret();
					Invalidate();

					return true;
				}
				case Keys.PageUp:
				{
					long currPos = _bytePos;
					currPos -= _iHexMaxBytes;					

					_isNibble = false;

					if (_isShiftActive)
					{
						currPos = Math.Max(0, currPos);
						_endSelection = currPos;
						OnSelectionChanged(EventArgs.Empty);
					}

					SetCaretPosition(currPos, _isNibble);

					if (currPos < _startByte)
						VScrollPageUp();

					if (_isShiftActive)
						_endSelection = currPos;

					if (!_isShiftActive)
						this.RemoveSelection();

					UpdateCaret();
					Invalidate();

					return true;
				}
				case Keys.Home:
				{
					long newPos = 0;
					_isNibble = false;
					SetCaretPosition(newPos, _isNibble);
					ScrollByteIntoView(newPos);

					if (_isShiftActive)
					{
						_endSelection = newPos;
						OnSelectionChanged(EventArgs.Empty);
						Invalidate();
					}

					if (!_isShiftActive)
						this.RemoveSelection();			

					return true;
				}
				case Keys.End:
				{
					long newPos = _byteData.Length - 1;
					_isNibble = false;
					SetCaretPosition(newPos, _isNibble);						
					ScrollByteIntoView(newPos);

					if (_isShiftActive)
					{
						_endSelection = newPos;
						OnSelectionChanged(EventArgs.Empty);
						Invalidate();
					}

					if (!_isShiftActive)
						this.RemoveSelection();
					
					return true;
				}
				case Keys.ShiftKey:
				{
					this._isShiftActive = true;
					if (!this.HasSelection())
						_startSelection = Math.Min(_bytePos, _byteData.Length - 1);
						
					return true;
				}
				case Keys.Tab:
				{
					_clickArea ^= ClickAreas.AREA_BYTES ^ ClickAreas.AREA_CHARS;
					_enterMode ^= EnterMode.BYTES ^ EnterMode.CHARS;					
					UpdateCaret();

					return true;
				}
				case Keys.Delete:
				{
					if (this._readOnly)
						return true;
					
					long currPos = _bytePos;

					if (currPos > _byteData.Length - 1)
						return true;

					if (this.HasSelection())
					{
						_byteData.DeleteBytes(_startSelection > _endSelection ? _endSelection : _startSelection, GetSelectionLength());

						SetCaretPosition(_startSelection > _endSelection ? _endSelection : _startSelection, false);
						UpdateCaret();
						RemoveSelection();
					}
					else
					{
						_byteData.DeleteBytes(currPos, 1);
						Invalidate();
					}

					return true;
				}
				case Keys.Back:
				{
					if (this._readOnly || _bytePos <= 0)
						return true;

					if (!this.HasSelection())
					{
						_bytePos--;
						_byteData.DeleteBytes(_bytePos, 1);
						
						SetCaretPosition(_bytePos, false);
						UpdateCaret();
						Invalidate();
					}
					else
						this.Cut();

					return true;
				}
				default:
				{
					return false;
				}
			}
		}

		protected bool PreProcessWmKeyUp(ref Message msg)
		{
			Keys key = (Keys)msg.WParam.ToInt32();
			Keys keyData = key | Control.ModifierKeys;			

			KeyEventArgs e = new KeyEventArgs(keyData);
			this.OnKeyUp(e);
			if (e.Handled) return true;

			switch (key)
			{
				case Keys.ShiftKey:
				{
					this._isShiftActive = false;
					return true;
				}
				case Keys.Insert:
				{
					this.InsertMode ^= InsertKeyMode.Insert ^ InsertKeyMode.Overwrite;					
					return true;
				}
				default:
					return false;
			}
		}

		protected bool PreProcessWmChar(ref Message msg)
		{
			byte currByte = 0;
			char ch = (char)msg.WParam.ToInt32();

			if (_readOnly)
				return true;

			if (_bytePos == _byteData.Length)
				this.InsertMode = InsertKeyMode.Insert;

			if (_enterMode == EnterMode.BYTES)
			{
				byte charByte = Convert.ToByte(Char.ToUpper(ch));
				if (charByte >= 0x41) charByte = (byte)(charByte - 0x17);

				if (!Uri.IsHexDigit(ch))
					return false;

				if (this._insertMode == InsertKeyMode.Insert)
				{

					if (_isNibble)
					{
						currByte = _byteData.ReadByte(_bytePos);
						currByte = (byte)((byte)(currByte & 0xF0) | charByte & 0xF);
						_byteData.WriteByte(_bytePos, currByte);
					}
					else
					{
						currByte = 0;
						currByte = (byte)((byte)(currByte & 0xF) | charByte << 4);
						_byteData.InsertBytes(_bytePos, new byte[] { currByte });
					}
					
					if (_isNibble) _bytePos++;
					_isNibble = !_isNibble;
				}
				else
				{
					currByte = _byteData.ReadByte(_bytePos);

					if (_isNibble)
						currByte = (byte)((byte)(currByte & 0xF0) | charByte & 0xF);
					else
						currByte = (byte)((byte)(currByte & 0xF) | charByte << 4);

					_byteData.WriteByte(_bytePos, currByte);
					if (_isNibble) _bytePos++;
					_isNibble = !_isNibble;
				}
			}
			else if (_enterMode == EnterMode.CHARS)
			{
				byte charByte = Convert.ToByte(ch);
				
				if (this._insertMode == InsertKeyMode.Insert)
					_byteData.InsertBytes(_bytePos, new byte[] { charByte });
				else
					_byteData.WriteByte(_bytePos, charByte);

				_bytePos++;
			}

			UpdateCaret();
			Invalidate();

			return true;
		}

		bool GetCopyData(ref byte[] byteData)
		{
			if (!this.HasSelection() || _byteData == null)
				return false;

			long counter = 0;

			byteData = new Byte[GetSelectionLength()];

			long lStartIdx = Math.Min(_startSelection, _endSelection);

			for (long pos = lStartIdx; pos < GetSelectionLength() + lStartIdx; pos++)
			{
				byte b = _byteData.ReadByte(pos);
				byteData[counter] = _byteData.ReadByte(pos);
				counter++;
			}

			return (counter == GetSelectionLength());
		}

		public bool Copy()
		{
			bool bRet = false;

			if (!this.HasSelection() || _byteData == null)
				return bRet;

			byte[] byteCopyData = null;

			try
			{
				if (!GetCopyData(ref byteCopyData))
					return false;

				DataObject clip_DO = new DataObject();
				clip_DO.SetData(DataFormats.Text, System.Text.Encoding.ASCII.GetString(byteCopyData));

				using (MemoryStream ms = new MemoryStream())
				{
					ms.Write(byteCopyData, 0, byteCopyData.Length);
					ms.Seek(0, SeekOrigin.Begin);
					clip_DO.SetData("rawbinary", false, ms);
					Clipboard.SetDataObject(clip_DO, true);
				}						

				bRet = true;
			}
			catch (Exception ex) 
			{
				MessageBox.Show("Failed to copy data to clipboard.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return bRet;
		}

		public bool Paste()
		{
			bool bRet = false;
			byte[] bytePasteData = null;

			if (_byteData == null || this._readOnly)
				return bRet;

			try
			{
				if (this.HasSelection())
				{
					_byteData.DeleteBytes(_startSelection > _endSelection ? _endSelection : _startSelection, GetSelectionLength());
					RemoveSelection();
				}

				DataObject clip_DO = Clipboard.GetDataObject() as DataObject;

				if (clip_DO == null)
					return bRet;

				if (clip_DO.GetDataPresent("rawbinary"))
				{
					MemoryStream ms = clip_DO.GetData("rawbinary") as MemoryStream;

					if (ms == null)
						return bRet;

					bytePasteData = new byte[ms.Length];
					ms.Read(bytePasteData, 0, bytePasteData.Length);
				}
				else if (clip_DO.GetDataPresent(typeof(string)))
				{
					string sBuffer = clip_DO.GetData(typeof(string)) as String;
					bytePasteData = System.Text.Encoding.ASCII.GetBytes(sBuffer);
				}
				else
					return bRet;

				_byteData.InsertBytes(_bytePos, bytePasteData);

				SetCaretPosition(_bytePos + bytePasteData.Length, false);
				UpdateCaret();

				Invalidate();

				bRet = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to paste data from clipboard.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			
			return bRet;
		}

		public bool Cut()
		{
			bool bRet = false;

			if (!this.HasSelection() || this._readOnly || _byteData == null)
				return bRet;

			try
			{
				bRet = Copy();
				if (bRet)
				{
					_byteData.DeleteBytes(_startSelection > _endSelection ? _endSelection : _startSelection, GetSelectionLength());

					SetCaretPosition(_startSelection > _endSelection ? _endSelection : _startSelection, false);
					UpdateCaret();
					RemoveSelection();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to cut data and copy it to the clipboard.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}			

			return bRet;
		}

		private void SetInternalContextMenu()
		{
			if (this._internalContextMenu == null)
			{
				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RPHexEditorUC));
				this._internalContextMenu = new ContextMenuStrip();				

				this._internalCutMenuItem = new ToolStripMenuItem();
				this._internalCopyMenuItem = new ToolStripMenuItem();
				this._internalPasteMenuItem = new ToolStripMenuItem();
				this._internalSeparatorMenuItem_1 = new ToolStripSeparator();
				this._internalSelectAllMenuItem = new ToolStripMenuItem();

				this._internalCutMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
				this._internalCutMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
				this._internalCutMenuItem.Name = "cutToolStripMenuItem";
				this._internalCutMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
				this._internalCutMenuItem.Size = new System.Drawing.Size(164, 22);
				this._internalCutMenuItem.Text = "Cu&t";
				this._internalCutMenuItem.Click += new System.EventHandler(this.InternalContextMenuCut_Click);
				this._internalContextMenu.Items.Add(_internalCutMenuItem);
				
				this._internalCopyMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
				this._internalCopyMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
				this._internalCopyMenuItem.Name = "copyToolStripMenuItem";
				this._internalCopyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
				this._internalCopyMenuItem.Size = new System.Drawing.Size(164, 22);
				this._internalCopyMenuItem.Text = "&Copy";
				this._internalCopyMenuItem.Click += new System.EventHandler(this.InternalContextMenuCopy_Click);
				this._internalContextMenu.Items.Add(_internalCopyMenuItem);

				this._internalPasteMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
				this._internalPasteMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
				this._internalPasteMenuItem.Name = "pasteToolStripMenuItem";
				this._internalPasteMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
				this._internalPasteMenuItem.Size = new System.Drawing.Size(164, 22);
				this._internalPasteMenuItem.Text = "&Paste";
				this._internalPasteMenuItem.Click += new System.EventHandler(this.InternalContextMenuPaste_Click);
				this._internalContextMenu.Items.Add(_internalPasteMenuItem);

				this._internalSeparatorMenuItem_1.Name = "toolStripSeparator_1";
				this._internalSeparatorMenuItem_1.Size = new System.Drawing.Size(161, 6);
				this._internalContextMenu.Items.Add(_internalSeparatorMenuItem_1);


				this._internalSelectAllMenuItem.Name = "selectAllToolStripMenuItem";
				this._internalSelectAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
				this._internalSelectAllMenuItem.Size = new System.Drawing.Size(164, 22);
				this._internalSelectAllMenuItem.Text = "Select &All";
				this._internalSelectAllMenuItem.Click += new System.EventHandler(this.InternalContextMenuSelectAll_Click);
				this._internalContextMenu.Items.Add(_internalSelectAllMenuItem);

				this._internalContextMenu.Opening += new CancelEventHandler(InternalContextMenu_Opening);

				if (this._byteData == null && this.ContextMenuStrip == this._internalContextMenu)
					this.ContextMenuStrip = null;
				else
					if (this._byteData != null && this.ContextMenuStrip == null)
						this.ContextMenuStrip = _internalContextMenu;
			}
		}

		private void InternalContextMenuCut_Click(object sender, EventArgs e)
		{
			this.Cut();
		}

		private void InternalContextMenuCopy_Click(object sender, EventArgs e)
		{
			this.Copy();
		}

		private void InternalContextMenuPaste_Click(object sender, EventArgs e)
		{
			this.Paste();
		}

		private void InternalContextMenuSelectAll_Click(object sender, EventArgs e)
		{
			this.SelectAll();
		}

		void InternalContextMenu_Opening(object sender, CancelEventArgs e)
		{
			_internalCutMenuItem.Enabled = HasSelection() && _byteData != null && !ReadOnly && Enabled;
			_internalCopyMenuItem.Enabled = HasSelection() && _byteData != null;

			DataObject clip_DO = Clipboard.GetDataObject() as DataObject;

			if (clip_DO == null || _byteData == null || ReadOnly || !Enabled)
				_internalPasteMenuItem.Enabled = false;
			else
				_internalPasteMenuItem.Enabled = clip_DO.GetDataPresent("rawbinary") || clip_DO.GetDataPresent(typeof(string));
			_internalSelectAllMenuItem.Enabled = _byteData != null && Enabled;
		}

		public void CommitChanges()
		{
			_byteData.CommitChanges();
		}
    }

	enum ClickAreas { AREA_NONE, AREA_ADDRESS, AREA_BYTES, AREA_CHARS }
	enum EnterMode { BYTES, CHARS }
	enum Encoder { ANSI, EBDIC }

	internal class PointL
	{
		private long _X;
		private long _Y;
		
		public PointL()
		{
			_X = 0;
			_Y = 0;
		}

		public PointL(long X, long Y)
		{
			_X = X;
			_Y = Y;
		}

		public long X
		{
			get { return _X; }
			set { _X = value; }
		}

		public long Y
		{
			get { return _Y; }
			set { _Y = value; }
		}

	}

	internal static class NativeMethods
	{
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool ShowCaret(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool DestroyCaret();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetCaretPos(int X, int Y);
	}

	internal interface IRPHexEditorCharEncoder	{
		char ToChar(byte b);
		byte ToByte(char c);
	}

	internal class RPHexEditorCharANSIEncoder : IRPHexEditorCharEncoder
	{
		public virtual char ToChar(byte b)
		{
			return b > 0x1F && !(b > 0x7E && b < 0xA0) ? (char)b : '.';
		}

		public virtual byte ToByte(char c)
		{
			return (byte)c;
		}
	}

	internal class RPHexEditorCharEBCDICEncoder : IRPHexEditorCharEncoder
	{
		private Encoding _ebcdicEncoding = Encoding.GetEncoding(500);
		public virtual char ToChar(byte b)
		{
			string encoded = _ebcdicEncoding.GetString(new byte[] { b });
			return encoded.Length > 0 ? encoded[0] : '.';
		}

		public virtual byte ToByte(char c)
		{
			byte[] decoded = _ebcdicEncoding.GetBytes(new char[] { c });
			return decoded.Length > 0 ? decoded[0] : (byte)0;
		}
	}
}
