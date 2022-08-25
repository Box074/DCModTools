using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCSkinGUI
{
    public interface IFrameInfo
    {
        public string Name { get; }
        public Bitmap Preview { get; }
        public Bitmap? NormalPreview { get; }
        public string Background { get; }
        public void Refresh();
    }
    public class AdditionalFrameInfo : IFrameInfo
    {
        public string Name { get; set; } = "";
        private Bitmap _preview = null!;
        private Bitmap _normal_preview = null!;
        public Bitmap Preview => _preview ??= new(ModifiedBitmap);

        public Bitmap? NormalPreview => _normal_preview ??= (File.Exists(ModifiedNormal) ? new(ModifiedNormal) : null!);

        public string Background => "Green";
        public string ModifiedBitmap { get; set; } = "";
        public string ModifiedNormal => string.IsNullOrEmpty(ModifiedBitmap) ? null! : (Path.Combine(Path.GetDirectoryName(ModifiedBitmap)!, Path.GetFileNameWithoutExtension(ModifiedBitmap)) + "_n.png");
        public void Refresh()
        {
            
        }
    }
    public class FrameInfo : IFrameInfo
    {
        public Tile Tile { get; set; } = null!;
        private Bitmap _preview = null!;
        private Bitmap _normal_preview = null!;
        private SysBitmap _colorize_img = null!;
        public void Refresh()
        {
            _preview = null!;
            _normal_preview = null!;
        }
        public SysBitmap ColorizeImage
        {
            get
            {
                if (_colorize_img == null)
                {
                    _colorize_img = (SysBitmap)Tile.bitmap;
                }
                return _colorize_img;
            }
        }
        private SysBitmap _normal_img = null!;
        public SysBitmap NormalizeImage
        {
            get
            {
                if(_normal_img == null)
                {
                    var atlas = MainWindow.mainWindow.GetAtlasPngAsync(Path.GetFileNameWithoutExtension(Tile.texName) + "_n.png");
                    atlas.Wait();
                    _normal_img = Tile.CopyBitmapFromAtlas(atlas.Result);
                }
                return _normal_img;
            }
        }
        public string Name => Tile.name;
        public Bitmap Preview
        {
            get
            {
                if (_preview == null)
                {
                    if(!string.IsNullOrEmpty(ModifiedBitmap) && File.Exists(ModifiedBitmap))
                    {
                        _preview = new(ModifiedBitmap);
                    }
                    else
                    {
                        var ms = new MemoryStream();
                        var bitmap = ColorizeImage.Clone(new(Tile.offsetX, Tile.offsetY, Tile.width, Tile.height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                       
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        bitmap.Dispose();
                        ms.Position = 0;
                        _preview = new(ms);
                        ms.Close();
                    }
                }
                return _preview;
            }
        }
        public Bitmap NormalPreview
        {
            get
            {
                if (_normal_preview == null)
                {
                    if (!string.IsNullOrEmpty(ModifiedNormal) && File.Exists(ModifiedNormal))
                    {
                        _normal_preview = new(ModifiedNormal);
                    }
                    else
                    {
                        if(Tile == null)
                        {
                            return null!;
                        }
                        var ms = new MemoryStream();
                        var bitmap = NormalizeImage.Clone(new(Tile.offsetX, Tile.offsetY, Tile.width, Tile.height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        bitmap.Dispose();
                        ms.Position = 0;
                        _normal_preview = new(ms);
                        ms.Close();
                    }
                }
                return _normal_preview;
            }
        }
        public string Background => string.IsNullOrEmpty(ModifiedBitmap) ? null! : "Green";
        public string ModifiedBitmap { get; set; } = null!;
        public string ModifiedNormal => string.IsNullOrEmpty(ModifiedBitmap) ? null! : (Path.Combine(Path.GetDirectoryName(ModifiedBitmap)!, Path.GetFileNameWithoutExtension(ModifiedBitmap)) + "_n.png");

    }
}
