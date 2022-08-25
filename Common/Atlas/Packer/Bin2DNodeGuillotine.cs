using System;
using System.Collections.Generic;
using System.Drawing;

namespace DCTCommon.Atlas.Packer
{
    public class Bin2DNodeGuillotine
    {
        public Rectangle area { get; set; }

        public bool isLeaf
        {
            get
            {
                return m_LeftChild == null && m_RightChild == null;
            }
        }

        public Bin2DNodeGuillotine(Bin2DGuillotine _bin)
        {
            id = uint.MaxValue;
            m_Bin = _bin;
        }

        public Bin2DNodeGuillotine Insert(uint _id, Size _size, Size _margins, MarginType _marginType)
        {
            if (!isLeaf)
            {
                Bin2DNodeGuillotine bin2DNodeGuillotine = m_LeftChild.Insert(_id, _size, _margins, _marginType);
                if (bin2DNodeGuillotine != null)
                {
                    return bin2DNodeGuillotine;
                }
                return m_RightChild.Insert(_id, _size, _margins, _marginType);
            }
            else
            {
                if (id != 4294967295U)
                {
                    return null;
                }
                Size sizeWithMargin = GetSizeWithMargin(_size, _margins, _marginType);
                if (sizeWithMargin.Width > area.Width || sizeWithMargin.Height > area.Height)
                {
                    return null;
                }
                if (sizeWithMargin.Width == area.Width && sizeWithMargin.Height == area.Height)
                {
                    id = _id;
                    return this;
                }
                m_LeftChild = new Bin2DNodeGuillotine(m_Bin);
                m_RightChild = new Bin2DNodeGuillotine(m_Bin);
                m_LeftChild.m_Border = BorderType.None;
                m_RightChild.m_Border = BorderType.None;
                int num = area.Width - sizeWithMargin.Width;
                int num2 = area.Height - sizeWithMargin.Height;
                if (num > num2)
                {
                    m_LeftChild.m_Border = m_Border & BorderType.Left | m_Border & BorderType.Top | m_Border & BorderType.Bottom;
                    sizeWithMargin = GetSizeWithMargin(_size, _margins, _marginType);
                    m_LeftChild.area = new Rectangle(area.Location, new Size(sizeWithMargin.Width, area.Height));
                    m_RightChild.m_Border = m_Border & BorderType.Right | m_Border & BorderType.Top | m_Border & BorderType.Bottom;
                    m_RightChild.area = new Rectangle(area.Left + sizeWithMargin.Width, area.Top, area.Width - sizeWithMargin.Width, area.Height);
                }
                else
                {
                    m_LeftChild.m_Border = m_Border & BorderType.Left | m_Border & BorderType.Top | m_Border & BorderType.Right;
                    sizeWithMargin = GetSizeWithMargin(_size, _margins, _marginType);
                    m_LeftChild.area = new Rectangle(area.Location, new Size(area.Width, sizeWithMargin.Height));
                    m_RightChild.m_Border = m_Border & BorderType.Left | m_Border & BorderType.Bottom | m_Border & BorderType.Right;
                    m_RightChild.area = new Rectangle(area.Left, area.Top + sizeWithMargin.Height, area.Width, area.Height - sizeWithMargin.Height);
                }
                return m_LeftChild.Insert(_id, _size, _margins, _marginType);
            }
        }

        public void RetrieveSizes(ref List<Size> _sizeList)
        {
            if (isLeaf)
            {
                if (id != 4294967295U)
                {
                    _sizeList.Add(GetAreaWithoutMargin(m_Bin.margin, m_Bin.marginType).Size);
                    return;
                }
            }
            else
            {
                m_LeftChild.RetrieveSizes(ref _sizeList);
                m_RightChild.RetrieveSizes(ref _sizeList);
            }
        }

        private Size GetSizeWithMargin(Size _sizeWithoutMargins, Size _margin, MarginType _marginType)
        {
            Size result = new(_sizeWithoutMargins.Width, _sizeWithoutMargins.Height);
            if ((_marginType == MarginType.OnlyBorder || _marginType == MarginType.All) && (m_Border & BorderType.Left) != BorderType.None)
            {
                result.Width += _margin.Width;
            }
            if (_marginType == MarginType.All || _marginType == MarginType.OnlyBorder && (m_Border & BorderType.Right) != BorderType.None || _marginType == MarginType.NoBorder && (m_Border & BorderType.Right) == BorderType.None)
            {
                result.Width += _margin.Width;
            }
            if ((_marginType == MarginType.OnlyBorder || _marginType == MarginType.All) && (m_Border & BorderType.Top) != BorderType.None)
            {
                result.Height += _margin.Height;
            }
            if (_marginType == MarginType.All || _marginType == MarginType.OnlyBorder && (m_Border & BorderType.Bottom) != BorderType.None || _marginType == MarginType.NoBorder && (m_Border & BorderType.Bottom) == BorderType.None)
            {
                result.Height += _margin.Height;
            }
            return result;
        }

        public Rectangle GetAreaWithoutMargin(Size _margin, MarginType _marginType)
        {
            Rectangle result = new(area.Location, area.Size);
            if ((_marginType == MarginType.OnlyBorder || _marginType == MarginType.All) && (m_Border & BorderType.Left) != BorderType.None)
            {
                result.X += _margin.Width;
                result.Width -= _margin.Width;
            }
            if (_marginType == MarginType.All || _marginType == MarginType.OnlyBorder && (m_Border & BorderType.Right) != BorderType.None || _marginType == MarginType.NoBorder && (m_Border & BorderType.Right) == BorderType.None)
            {
                result.Width -= _margin.Width;
            }
            if ((_marginType == MarginType.OnlyBorder || _marginType == MarginType.All) && (m_Border & BorderType.Top) != BorderType.None)
            {
                result.Y += _margin.Height;
                result.Height -= _margin.Height;
            }
            if (_marginType == MarginType.All || _marginType == MarginType.OnlyBorder && (m_Border & BorderType.Bottom) != BorderType.None || _marginType == MarginType.NoBorder && (m_Border & BorderType.Bottom) == BorderType.None)
            {
                result.Height -= _margin.Height;
            }
            return result;
        }

        public void RetrieveIDs(ref List<uint> _idList)
        {
            if (isLeaf)
            {
                if (id != 4294967295U)
                {
                    _idList.Add(id);
                    return;
                }
            }
            else
            {
                m_LeftChild.RetrieveIDs(ref _idList);
                m_RightChild.RetrieveIDs(ref _idList);
            }
        }

        private uint id { get; set; }

        private BorderType m_Border { get; set; }

        private const uint invalidID = 4294967295U;

        private Bin2DNodeGuillotine m_LeftChild;

        private Bin2DNodeGuillotine m_RightChild;

        private readonly Bin2DGuillotine m_Bin;

        private enum BorderType
        {
            None,
            Left,
            Top,
            Right = 4,
            Bottom = 8
        }
    }
}
