using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DCModToolsGUI
{
    public partial class AtlasInfoControl : UserControl
    {
        public class AtlasInfo : IAtlasInfo
        {
            public string Name { get; set; } = "";
            public ModifyMode Mode { get; set; } = ModifyMode.None;
            public Dictionary<string, List<Tile>> Tiles { get; set; } = null!;
            public AtlasInfoControl Bind { get; set; } = null!;
            public bool IsModified => Mode != ModifyMode.None;
            public WeakReference<List<AtlasTexListControl.AtlasTexInfo>> _texInfo = null!;
            public List<AtlasTexListControl.AtlasTexInfo> texInfo
            {
                get => (_texInfo?.TryGetTarget(out var r) ?? false) ? r : null!;
                set => (_texInfo ??= new(value)).SetTarget(value);
            }
            public enum ModifyMode
            {
                None, Replace, Diff
            }
        }
        public AtlasInfo atlas { get; set; } = null!;
        public AtlasInfoControl(AtlasInfo atlas) : this()
        {
            this.atlas = atlas;
            atlas.Bind = this;
            Refresh();
        }
        public AtlasInfoControl()
        {
            InitializeComponent();

            btnEdit.Click += BtnEdit_Click;
            btnRestore.Click += BtnRestore_Click;

            grid.PointerEnter += AtlasInfoControl_PointerEnter;
            grid.PointerLeave += AtlasInfoControl_PointerLeave;
        }

        private void AtlasInfoControl_PointerLeave(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            grid.Background = atlas.IsModified ? Brushes.Green : null;
        }

        private void AtlasInfoControl_PointerEnter(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            grid.Background = atlas.IsModified ? Brushes.DarkGreen : Brushes.DarkGray;
        }

        private void BtnRestore_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainWindow.mainWindow.openedPAK!.RemoveAtlasDir(atlas.Name);
            atlas.Mode = AtlasInfo.ModifyMode.None;
            if (atlas.texInfo != null)
            {
                foreach (var v in atlas.texInfo)
                {
                    v.Bind.Restore();
                }
            }
            Refresh();
        }

        private async void BtnEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dir = MainWindow.mainWindow.openedPAK!.GetAtlasDir(atlas.Name);
            
            if(atlas.texInfo == null)
            {
                atlas.texInfo = new();
                foreach((var texPath, var tiles) in atlas.Tiles)
                {
                    var path = Path.Combine(await Config.config.GetOriginalResPath(), "Atlas", texPath);
                    if (!File.Exists(path)) continue;
                    SysBitmap bitmap = (SysBitmap)SysBitmap.FromFile(path);
                    foreach(var tile in tiles)
                    {
                        var original = tile.CopyBitmapFromAtlas(bitmap);
                        using var stream = new MemoryStream();
                        original.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        original.Dispose();
                        stream.Position = 0;
                        var img = new Bitmap(stream);
                        var info = new AtlasTexListControl.AtlasTexInfo()
                        {
                            AtlasInfo = atlas,
                            original = img,
                            preview = img,
                            tile = tile
                        };
                        _ = new AtlasTexListControl(info);
                        atlas.texInfo.Add(info);
                    }
                }
                foreach(var v in dir.files)
                {
                    var tex = atlas.texInfo.FirstOrDefault(x => x.Name == v.name);
                    if(tex != null)
                    {
                        tex.IsModified = true;
                        using var stream = new MemoryStream(v.data); tex.preview = new Bitmap(stream);
                    }
                }
            }
            MainWindow.mainWindow.CurrentAtlasInfo = atlas.texInfo;
            MainWindow.mainWindow.RefreshAtlasList();
            if (!atlas.IsModified) atlas.Mode = AtlasInfo.ModifyMode.Diff;
            Refresh();
        }

        public void Refresh()
        {
            grid.Background = atlas.IsModified ? Brushes.Green : null;
            atlasName.Content = atlas.Name;
            atlasModified.Content = atlas.IsModified ? "Modified" : "Original";
            atlasTileCount.Content = "Tile Count: " + atlas.Tiles.Select(x => x.Value.Count).Sum();
            btnRestore.IsEnabled = atlas.IsModified;
        }

    }
}
