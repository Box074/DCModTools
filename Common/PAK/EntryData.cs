namespace DCTCommon.PAK
{
    public abstract class EntryData
    {
        public abstract bool isDirectory { get; }

        public string name { get; set; }

        public string fullName
        {
            get
            {
                DirectoryData parent = this.parent;
                if (parent != null)
                {
                    return Path.Combine(parent.fullName, name);
                }
                return name;
            }
        }

        public DirectoryData parent { get; set; }

        public EntryData(DirectoryData _parent, string _name)
        {
            name = _name;
            parent = _parent;
        }
    }
}
