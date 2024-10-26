﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class SpoilNodeElement : BaseNodeElement
	{
		protected override Brush CleanBgBrush { get { return spoilBGBrush; } }
		private static Brush spoilBGBrush = new SolidBrush(Color.FromArgb(190, 217, 212));

        private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private string InputName { get { return DisplayedNode.InputItem.FriendlyName; } }

		private new readonly ReadOnlySpoilNode DisplayedNode;

		public SpoilNodeElement(ProductionGraphViewer graphViewer, ReadOnlySpoilNode node) : base(graphViewer, node)
		{
			Width = MinWidth;
			Height = BaseSimpleHeight;
			DisplayedNode = node;
		}

		protected override Bitmap NodeIcon() { return IconCache.GetSpoilageIcon(); }

        protected override void DetailsDraw(Graphics graphics, Point trans)
        {
			//text
			bool overproducing = DisplayedNode.IsOverproducing();
			Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 40, trans.Y - (Height / 2) + (overproducing ? 32 : 27), (Width - 10 - 40), Height - (overproducing ? 64 : 54));
			//graphics.DrawRectangle(devPen, textSlot);

			int textLength;

			if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low)
				textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, InputName + " Spoilage", BaseFont, textSlot);
			else
				textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, BuildingQuantityToText(DisplayedNode.ActualSetValue) + " stacks", CounterBaseFont, textSlot);

            //spoilage icon
            graphics.DrawImage(IconCache.GetSpoilageIcon(), trans.X - Math.Min((Width / 2) - 10, (textLength / 2) + 32), trans.Y - 16, 32, 32);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive)
		{
			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			if (exclusive)
			{
				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = string.Format("Left click on this node to edit the throughput of {0} Spoilage.\nxN quantity lists number of slots required for throughput.\nRight click for options.", InputName);
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				tooltips.Add(helpToolTipInfo);
			}

			return tooltips;
		}
	}
}