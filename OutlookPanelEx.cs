using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Owf.Controls
{
	public partial class OutlookPanelEx : Panel
	{
		int _headerHeight = 25;
		string _headerText = "header title";
		Color _headerColor1 = Color.FromArgb(89, 135, 214);
		Color _headerColor2 = Color.FromArgb(3, 56, 147);
		Font _headerFont = new Font("Arial", 12F, System.Drawing.FontStyle.Bold);
		Image _icon = null;
		Color _iconTransparentColor = Color.White;

		[Browsable(true), Category("Owf")]
		public string HeaderText
		{
			get { return _headerText; }
			set
			{
				_headerText = value;
				Invalidate();
			}
		}

		[Browsable(true), Category("Owf")]
		public Color HeaderColor1
		{
			get { return _headerColor1; }
			set
			{
				_headerColor1 = value;
				Invalidate();
			}
		}

		[Browsable(true), Category("Owf")]
		public Color HeaderColor2
		{
			get { return _headerColor2; }
			set
			{
				_headerColor2 = value;
				Invalidate();
			}
		}

		[Browsable(true), Category("Owf")]
		public Image Icon
		{
			get { return _icon; }
			set
			{
				_icon = value;
				Invalidate();
			}
		}

		[Browsable(true), Category("Owf")]
		public Color IconTransparentColor
		{
			get { return _iconTransparentColor; }
			set
			{
				_iconTransparentColor = value;
				Invalidate();
			}
		}

		//public override Rectangle DisplayRectangle
		//{
		//    get
		//    {
		//        Rectangle clientSize = base.DisplayRectangle;
		//        clientSize.X = 20;
		//        clientSize.Height -= 20;
		//        return clientSize;
		//    }
		//}

		public OutlookPanelEx()
		{
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			InitializeComponent();
			this.Padding = new Padding(5, _headerHeight + 4, 5, 4);
		}

		private void OutlookPanelEx_Paint(object sender, PaintEventArgs e)
		{
			if (_headerHeight > 1)
			{
				// Draw border;
				DrawBorder(e.Graphics);

				// Draw heaeder
				DrawHeader(e.Graphics);

				// Draw text
				DrawText(e.Graphics);

				// Draw Icon
				DrawIcon(e.Graphics);
			}
		}

		private void DrawBorder(Graphics graphics)
		{
			using (Pen pen = new Pen(this._headerColor2))
			{
				graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
			}
		}

		private void DrawHeader(Graphics graphics)
		{
			Rectangle headerRect = new Rectangle(1, 1, this.Width-2, this._headerHeight);
			using (Brush brush = new LinearGradientBrush(headerRect, _headerColor1, _headerColor2, LinearGradientMode.Vertical))
			{
				graphics.FillRectangle(brush, headerRect);
			}
		}

		private void DrawText(Graphics graphics)
		{
			if (!string.IsNullOrEmpty(this._headerText))
			{
				SizeF size = graphics.MeasureString(this._headerText, this._headerFont);
				using (Brush brush = new SolidBrush(Color.White))
				{
					graphics.DrawString(this._headerText, this._headerFont, brush, 5, (_headerHeight - size.Height) / 2);
				}
			}
		}

		private void DrawIcon(Graphics graphics)
		{
			if (this._icon != null)
			{
				Point point = new Point(this.Width - _icon.Width - 2, (_headerHeight - _icon.Height) / 2);
				Bitmap bitmap = new Bitmap(_icon);
				bitmap.MakeTransparent(_iconTransparentColor);
				graphics.DrawImage(bitmap, point);
			}
		}
	}
}
