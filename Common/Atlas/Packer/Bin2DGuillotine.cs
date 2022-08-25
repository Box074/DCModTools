using System;
using System.Collections.Generic;
using System.Drawing;

namespace DCTCommon.Atlas.Packer
{
    public class Bin2DGuillotine : Bin2D
    {
        public Bin2DGuillotine(Size _startSize, Size _margin, MarginType _marginType) : base(_startSize, _margin, _marginType)
        {
            Reset();
        }

        protected override bool InsertElement(uint _id, Size _elementSize, out Rectangle _area)
        {
            Bin2DNodeGuillotine bin2DNodeGuillotine = m_Root.Insert(_id, _elementSize, margin, marginType);
            if (bin2DNodeGuillotine == null)
            {
                _area = default;
                return false;
            }
            _area = bin2DNodeGuillotine.GetAreaWithoutMargin(bin2DNodeGuillotine.area.Size, marginType);
            return true;
        }

        protected override void RetrieveSizes(ref List<Size> _sizeList)
        {
            m_Root.RetrieveSizes(ref _sizeList);
        }

        protected override void RetrieveIDs(ref List<uint> _idList)
        {
            m_Root.RetrieveIDs(ref _idList);
        }

        protected override void Reset()
        {
            m_Root = new Bin2DNodeGuillotine(this)
            {
                area = new Rectangle(0, 0, size.Width, size.Height)
            };
        }

        private Bin2DNodeGuillotine m_Root;
    }
}
