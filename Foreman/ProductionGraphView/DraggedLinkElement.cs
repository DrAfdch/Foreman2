﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Foreman
{
	public enum DragType { MouseDown, MouseUp }

	class DraggedLinkElement : GraphElement
	{
		public NodeElement SupplierElement { get; set; }
		public NodeElement ConsumerElement { get; set; }
		public Item Item { get; set; }
		public LinkType StartConnectionType { get; private set; }
		public DragType DragType;
		private Point newObjectLocation;

		private Point pointM, pointM2;
		private Point pointN, pointN2;
		private Point pointMidM, pointMidA, pointMidB, pointMidN;
		private bool linkingUp;

		public override Point Location
		{
			get { return new Point(); }
			set { }
		}
		public override int X
		{
			get { return 0; }
			set { }
		}
		public override int Y
		{
			get { return 0; }
			set { }
		}
		public override Point Size
		{
			get { return new Point(); }
			set { }
		}
		public override int Width
		{
			get { return 0; }
			set { }
		}
		public override int Height
		{
			get { return 0; }
			set { }
		}

		public DraggedLinkElement(ProductionGraphViewer parent, NodeElement startNode, LinkType startConnectionType, Item item)
			: base(parent)
		{
			if (startConnectionType == LinkType.Input)
			{
				ConsumerElement = startNode;
			}
			else
			{
				SupplierElement = startNode;
			}
			StartConnectionType = startConnectionType;
			Item = item;
			if ((Control.MouseButtons & MouseButtons.Left) != 0)
			{
				DragType = DragType.MouseDown;
			}
			else
			{
				DragType = DragType.MouseUp;
			}
		}

		public void UpdateCurve() //updates all points & boundaries (important for occluding objects outside view)
		{
			pointN = Parent.ScreenToGraph(Parent.PointToClient(Cursor.Position));
			pointM = pointN;
			newObjectLocation = pointN;

			//only snap to grid if grid exists and its a free link (not linking 2 existing objects)
			if ((SupplierElement == null || ConsumerElement == null) && Parent.ShowGrid && Parent.CurrentGridUnit > 0)
			{
				int X = pointN.X;
				int Y = pointN.Y;

				X += Math.Sign(X) * Parent.CurrentGridUnit / 2;
				X -= X % Parent.CurrentGridUnit + Width / 2;

				Y += Math.Sign(Y) * Parent.CurrentGridUnit / 2;
				Y -= Y % Parent.CurrentGridUnit + Height / 2;

				pointN = new Point(X, Y);
				pointM = pointN;
				newObjectLocation = pointN;
			}

			if (SupplierElement != null)
			{
				pointN = SupplierElement.GetOutputLineConnectionPoint(Item);
			}
			if (ConsumerElement != null)
			{
				pointM = ConsumerElement.GetInputLineConnectionPoint(Item);
			}

			linkingUp = (pointN.Y > pointM.Y);
			if (linkingUp)
			{
				//connecting up
				pointN2 = new Point(pointN.X, pointN.Y - Math.Max((int)((pointN.Y - pointM.Y) / 2), 20));
				pointM2 = new Point(pointM.X, pointM.Y + Math.Max((int)((pointN.Y - pointM.Y) / 2), 20));
			}
			else
			{
				int midX = Math.Abs(pointN.X - pointM.X) > 200 ? (pointN.X + pointM.X) / 2 : pointN.X > pointM.X ? pointN.X + 150 : pointN.X - 150;

				pointMidA = new Point(midX, pointN.Y);
				pointMidB = new Point(midX, pointM.Y);


				pointN2 = new Point(pointN.X, pointN.Y - 120);
				pointMidN = new Point(midX, pointN.Y - 120);

				pointM2 = new Point(pointM.X, pointM.Y + 120);
				pointMidM = new Point(midX, pointM.Y + 120);
			}
		}
		public override void Paint(Graphics graphics, Point trans)
		{
			UpdateCurve();

			if (linkingUp)
			{
				//connecting up
				using (Pen pen = new Pen(Item.AverageColor, 3f))
				{
					if (pen.Color.GetBrightness() > 0.8)
						pen.Color = Color.FromArgb((int)(pen.Color.R * 0.8), (int)(pen.Color.G * 0.8), (int)(pen.Color.B * 0.8));
					graphics.DrawBezier(pen, PT(pointN,trans), PT(pointN2, trans), PT(pointM2,trans), PT(pointM,trans));
				}
			}
			else
			{
				using (Pen pen = new Pen(Item.AverageColor, 3f))
				{
					graphics.DrawBezier(pen, pointN, pointN2, pointMidN, pointMidA);
					graphics.DrawLine(pen, pointMidA, pointMidB);
					graphics.DrawBezier(pen, pointMidB, pointMidM, pointM2, pointM);
				}
			}
		}

		public override bool ContainsPoint(Point point)
		{
			return true;
		}

		private void EndDrag(Point location)
		{
			if (SupplierElement != null && ConsumerElement != null)
			{
				if (StartConnectionType == LinkType.Input)
				{
					NodeLink.Create(SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode, this.Item);
				}
				else
				{
					NodeLink.Create(SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode, this.Item);
				}

				Parent.Graph.UpdateNodeValues();
				Parent.AddRemoveElements();
				Parent.UpdateNodes();
				Parent.UpdateGraphBounds();
				Parent.Invalidate();
			}
			else //at least one null
			{
				Point originPoint = new Point(Parent.GraphToScreen(location).X - 150, 15);
				originPoint.X = Math.Max(15, Math.Min(Parent.Width - 650, originPoint.X));

				bool includeSuppliers = StartConnectionType == LinkType.Input && SupplierElement == null;
				bool includeConsumers = StartConnectionType == LinkType.Output && ConsumerElement == null;
				if (includeSuppliers)
					Parent.AddRecipe(originPoint, Item, newObjectLocation, ProductionGraphViewer.NewNodeType.Supplier, ConsumerElement);
				else if (includeConsumers)
					Parent.AddRecipe(originPoint, Item, newObjectLocation, ProductionGraphViewer.NewNodeType.Consumer, SupplierElement);
			}
			Dispose();
		}

		public override void MouseDown(Point location, MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					if (DragType == DragType.MouseUp)
						EndDrag(location);
					break;

				case MouseButtons.Right:
					Dispose();
					break;
			}
		}

		public override void MouseUp(Point location, MouseButtons button, bool wasDragged)
		{
			switch (button)
			{
				case MouseButtons.Left:
					if (DragType == DragType.MouseDown)
						EndDrag(location);
					break;
			}
		}


		public override void MouseMoved(Point location)
		{
			NodeElement mousedElement = Parent.GetElementsAtPoint(location).OfType<NodeElement>().FirstOrDefault();
			if (mousedElement != null)
			{
                if (StartConnectionType == LinkType.Input && mousedElement.DisplayedNode.Outputs.Contains(Item))
                    SupplierElement = mousedElement;
                else if (StartConnectionType == LinkType.Output && mousedElement.DisplayedNode.Inputs.Contains(Item))
                    ConsumerElement = mousedElement;

				//if we have found a possible connection above (both supplier & consumer are no longer null), but the item is temperature dependent AND the temperature check fails, break connection
                if (SupplierElement != null &&
					ConsumerElement != null &&
					Item.IsTemperatureDependent &&
					!LinkElement.IsValidTemperatureConnection(Item, SupplierElement, ConsumerElement))
                {
                    if (StartConnectionType == LinkType.Input)
                        SupplierElement = null;
                    else  //if(StartConnectionType == LinkType.Output)
                        ConsumerElement = null;
                }
            }
			else //no node under mouse, break any previously established connections (ex:when mouse drag leaves a possible connection)
			{
				if (StartConnectionType == LinkType.Input)
					SupplierElement = null;
                else  //if(StartConnectionType == LinkType.Output)
                    ConsumerElement = null;
			}
		}
	}
}