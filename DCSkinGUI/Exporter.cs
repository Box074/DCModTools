using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCSkinGUI
{
    public static class Exporter
    {
        public static Dictionary<SysColor, SysColor> colormap = new();
        private static Dictionary<string, List<Tile>> atlas = null!;
        public static async Task<(Tile, SysBitmap, SysBitmap?)?> TryGetFrame(string clipName, int frame, string mkhpath, string projectPath)
        {

            if (atlas == null)
            {
                var fs = File.OpenRead(Path.Combine(mkhpath, "beheadedModHelper.atlas"));
                atlas = AtlasHelper.ReadAtlas(fs);
                fs.Close();
            }
            var modified = Path.Combine(projectPath, clipName, frame + ".png");
            var modifiedNormal = Path.Combine(projectPath, clipName, frame + "_n.png");
            if(File.Exists(modified))
            {
                var tex = (SysBitmap)SysBitmap.FromFile(modified);
                var tile = DCModToolsGUI.Build.AtlasBuilder.TrimTile(tex);
                if(!File.Exists(modifiedNormal))
                {
                    return (tile, tex, null);
                }
                else
                {
                    return (tile, tex, (SysBitmap)SysBitmap.FromFile(modifiedNormal));
                }
            }
            var frameName = string.Format("{0}_{1:D2}", clipName, frame);
            var origframe = atlas.SelectMany(x => x.Value).FirstOrDefault(x => x.name == frameName);
            if(origframe == null)
            {
                return null;
            }
            var atlasTex = await Dispatcher.UIThread.InvokeAsync(async () => await MainWindow.mainWindow.GetAtlasPngAsync(origframe.texName));
            var atlasTex_n = await Dispatcher.UIThread.InvokeAsync(async () => await MainWindow.mainWindow.GetAtlasPngAsync(Path.GetFileNameWithoutExtension(origframe.texName) + "_n.png"));
            lock (atlasTex)
            {
                lock (atlasTex_n)
                {
                    return (origframe, origframe.CopyBitmapFromAtlas(atlasTex), origframe.CopyBitmapFromAtlas(atlasTex_n));
                }
            }
        }
        public static async Task<List<(Tile, SysBitmap, SysBitmap)>> ScanClipFrames(string clipName, string projectPath)
        {
            var result = new List<(Tile, SysBitmap, SysBitmap)>();
            var mkhpath = await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    return await Config.config.GetOriginalResPath();
                });
            int frame = 0;
            ProgressWindow.SetProgress("Collect clip: " + clipName, 3);
            (Tile tile, SysBitmap tex, SysBitmap? normal)? bitmaps;
            while ((bitmaps = await TryGetFrame(clipName, frame, mkhpath, projectPath)) != null)
            {
                var normal = bitmaps.Value.normal;
                if(normal == null)
                {
                    normal = new SysBitmap(bitmaps.Value.tex.Width, bitmaps.Value.tex.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    for(int y = 0; y < bitmaps.Value.tex.Height; y++)
                    {
                        for(int x = 0; x < bitmaps.Value.tex.Width; x++)
                        {
                            normal.SetPixel(x, y, SysColor.FromArgb(127, 127, 255));
                        }
                    }
                }
                ApplyPalette(bitmaps.Value.tex);
                result.Add((bitmaps.Value.tile, bitmaps.Value.tex, normal));
                frame++;
            }
            return result;
        }
        public static void ApplyPalette(SysBitmap bitmap)
        {
            for(int y = 0; y < bitmap.Height; y++)
            {
                for(int x =0; x < bitmap.Width; x++)
                {
                    var col = bitmap.GetPixel(x, y);
                    if (col.A == 0) continue;
                    if(!colormap.TryGetValue(col, out var rcol))
                    {
                        rcol = SysColor.FromArgb(colormap.Count % 255, (int)Math.Round(255.0D / 256 * (0.5D + (colormap.Count / 255))), 0);
                        colormap.Add(col, rcol);
                    }
                    bitmap.SetPixel(x, y, rcol);
                }
            }
        }
        public static SysBitmap MakePalette()
        {
            var palette = new SysBitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var rows = 256;
            for (int j = 0; j < rows; j++)
            {
                int green = (int)Math.Round(255.0D / rows * (0.5D + j));
                for (int i = 0; i < 256; i++)
                {
                    var pcc = SysColor.FromArgb(i, green, 0);
                    var pc = colormap.FirstOrDefault(x => x.Value == pcc);
                    palette.SetPixel(i, j, pc.Key);
                }
            }
            return palette;
        }
        public static async void Export(string projectPath, string outPath)
        {
            var mkhpath = await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                return await Config.config.GetOriginalResPath();
            });
            var fs = File.OpenRead(Path.Combine(mkhpath, "beheadedModHelper.atlas"));
            atlas = AtlasHelper.ReadAtlas(fs);
            fs.Close();
            List<string> clipNames = new();
            foreach(var v in atlas.SelectMany(x => x.Value))
            {
                var clipName = v.name[..v.name.LastIndexOf('_')];
                if (!clipNames.Contains(clipName)) clipNames.Add(clipName);
            }
            clipNames.AsParallel().ForAll(async v =>
            {
                var frames = await ScanClipFrames(v, projectPath);
                int frame = 0;
                foreach(var f in frames)
                {
                    f.Item2.Save(Path.Combine(outPath, string.Format("{0}_{1:D2}-=-65535-=-.png", v , frame)));
                    f.Item3.Save(Path.Combine(outPath, string.Format("{0}_{1:D2}-=-65535-=-_n.png", v, frame)));
                    f.Item2.Dispose();
                    f.Item3.Dispose();
                    frame++;
                }
            });
            var p = MakePalette();
            p.Save(Path.Combine(outPath, "0_default_s.png"));
            p.Dispose();
        }
    }
}
