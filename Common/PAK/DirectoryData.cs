namespace Common.PAK
{
    public class DirectoryData : EntryData
	{
		public override bool isDirectory
		{
			get
			{
				return true;
			}
		}

		public DirectoryData(DirectoryData _parent, string _name) : base(_parent, _name)
		{
		}
		public DirectoryData(string name) : base(null, name) { }

		public void AddEntry(EntryData _entry)
		{
			_entry.parent = this;
			if (_entry.isDirectory)
			{
				directories.Add((DirectoryData)_entry);
				return;
			}
			files.Add((FileData)_entry);
		}
		public FileData GetFile(string name)
        {
			var result = files.FirstOrDefault(f => f.name == name);
			if(result == null)
            {
				result = new(name, new byte[0]);
				AddEntry(result);
            }
			return result;
        }
		public void DeleteFile(string name)
        {
			files.RemoveAll(x => x.name == name);
        }
		public List<FileData> files { get; private set; } = new List<FileData>();

		public List<DirectoryData> directories { get; private set; } = new List<DirectoryData>();
	}
}
