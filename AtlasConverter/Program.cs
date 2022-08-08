using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

bool outascii = false;
if(args[0].ToLower() == "-ascii")
{
	outascii = true;
	args = args.Skip(1).ToArray();
}
var inatlas = args[0];
var outatlas = args.Length == 1 ? Path.ChangeExtension(inatlas, ".out.atlas") : args[1];

string ReadString(BinaryReader _reader)
{
	int num = (int)_reader.ReadByte();
	if (num == byte.MaxValue)
	{
		num = (int)_reader.ReadUInt16();
	}
	if (num != 0)
	{
		return new string(_reader.ReadChars(num));
	}
	return "";
}
void WriteString(BinaryWriter _writer, string _stringToWrite)
{
	if (_stringToWrite.Length >= byte.MaxValue)
	{
		_writer.Write(byte.MaxValue);
		_writer.Write((ushort)_stringToWrite.Length);
	}
	else
	{
		_writer.Write((byte)_stringToWrite.Length);
	}
	_writer.Write(_stringToWrite.ToCharArray());
}


using (var writer = File.OpenWrite(outatlas))
{
    using (var stream = File.OpenRead(inatlas))
    {
        var binreader = new BinaryReader(stream);
        var magic = binreader.ReadInt32();
		List<Tile>? list = null;
		Dictionary<string, List<Tile>> tiles = new();
		if (magic != 0x4241544C)
		{
			
			binreader.Close();
			var lines = File.ReadAllLines(inatlas);
			string? name = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "")
                {
                    list = null;
                    continue;
                }
                if (list == null)
                {
                    name = lines[i];
                    if (!tiles.TryGetValue(name, out list))
                    {
                        list = new();
                        tiles.Add(name, list);
                    }
                    i += 4;
                    continue;
                }
				var tileName = lines[i++].Split(':')[0].Trim();
				i++;
                var xy = lines[i++].Split(':')[1].Trim().Split(',');
                var size = lines[i++].Split(':')[1].Trim().Split(',');
                var orig = lines[i++].Split(':')[1].Trim().Split(',');
                var offset = lines[i++].Split(':')[1].Trim().Split(',');

                var index = int.Parse(lines[i].Split(':')[1].Trim());

				var x = int.Parse(xy[0].Trim());
				var y = int.Parse(xy[1].Trim());

				var width = int.Parse(size[0].Trim());
				var height = int.Parse(size[1].Trim());

				var origWidth = int.Parse(orig[0].Trim());
				var origHeight = int.Parse(orig[1].Trim());

				var offsetx = int.Parse(offset[0].Trim());
				var offsety = int.Parse(offset[1].Trim());

				var tile = new Tile();
				tile.name = tileName;
				tile.x = x - 1;
				tile.y = y - 1;
				tile.width = width + 2;
				tile.height = height + 2;
				tile.originalWidth = origWidth + 2;
				tile.originalHeight = origHeight + 2;
				tile.offsetX = offsetx;
				tile.offsetY = origHeight - height - offsety;
				tile.index = index;
				list.Add(tile);
            }
        }
        else
		{
			while (binreader.BaseStream.Position + 18L < binreader.BaseStream.Length)
			{
				var prevPosition = stream.Position;
				magic = binreader.ReadInt32();
				if(magic != 0x4241544C) stream.Position = prevPosition;

				string text = ReadString(binreader);
				if(!tiles.TryGetValue(text, out list))
                {
					list = new();
					tiles.Add(text, list);
                }
				if (text == "")
				{
					break;
				}
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
					tile.width = (int)binreader.ReadUInt16();
					tile.height = (int)binreader.ReadUInt16();
					tile.offsetX = (int)binreader.ReadUInt16();
					tile.offsetY = (int)binreader.ReadUInt16();
					tile.originalWidth = (int)binreader.ReadUInt16();
					tile.originalHeight = (int)binreader.ReadUInt16();

					list.Add(tile);
				}
			}
		}
		if (!outascii)
		{
			using (var binwriter = new BinaryWriter(writer))
			{
				binwriter.Write("BATL".ToCharArray());
				foreach ((string texName, List<Tile> tlist) in tiles)
				{
					//binwriter.Write("BATL".ToCharArray());
					WriteString(binwriter, texName);
					foreach (var tile in tlist)
					{
						WriteString(binwriter, tile.name);
						binwriter.Write((ushort)tile.index);
						binwriter.Write((ushort)tile.x);
						binwriter.Write((ushort)tile.y);
						binwriter.Write((ushort)tile.width);
						binwriter.Write((ushort)tile.height);
						binwriter.Write((ushort)tile.offsetX);
						binwriter.Write((ushort)tile.offsetY);
						binwriter.Write((ushort)tile.originalWidth);
						binwriter.Write((ushort)tile.originalHeight);
					}
					binwriter.Write((byte)0);
				}
			}
		}
		else
        {
			using (var text = new StreamWriter(writer))
			{
				foreach ((string texName, List<Tile> tlist) in tiles)
				{
					var tex = (Bitmap)Image.FromFile(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(inatlas!)!)!, texName));
					text.WriteLine();
					text.WriteLine(texName);
					text.WriteLine("size: {0},{1}", tex.Width, tex.Height);
					text.WriteLine("format: RGBA8888");
					text.WriteLine("filter: Linear,Linear");
					text.WriteLine("repeat: none");
					foreach(var tile in tlist)
                    {
						text.WriteLine(tile.name);
						text.WriteLine("  rotate: false");
						text.WriteLine("  xy: {0}, {1}", tile.x + 1, tile.y + 1);
						text.WriteLine("  size: {0}, {1}", tile.width - 2, tile.height - 2);
						text.WriteLine("  orig: {0}, {1}", tile.originalWidth - 2, tile.originalHeight - 2);
						text.WriteLine("  offset: {0}, {1}", tile.offsetX, tile.originalHeight - 2 - (tile.height - 2 + tile.offsetY));
						text.WriteLine("  index: {0}", tile.index);
					}
				}
			}
        }
    }
}