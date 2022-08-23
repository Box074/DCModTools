using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DCModToolsGUI.Build
{
	public static class AtlasBuilder
	{
		public static void BuildDiffAtlas(Dictionary<string, List<Tile>> atlas, Dictionary<string, SysBitmap> outTex)
        {
			foreach(var v in atlas)
            {
				if (!v.Key.StartsWith("Diff_")) continue;
				if(v.Value.Count == 0) continue;
				SysBitmap bitmap = new(v.Value[0].atlasWidth, v.Value[0].atlasHeight, PixelFormat.Format32bppArgb);
				outTex[v.Key] = bitmap;
				foreach(var tile in v.Value)
                {
					tile.CopyBitmapToAtlas(bitmap);
                }
            }
        }
		
		public static void CreateDiffAtlas(Dictionary<string, List<Tile>> atlas, DirectoryData atlasDiff)
		{
			Bin2DPacker packer = new(new(32,32), new(4096,4099), Bin2DPacker.Algorithm.Guillotine);
			List<Tile> tiles = new();
			foreach(var v in atlasDiff.files)
            {
                using var stream = new MemoryStream(v.data);
                var bitmap = (SysBitmap)SysBitmap.FromStream(stream);
                var tile = TrimTile(bitmap);
                string[] vs = v.name.Trim().Split(' ');
                tile.name = vs[0];
                string s = vs.Last().Trim();
                tile.index = int.Parse(s);
                packer.InsertElement((uint)tiles.Count, new(tile.width, tile.height), out _);
                tiles.Add(tile);
            }
			int id = 0;
			foreach(var v in packer.bins)
            {
				string texName = "Diff_" + atlasDiff.name + (id++) + ".png";
				foreach(var el in v.elements)
                {
					var tile = tiles[(int)el.Key];
					tile.atlasWidth = v.size.Width;
					tile.atlasHeight = v.size.Height;
					tile.x = el.Value.X;
					tile.y = el.Value.Y;
					var match = atlas.Select(x => (x, x.Value.FirstOrDefault(x2 => x2.name == tile.name && x2.index == tile.index))).FirstOrDefault(x => x.Item2 is not null);
					if (match.Item2 != null)
					{
						match.x.Value.Remove(match.Item2!);
					}
					if(!atlas.TryGetValue(texName, out var tilesList))
                    {
						tilesList = new();
						atlas.Add(texName, tilesList);
                    }
					tilesList.Add(tile);
                }
            }
		}
		public static Tile TrimTile(SysBitmap bitmap)
		{
			Tile _tile = new();
			_tile.bitmap = bitmap;
			_tile.width = _tile.originalWidth = bitmap.Width;
			_tile.height = _tile.originalHeight = bitmap.Height;
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, _tile.width, _tile.height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			
			bool flag2 = false;
			int num = 0;
			while (num < _tile.originalHeight && !flag2 && _tile.height > 1)
			{
				int num2 = 0;
				while (num2 < _tile.originalWidth && !flag2 && _tile.height > 1)
				{
					flag2 = (((long)Marshal.ReadInt32(bitmapData.Scan0 + (num * _tile.originalWidth + num2) * 4) & (long)(-16777216)) != 0L);
					num2++;
				}
				if (!flag2)
				{
					_tile.offsetY++;
					_tile.height--;
				}
				num++;
			}
			flag2 = false;
			int num3 = 0;
			while (num3 < _tile.originalWidth && !flag2 && _tile.width > 1)
			{
				int num4 = _tile.offsetY;
				while (num4 < _tile.originalHeight && !flag2 && _tile.width > 1)
				{
					flag2 = (((long)Marshal.ReadInt32(bitmapData.Scan0 + (num4 * _tile.originalWidth + num3) * 4) & (long)(-16777216)) != 0L);
					num4++;
				}
				if (!flag2)
				{
					_tile.offsetX++;
					_tile.width--;
				}
				num3++;
			}
			flag2 = false;
			int num5 = _tile.originalHeight - 1;
			while (num5 >= _tile.offsetY && !flag2 && _tile.height > 1)
			{
				int num6 = _tile.offsetX;
				while (num6 < _tile.originalWidth && !flag2 && _tile.height > 1)
				{
					flag2 = (((long)Marshal.ReadInt32(bitmapData.Scan0 + (num5 * _tile.originalWidth + num6) * 4) & (long)(-16777216)) != 0L);
					num6++;
				}
				if (!flag2)
				{
					_tile.height--;
				}
				num5--;
			}
			flag2 = false;
			int num7 = _tile.originalWidth - 1;
			while (num7 >= _tile.offsetX && !flag2 && _tile.width > 1)
			{
				int num8 = _tile.offsetY;
				while (num8 < _tile.originalHeight && !flag2 && _tile.width > 1)
				{
					flag2 = (((long)Marshal.ReadInt32(bitmapData.Scan0 + (num8 * _tile.originalWidth + num7) * 4) & (long)(-16777216)) != 0L);
					num8++;
				}
				if (!flag2)
				{
					_tile.width--;
				}
				num7--;
			}
			bitmap.UnlockBits(bitmapData);
			return _tile;
		}
	}
}
