using System;
using System.Collections.Generic;
using System.Drawing;

namespace Packer
{
	public abstract class Bin2D
	{
		public Size size { get; private set; }

		public Dictionary<uint, Rectangle> elements
		{
			get
			{
				return m_Elements;
			}
		}

		public Size margin { get; private set; }

		public MarginType marginType { get; private set; }

		public Size nextSize
		{
			get
			{
				if (currentGrowthState == Bin2D.GrowthState.GrowWidth)
				{
					return new Size(size.Width * 2, size.Height);
				}
				if (currentGrowthState == Bin2D.GrowthState.SwapWidthHeight)
				{
					return new Size(size.Height, size.Width);
				}
				throw new NotImplementedException();
			}
		}

		public Bin2D(Size _startSize, Size _margin, MarginType _marginType)
		{
			size = _startSize;
			margin = _margin;
			marginType = _marginType;
			currentGrowthState = Bin2D.GrowthState.GrowWidth;
			startSize = _startSize;
		}

		public void IncreaseSize()
		{
			size = nextSize;
			currentGrowthState = (GrowthState)(((int)currentGrowthState + 1) % (int)GrowthState.Count);
		}

		public bool InsertElement(uint _id, Size _elementSize)
		{
			if (InsertElement(_id, _elementSize, out var value))
			{
				m_Elements.Add(_id, value);
				return true;
			}
			return false;
		}

		protected abstract bool InsertElement(uint _id, Size _elementSize, out Rectangle _area);

		protected abstract void RetrieveSizes(ref List<Size> _areaList);

		protected abstract void RetrieveIDs(ref List<uint> _idList);

		protected abstract void Reset();

		public void RearrangeBin()
		{
			List<Size> list = new();
			List<uint> list2 = new();
			RetrieveSizes(ref list);
			RetrieveIDs(ref list2);
			bool flag;
			do
			{
				flag = true;
				m_Elements.Clear();
				Reset();
				int count = list.Count;
				int num = 0;
				while (num < count && flag)
				{
					if (!InsertElement(list2[num], list[num]))
					{
						flag = false;
						IncreaseSize();
					}
					num++;
				}
			}
			while (!flag);
		}

		public Size startSize { get; set; }

		private Bin2D.GrowthState currentGrowthState { get; set; }

		private readonly Dictionary<uint, Rectangle> m_Elements = new();

		private enum GrowthState
		{
			GrowWidth,
			SwapWidthHeight,
			Count
		}
	}
}
