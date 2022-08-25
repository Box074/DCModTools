using System;
using System.Collections.Generic;
using System.Drawing;

namespace DCTCommon.Atlas.Packer
{
    public class Bin2DMaxRects : Bin2D
    {
        public Bin2DMaxRects(Size _startSize, Size _margin, MarginType _marginType) : base(_startSize, _margin, _marginType)
        {
            Reset();
        }

        protected override bool InsertElement(uint _id, Size _elementSize, out Rectangle _area)
        {
            _area = default;
            Size size = _elementSize + new Size(1, 1);
            int bestIndexForElement = GetBestIndexForElement(size);
            if (bestIndexForElement == -1)
            {
                return false;
            }
            _area.Size = size;
            _area.Location = m_FreeAreas[bestIndexForElement].area.Location;
            Element element = new()
            {
                area = _area,
                id = _id
            };
            m_UsedAreas.Add(element);
            Rectangle area = m_FreeAreas[bestIndexForElement].area;
            Rectangle rectangle = new(_area.X + _area.Width, _area.Y, area.Width - _area.Width, area.Height);
            if (rectangle.GetArea() > 0)
            {
                m_FreeAreas.Add(new Element(rectangle));
            }
            rectangle = new Rectangle(_area.X, _area.Y + _area.Height, area.Width, area.Height - _area.Height);
            if (rectangle.GetArea() > 0)
            {
                m_FreeAreas.Add(new Element(rectangle));
            }
            m_FreeAreas.RemoveAt(bestIndexForElement);
            List<Rectangle> list = new();
            int i = 0;
            while (i < m_FreeAreas.Count)
            {
                Rectangle this2 = Rectangle.Intersect(m_FreeAreas[i].area, _area);
                if (this2.GetArea() > 0)
                {
                    Rectangle area2 = m_FreeAreas[i].area;
                    Rectangle rectangle2 = new(area2.X, area2.Y, this2.X - area2.X, area2.Height);
                    if (rectangle2.GetArea() > 0)
                    {
                        list.Add(rectangle2);
                    }
                    rectangle2 = new Rectangle(area2.X, this2.Y + this2.Height, area2.Width, area2.Height - (this2.Y - area2.Y + this2.Height));
                    if (rectangle2.GetArea() > 0)
                    {
                        list.Add(rectangle2);
                    }
                    rectangle2 = new Rectangle(area2.X, area2.Y, area2.Width, this2.Y - area2.Y);
                    if (rectangle2.GetArea() > 0)
                    {
                        list.Add(rectangle2);
                    }
                    rectangle2 = new Rectangle(this2.X + this2.Width, area2.Y, area2.Width - (this2.X - area2.X + this2.Width), area2.Height);
                    if (rectangle2.GetArea() > 0)
                    {
                        list.Add(rectangle2);
                    }
                    m_FreeAreas.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            for (int j = 0; j < list.Count; j++)
            {
                m_FreeAreas.Add(new Element(list[j]));
            }
            int k = 0;
            while (k < m_FreeAreas.Count - 1)
            {
                bool flag = false;
                int num = 1;
                while (num < m_FreeAreas.Count && k < m_FreeAreas.Count - 1)
                {
                    if (k == num)
                    {
                        num++;
                    }
                    else if (m_FreeAreas[k].area.Contains(m_FreeAreas[num].area))
                    {
                        m_FreeAreas.RemoveAt(num);
                    }
                    else
                    {
                        if (m_FreeAreas[num].area.Contains(m_FreeAreas[k].area))
                        {
                            m_FreeAreas.RemoveAt(k);
                            flag = true;
                            break;
                        }
                        num++;
                    }
                }
                if (!flag)
                {
                    k++;
                }
            }
            return true;
        }

        protected override void RetrieveSizes(ref List<Size> _sizeList)
        {
            foreach (Element element in m_UsedAreas)
            {
                _sizeList.Add(element.area.Size - margin);
            }
        }

        protected override void RetrieveIDs(ref List<uint> _idList)
        {
            foreach (Element element in m_UsedAreas)
            {
                _idList.Add(element.id);
            }
        }

        protected override void Reset()
        {
            m_FreeAreas = new List<Element>();
            m_UsedAreas = new List<Element>();
            m_FreeAreas.Add(new Element(new Rectangle(0, 0, size.Width, size.Height)));
        }

        private int GetBestIndexForElement(Size _elementSize)
        {
            int num = int.MaxValue;
            int result = -1;
            for (int i = 0; i < m_FreeAreas.Count; i++)
            {
                Rectangle area = m_FreeAreas[i].area;
                if (area.Size.CanFit(_elementSize))
                {
                    int num2 = Math.Min(area.Size.Width - _elementSize.Width, area.Size.Height - _elementSize.Height);
                    if (num2 < num)
                    {
                        result = i;
                        num = num2;
                    }
                }
            }
            return result;
        }

        private List<Element> m_FreeAreas;

        private List<Element> m_UsedAreas;

        private class Element
        {
            public Element()
            {
                area = default;
                id = uint.MaxValue;
            }

            public Element(Rectangle _rectangle)
            {
                area = _rectangle;
                id = uint.MaxValue;
            }

            public Rectangle area;

            public uint id;
        }
    }
}
