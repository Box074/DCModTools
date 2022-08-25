using System.Drawing;
using System.Drawing.Imaging;

namespace DCTCommon.Atlas;
public class Tile
{

    public string name;

    public int index;

    public int x;

    public int y;

    public int width;

    public int height;

    public int offsetX;

    public int offsetY;

    public int originalWidth;

    public int originalHeight;

    public string originalFilePath;

    public bool hasNormal;

    public Tile duplicateOf;
    public string texName;
    public int atlasIndex;
    public byte[] texData;

    public int atlasWidth;
    public int atlasHeight;
    public Bitmap bitmap;
    public unsafe void CopyBitmapToAtlas(Bitmap _atlas)
    {
        var _tile = this;
        if (_tile.bitmap == null)
        {
            return;
        }
        BitmapData bitmapData = _atlas.LockBits(new Rectangle(_tile.x, _tile.y, _tile.width, _tile.height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        BitmapData bitmapData2 = _tile.bitmap.LockBits(new Rectangle(_tile.offsetX, _tile.offsetY, _tile.width, _tile.height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        for (int i = 0; i < _tile.height; i++)
        {
            Unsafe.CopyBlockUnaligned((void*)(bitmapData.Scan0 + i * bitmapData.Stride), (void*)(bitmapData2.Scan0 + i * bitmapData2.Stride), (uint)(_tile.width * 4));
        }
        _atlas.UnlockBits(bitmapData);
        _tile.bitmap.UnlockBits(bitmapData2);
    }
    public unsafe Bitmap CopyBitmapFromAtlas(Bitmap _atlas)
    {
        var _tile = this;
        BitmapData bitmapData = _atlas.LockBits(new Rectangle(_tile.x, _tile.y, _tile.width, _tile.height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        Bitmap bitmap = new(_tile.originalWidth, _tile.originalHeight > _tile.height ? _tile.originalHeight : _tile.height);
        BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(_tile.offsetX, _tile.offsetY, _tile.width, _tile.height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        for (int j = 0; j < _tile.height; j++)
        {
            Unsafe.CopyBlockUnaligned((void*)(bitmapData2.Scan0 + j * bitmapData2.Stride), (void*)(bitmapData.Scan0 + j * bitmapData.Stride), (uint)(_tile.width * 4));
        }
        bitmap.UnlockBits(bitmapData2);
        _atlas.UnlockBits(bitmapData);
        return bitmap;
    }
}