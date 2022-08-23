

namespace Common.Atlas
{
    public static class AtlasHelper
    {
		public static string ReadString(BinaryReader _reader)
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
		public static void WriteString(BinaryWriter _writer, string _stringToWrite)
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

		public static Dictionary<string, List<Tile>> ReadAtlas(Stream stream)
        {
			var binreader = new BinaryReader(stream, Encoding.UTF8, true);
			bool isBin = binreader.ReadByte() == 'B' && binreader.ReadByte() == 'A' && binreader.ReadByte() == 'T' && binreader.ReadByte() == 'L';
			List<Tile> list = null;
			Dictionary<string, List<Tile>> tiles = new();
			if (!isBin)
			{

				binreader.Close();
				stream.Seek(0, SeekOrigin.Begin);
				string[] lines;
				using (StreamReader reader = new(stream))
					lines = reader.ReadToEnd().Split('\n');
				string name;
				(int, int) texSize = (0,0);
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
						i++;
						var sizet = lines[i].Split(':')[1].Trim().Split(',');
						texSize.Item1 = int.Parse(sizet[0].Trim());
						texSize.Item2 = int.Parse(sizet[1].Trim());
						i += 3;
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

                    var tile = new Tile
                    {
                        name = tileName,
                        x = x - 1,
                        y = y - 1,
                        width = width + 2,
                        height = height + 2,
                        originalWidth = origWidth + 2,
                        originalHeight = origHeight + 2,
                        offsetX = offsetx,
                        offsetY = origHeight - height - offsety,
                        index = index,
                        atlasWidth = texSize.Item1,
                        atlasHeight = texSize.Item2
                    };
                    list.Add(tile);
				}
			}
			else
			{
				while (binreader.BaseStream.Position + 18L < binreader.BaseStream.Length)
				{
					var prevPosition = stream.Position;
					
					if (!(binreader.ReadByte() == 'B' && binreader.ReadByte() == 'A' && binreader.ReadByte() == 'T' && binreader.ReadByte() == 'L')) stream.Position = prevPosition;

					string text = ReadString(binreader);
					if (!tiles.TryGetValue(text, out list))
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
			return tiles;
        }

		public static void WriteAtlas(Stream stream, Dictionary<string, List<Tile>> atlas, bool ascii = false, Func<string, (int, int)> sizeGetter = null)
		{
			if (!ascii)
			{
                using var binwriter = new BinaryWriter(stream, Encoding.UTF8, true);
                binwriter.Write("BATL".ToCharArray());
                foreach ((string texName, List<Tile> tlist) in atlas)
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
			else
			{
                using var text = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
                text.NewLine = "\n";
                foreach ((string texName, List<Tile> tlist) in atlas)
                {
                    (int, int) size = (0, 0);
                    if (sizeGetter is not null)
                    {
                        try
                        {
                            size = sizeGetter(texName);
                        }
                        catch (Exception)
                        {
                            size = (tlist[0].atlasWidth, tlist[1].atlasHeight);
                        }
                    }
                    else
                    {
                        size = (tlist[0].atlasWidth, tlist[1].atlasHeight);
                    }
                    text.WriteLine();
                    text.WriteLine(texName);
                    text.WriteLine("size: {0},{1}", size.Item1, size.Item2);
                    text.WriteLine("format: RGBA8888");
                    text.WriteLine("filter: Linear,Linear");
                    text.WriteLine("repeat: none");
                    foreach (var tile in tlist)
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
