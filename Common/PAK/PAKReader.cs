

namespace Common.PAK
{
    public class PAKReader : PAKBase
    {
		public FileData Read(string fullname)
        {
			return filesData.TryGetValue(fullname, out var data) ? data : null;
        }
		private readonly Dictionary<string, EntryData> headerData = new();
		private readonly Dictionary<string, FileData> filesData = new();
		public PAKReader(BinaryReader reader)
        {
			ReadPAKHeader(reader);
        }
		private BinaryReader ReadPAKHeader(BinaryReader binaryReader)
		{
			binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
			_ = new string(binaryReader.ReadChars(3));
			version = binaryReader.ReadByte();
			headerSize = binaryReader.ReadInt32();
			dataSize = binaryReader.ReadInt32();
			root = (DirectoryData)ReadPAKEntry(binaryReader, null);
			return binaryReader;
		}
		private EntryData ReadPAKEntry(BinaryReader _reader, DirectoryData _parent)
		{
			string name = new(_reader.ReadChars((int)_reader.ReadByte()));
			EntryData result;
			if ((_reader.ReadByte() & 1) != 0)
			{
				DirectoryData directoryData = new(_parent, name);
				int num = _reader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					EntryData entryData = ReadPAKEntry(_reader, directoryData);
					directoryData.AddEntry(entryData);
					headerData.Add(entryData.fullName, entryData);
				}
				result = directoryData;
			}
			else
			{
				result = new FileData(_parent, name, headerSize + _reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32());
				int lastPos = (int)_reader.BaseStream.Position;
				_reader.BaseStream.Position = ((FileData)result).position;
				((FileData)result).data = _reader.ReadBytes(((FileData)result).size);
				_reader.BaseStream.Position = lastPos;
				filesData.TryAdd(result.fullName, (FileData)result);
			}
			return result;
		}
	}
}
