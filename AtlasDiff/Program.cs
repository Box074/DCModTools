using DCTCommon.Atlas;

var atlas1 = AtlasHelper.ReadAtlas(File.OpenRead(args[0])).SelectMany(x => x.Value).ToArray();
var atlas2 = AtlasHelper.ReadAtlas(File.OpenRead(args[1])).SelectMany(x => x.Value).ToArray();

foreach (var v in atlas1)
{
    if(atlas2.All(x => x.index != v.index || x.name != v.name))
    {
        Console.WriteLine("Atlas2 Missing: " + v.name + "  " + v.index);
    }
}

foreach (var v in atlas2)
{
    if (atlas1.All(x => x.index != v.index || x.name != v.name))
    {
        Console.WriteLine("Atlas1 Missing: " + v.name + "  " + v.index);
    }
}

