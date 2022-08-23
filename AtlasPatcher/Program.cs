using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Common.Atlas;

var atlas = args[0];
var indir = args[1];

var patlas = Path.GetDirectoryName(Path.GetFullPath(atlas))!;

var binreader = new BinaryReader(File.OpenRead(atlas));
_ = binreader.ReadInt32();

string ReadString(BinaryReader _reader)
{
	int num = (int)_reader.ReadByte();
	if (num == 255)
	{
		num = (int)_reader.ReadUInt16();
	}
	if (num != 0)
	{
		return new string(_reader.ReadChars(num));
	}
	return "";
}

while (binreader.BaseStream.Position + 18L < binreader.BaseStream.Length)
{
	List<Tile> list = new();
	string text = ReadString(binreader);
	if (text == "")
	{
		break;
	}
	var atlasTexPath = Path.Combine(patlas, text);
	if (!File.Exists(atlasTexPath)) continue;
	File.Move(atlasTexPath, Path.ChangeExtension(atlasTexPath, "bak.png"), true);
    Bitmap atlasTex = (Bitmap)Image.FromFile(Path.ChangeExtension(atlasTexPath, "bak.png"));
	while (binreader.BaseStream.Position + 18L < binreader.BaseStream.Length)
	{
		Tile tile = new();
		tile.name = ReadString(binreader);
		if (tile.name == "")
		{
			break;
		}
		tile.index = (int)binreader.ReadUInt16();
		tile.x = (int)binreader.ReadUInt16() - 1;
		tile.y = (int)binreader.ReadUInt16() - 1;
		tile.width = (int)binreader.ReadUInt16() - 2;
		tile.height = (int)binreader.ReadUInt16() - 2;
		tile.offsetX = (int)binreader.ReadUInt16();
		tile.offsetY = (int)binreader.ReadUInt16();
		tile.originalWidth = (int)binreader.ReadUInt16();
		tile.originalHeight = (int)binreader.ReadUInt16();

		list.Add(tile);
	}

	foreach(var tile in list)
    {
		var path = Path.Combine(tile.name.Split('/'));
		if(tile.index != -1)
        {
			path = path + "-=-" + tile.index.ToString() + "-=-";
		}
		path += ".png";
		var texPath = Path.Combine(indir, path);
		if (!File.Exists(texPath))
        {
			Console.WriteLine($"Missing File: {texPath}");
			continue;
        }
		Bitmap tex = (Bitmap)Image.FromFile(texPath);
		var src = tex.LockBits(new(tile.offsetX, tile.offsetY, tile.width, tile.height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		var dest = atlasTex.LockBits(new(tile.x, tile.y, tile.width, tile.height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
		unsafe
        {
			for(int i = 0; i < tile.height; i++)
            {
				Unsafe.CopyBlockUnaligned((void*)((byte*)dest.Scan0 + i * dest.Stride), (void*)((byte*)src.Scan0 + i * src.Stride),(uint)(tile.width * 4));
            }
        }
		tex.UnlockBits(src);
		atlasTex.UnlockBits(dest);
		tex.Dispose();
    }
	atlasTex.Save(atlasTexPath);
	atlasTex.Dispose();
	File.Delete(Path.ChangeExtension(atlasTexPath, "bak.png"));
}
binreader.Close();
