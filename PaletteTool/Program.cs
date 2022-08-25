
using System.Drawing;
using System.Drawing.Imaging;

List<string> infiles = new();
string outdir = "";
var palettepath = "";
bool nextIsOutDir = false;
foreach (var v in args)
{
    if (v.Equals("--outdir", StringComparison.OrdinalIgnoreCase))
    {
        nextIsOutDir = true;
    }
    else if (nextIsOutDir)
    {
        nextIsOutDir = false;
        outdir = v;
    }
    else if(v.EndsWith("_s.png"))
    {
        palettepath = v;
    }
    else
    {
        infiles.Add(v);
    }
}

if (infiles.Count < 1) return -1;

if(string.IsNullOrEmpty(outdir))
{
    outdir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(infiles[0]))!, "Out");
}

Directory.CreateDirectory(outdir);

if(string.IsNullOrEmpty(palettepath))
{
    #region Make palette
    palettepath = Path.Combine(outdir, Path.GetFileNameWithoutExtension(infiles[0]) + "_default_s.png");
    HashSet<Color> colors = new();
    List<(string, Bitmap)> images = new();
    foreach (var v in infiles)
    {
        var tex = (Bitmap)Image.FromFile(v);
        images.Add((v, tex));
        BitmapData data = tex.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        unsafe
        {
            for (int y = 0; y < tex.Height; y++)
            {
                int* rowbase = (int*)((byte*)data.Scan0 + data.Stride * y);
                for (int x = 0; x < tex.Width; x++)
                {
                    colors.Add(Color.FromArgb(rowbase[x]));
                }
            }
        }
        tex.UnlockBits(data);
    }
    Console.WriteLine("Number of colors: " + colors.Count);
    Dictionary<Color, Color> colormap = new();
    int rows = colors.Count / 255 + 1;
    Bitmap palette = new(256, rows, PixelFormat.Format32bppArgb);
    int cRow = 0;
    int cRed = 0;
    foreach(var e in images)
    {
        var v = e.Item2;
        for(int y = 0; y < v.Height; y++)
        {
            for(int x = 0; x < v.Width; x++)
            {
                var col = v.GetPixel(x, y);
                if (col.A == 0) continue;
                if(!colormap.TryGetValue(col, out var rcol))
                {
                    var green = (int)Math.Round(255.0D / rows * (0.5D + cRow));
                    rcol = Color.FromArgb(cRed, green, 0);
                    colormap.Add(col, rcol);
                    palette.SetPixel(cRed, cRow, col);
                    cRed++;
                    if(cRed >= 256)
                    {
                        cRed = 0;
                        cRow++;
                    }
                }
                v.SetPixel(x, y, rcol);
            }
        }
        v.Save(Path.Combine(outdir, Path.GetFileName(e.Item1)));
        v.Dispose();
    }
    palette.Save(palettepath);
    palette.Dispose();
    
    #endregion
}
else
{
    #region Colorize
    Dictionary<Color, Color> colormap = new();
    var pattle = (Bitmap)Image.FromFile(palettepath);
    var rows = pattle.Size.Height;
    Console.WriteLine("Reading palette");
    for (int j = 0; j < rows; j++)
    {
        int green = (int)Math.Round(255.0D / rows * (0.5D + j));
        for (int i = 0; i < 256; i++)
        {
            colormap.Add(Color.FromArgb(i, green, 0), pattle.GetPixel(i, j));
        }
    }
    Console.WriteLine("Number of colors: " + colormap.Count);
    pattle.Dispose();
    foreach(var v in infiles)
    {
        Console.WriteLine("Start processing \"" + v + "\"");
        var tex = (Bitmap)Image.FromFile(v);
        for(int y = 0; y < tex.Height; y++)
        {
            for(int x = 0; x < tex.Width; x++)
            {
                if(colormap.TryGetValue(tex.GetPixel(x, y), out Color color))
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
        tex.Save(Path.Combine(outdir, Path.GetFileName(v)));
        tex.Dispose();
    }
    #endregion
}
return 0;
