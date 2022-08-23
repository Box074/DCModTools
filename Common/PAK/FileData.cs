namespace Common.PAK
{
    public class FileData : EntryData
	{
		public int position { get; set; }

		public int size { get; set; }

		public int checksum { get; set; }
		public byte[] data { get; set; }
		public override bool isDirectory
		{
			get
			{
				return false;
			}
		}

		public FileData(DirectoryData _parent, string _name, int _position, int _size, int _crc) : base(_parent, _name)
		{
			position = _position;
			size = _size;
			checksum = _crc;
		}
		public FileData(string name, byte[] data) : base(null, name)
        {
			this.data = data;
        }
	}
}
