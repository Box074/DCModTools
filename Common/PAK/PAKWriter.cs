namespace Common.PAK
{
    public class PAKWriter : PAKBase
    {
        public PAKWriter(PAKReader reader)
        {
            void CopyFileInfo(DirectoryData src, DirectoryData dest)
            {
                foreach (var v in src.files)
                {
                    dest.files.Add(new(dest == root ? null : dest, v.name, 0, 0, 0)
                    {
                        data = v.data[..v.data.Length]
                    });
                }
                foreach (var v in src.directories)
                {
                    var dir = new DirectoryData(dest == root ? null : dest, v.name);
                    dest.directories.Add(dir);
                    CopyFileInfo(v, dir);
                }
            }
            root.name = reader.root.name;
            CopyFileInfo(reader.root, root);
        }
        public PAKWriter() { }
        public void Write(BinaryWriter writer)
        {
            base.headerSize = 0;
            dataSize = 0;
            FixFileInfo(root);
            writer.Write(new char[]
    {
        'P',
        'A',
        'K'
    });
            writer.Write(version);
            var headerSize = writer.BaseStream.Position;
            writer.Write(base.headerSize); //Header Size
            writer.Write(dataSize);
            WritePAKEntry(writer, root);
            writer.Write(new char[]
            {
        'D',
        'A',
        'T',
        'A'
            });
            var pos = writer.BaseStream.Position;
            writer.BaseStream.Position = headerSize;
            writer.Write((int)pos);
            writer.BaseStream.Position = pos;
            WritePAKContent(writer, root);
        }
        private void WritePAKEntry(BinaryWriter _writer, EntryData _entry)
        {
            byte b = (byte)_entry.name.Length;
            _writer.Write(b);
            if (b > 0)
            {
                _writer.Write(_entry.name.ToCharArray());
            }
            if (_entry.isDirectory)
            {
                DirectoryData directoryData = (DirectoryData)_entry;
                _writer.Write((byte)1);
                _writer.Write(directoryData.directories.Count + directoryData.files.Count);
                foreach (DirectoryData entry in directoryData.directories)
                {
                    WritePAKEntry(_writer, entry);
                }
                using List<FileData>.Enumerator enumerator2 = directoryData.files.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    FileData entry2 = enumerator2.Current;
                    WritePAKEntry(_writer, entry2);
                }
                return;
            }
            FileData fileData = (FileData)_entry;
            _writer.Write((byte)0);
            _writer.Write(fileData.position);
            _writer.Write(fileData.size);
            _writer.Write(fileData.checksum);
        }
        private void FixFileInfo(DirectoryData _pakDirectory)
        {
            headerSize++;
            headerSize += ((_pakDirectory.parent == null) ? 0 : _pakDirectory.name.Length);
            headerSize++;
            headerSize += 4;
            foreach (DirectoryData directoryInfo in _pakDirectory.directories)
            {
                FixFileInfo(directoryInfo);
            }
            var adler32 = new Adler32();
            foreach (FileData fileInfo in _pakDirectory.files)
            {
                headerSize++;
                headerSize += fileInfo.name.Length;
                headerSize++;
                headerSize += 12;
                fileInfo.position = dataSize;
                fileInfo.size = fileInfo.data.Length;
                fileInfo.checksum = adler32.Make(fileInfo.data);
                dataSize += fileInfo.data.Length;
            }
        }
        private void WritePAKContent(BinaryWriter _writer, DirectoryData _dir)
        {
            foreach (DirectoryData dir in _dir.directories)
            {
                WritePAKContent(_writer, dir);
            }
            foreach (FileData fileInfo in _dir.files)
            {
                _writer.Write(fileInfo.data);
            }
        }
    }
}
