using System.Drawing;
using Common.Atlas;


bool outascii = false;
if(args[0].ToLower() == "-ascii")
{
	outascii = true;
	args = args.Skip(1).ToArray();
}
var inatlas = args[0];
var outatlas = args.Length == 1 ? Path.ChangeExtension(inatlas, ".out.atlas") : args[1];


using (Stream reader = File.OpenRead(inatlas))
{
    using Stream writer = File.OpenWrite(outatlas); AtlasHelper.WriteAtlas(writer, AtlasHelper.ReadAtlas(reader), outascii, name =>
    {
        using var tex = (Bitmap)Image.FromFile(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(inatlas!)!)!, name)); return (tex.Width, tex.Height);
    });
}
