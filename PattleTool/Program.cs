using System.Drawing;
using System.Drawing.Imaging;

Dictionary<Color, Color> GetColorMap(string pattlepath)
{
    Dictionary<Color, Color> result = new();
    using (var pattle = (Bitmap)Image.FromFile(pattlepath))
    {
        for (int j = 0; j < pattle.Height; j++)
        {
            int green = (int)Math.Round(255.0D / pattle.Height * (0.5D + j));
            for (int i = 0; i < 256; i++)
            {
                result.Add(Color.FromArgb(i, green, 0), pattle.GetPixel(i, j));
            }
        }
    }
    return result;
}

var option = args[0].ToLower();
if (option == "-decode")
{
    var pattle = GetColorMap(args[1]);
    foreach (var v in args.Skip(2))
    {
        File.Copy(v, Path.ChangeExtension(v, "bak.png"), true);
        using (Bitmap bitmap = (Bitmap)Image.FromFile(Path.ChangeExtension(v, "bak.png")))
        {
            for(int y = 0; y < bitmap.Height; y++)
            {
                for(int x = 0; x < bitmap.Width; x++)
                {
                    var col = bitmap.GetPixel(x, y);
                    if(pattle.TryGetValue(Color.FromArgb(col.R, col.B, 0), out var ocol))
                    {
                        bitmap.SetPixel(x, y, ocol);
                    }
                }
            }
            bitmap.Save(v);
        }
        File.Delete(Path.ChangeExtension(v, "bak.png"));
    }
}
else if(option == "-encode")
{
    Dictionary<Color, Color> pattle = new();
    var outpattle = args[1];
    int red = 0;
    int green = 1;
    foreach (var v in args.Skip(2))
    {
        File.Copy(v, Path.ChangeExtension(v, "bak.png"), true);
        using (Bitmap bitmap = (Bitmap)Image.FromFile(Path.ChangeExtension(v, "bak.png")))
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var col = bitmap.GetPixel(x, y);
                    if (!pattle.TryGetValue(col, out var ocol))
                    {
                        if(red == 256)
                        {
                            red = 0;
                            green++;
                        }
                        ocol = Color.FromArgb(red++, green, 0);
                        pattle.Add(col, ocol);
                    }
                    bitmap.SetPixel(x, y, ocol);
                }
            }
            bitmap.Save(v);
        }
        File.Delete(Path.ChangeExtension(v, "bak.png"));
    }
    foreach(var v in pattle)
    {
        var col = v.Key;

    }
}
