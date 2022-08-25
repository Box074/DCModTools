namespace DCTCommon.PAK
{
    public class PAKBase
    {
        public byte version { get; set; }
        public DirectoryData root { get; set; } = new(null, "");
        public int headerSize { get; set; }
        public int dataSize { get; set; }
    }
}
