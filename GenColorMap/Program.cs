using System.Drawing;
using System.Drawing.Imaging;

var pattlepath = args[0];
var pattle = (Bitmap)Image.FromFile(pattlepath);
var rows = pattle.Size.Height;
var colorMap = new Bitmap(256, 256, PixelFormat.Format32bppArgb);

for (int j = 0; j < rows; j++)
{
    int green = (int)Math.Round(255.0D / rows * (0.5D + j));
    for (int i = 0; i < 256; i++)
    {
        colorMap.SetPixel(i, green, pattle.GetPixel(i, j));
    }
}
colorMap.Save(Path.ChangeExtension(pattlepath, "colormap.png"));
colorMap.Dispose();
pattle.Dispose();