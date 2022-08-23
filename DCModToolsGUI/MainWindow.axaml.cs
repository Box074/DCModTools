using Avalonia.Controls;

namespace DCModToolsGUI
{
    public partial class MainWindow : Window
    {
        public ProjectInfo? openedPAK = null;
        public string? lastOpen = null;
        public static MainWindow mainWindow = null!;
        public MainWindow()
        {
            InitializeComponent();

            //InitWorkshop();
            mainWindow = this;
            newProject.Click += NewProject_Click;
            openPAK.Click += OpenPAK_Click;
            savePAK.Click += SavePAK_Click;

            exportDiffPAK.Click += ExportDiffPAK_Click;

            textAtlasSearch.KeyDown += TextAtlasSearch_KeyDown; ;
            checkAtlasOnlyModified.Checked += CheckAtlasOnlyModified_Checked;
            checkAtlasOnlyModified.Unchecked += CheckAtlasOnlyModified_Unchecked;

            atlasBack.Click += AtlasBack_Click;
        }

        private async void ExportDiffPAK_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filters = new()
                {
                    new()
                    {
                        Name = "PAK",
                        Extensions = new() { "pak" }
                    }
                },
                InitialFileName = "res.pak"
            };
            var file = await saveFileDialog.ShowAsync(this);
            if (string.IsNullOrEmpty(file)) return;
            PAKWriter writer = new();
            var atlasDir = new DirectoryData("atlas");
            writer.root.AddEntry(atlasDir);
            foreach(var atlas in openedPAK!.modifiedAtlas.directories)
            {
                var stream = File.OpenRead(Path.Combine(await Config.config.GetOriginalResPath(), "Atlas", atlas.name + ".atlas"));
                var orig = AtlasHelper.ReadAtlas(stream);
                stream.Close();
                Build.AtlasBuilder.CreateDiffAtlas(orig, atlas);
                var bitmaps = new Dictionary<string, SysBitmap>();
                Build.AtlasBuilder.BuildDiffAtlas(orig, bitmaps);
                var gorp = await Config.config.GetOriginalResPath(this);
                using(var ms = new MemoryStream())
                {
                    AtlasHelper.WriteAtlas(ms, orig, true, (name) =>
                    {
                        if (name.StartsWith("Diff_")) throw new Exception();
                        using var tex = (SysBitmap)SysBitmap.FromFile(Path.Combine(gorp, "atlas", name)); return (tex.Width, tex.Height);
                    });
                    atlasDir.AddEntry(new FileData(atlas.name + ".atlas", ms.ToArray()));
                }
                foreach(var v in orig)
                {
                    if(v.Value.Count == 0 || v.Key.StartsWith("Diff_")) continue;
                    atlasDir.AddEntry(new FileData(v.Key, File.ReadAllBytes(Path.Combine(gorp, "atlas", v.Key))));
                    var normalName = Path.GetFileNameWithoutExtension(v.Key) + "_n.png";
                    atlasDir.AddEntry(new FileData(normalName, File.ReadAllBytes(Path.Combine(gorp, "atlas", normalName))));
                }
                foreach(var v in bitmaps)
                {
                    MemoryStream ms = new();
                    var normal = new SysBitmap(v.Value.Width, v.Value.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    var emptyNormal = SysColor.FromArgb(127, 127, 255);
                    for(int y = 0; y < normal.Height; y++)
                    {
                        for(int x = 0; x < normal.Width; x++)
                        {
                            normal.SetPixel(x, y, emptyNormal);
                        }
                    }
                    normal.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    normal.Dispose();
                    atlasDir.AddEntry(new FileData(Path.GetFileNameWithoutExtension(v.Key) + "_n.png", ms.ToArray()));
                    ms.Close();
                    ms = new();
                    v.Value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    v.Value.Dispose();
                    atlasDir.AddEntry(new FileData(v.Key, ms.ToArray()));
                    ms.Close();
                }
            }
            using var s = File.OpenWrite(file);
            writer.Write(new(s));
        }

        private void AtlasBack_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            CurrentAtlasInfo = null!;
            RefreshAtlasList();
        }

        private void TextAtlasSearch_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            RefreshAtlasList();
        }

        private void CheckAtlasOnlyModified_Unchecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            RefreshAtlasList();
        }

        private void CheckAtlasOnlyModified_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            RefreshAtlasList();
        }

        private void NewProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            openedPAK = new();
            lastOpen = "";
            InitWorkshop();
        }

        private void SavePAK_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Save();
        }

        private async void OpenPAK_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filters = new()
                {
                    new()
                    {
                        Name = "Project File",
                        Extensions = new() { "dcm" }
                    }
                },
                AllowMultiple = false
            };
            var files = await openFileDialog.ShowAsync(this);
            if (files == null || files.Length == 0) return;
            var file = files[0];
            LoadPAK(file);
        }
        public async void Save()
        {
            if(openedPAK == null || lastOpen == null) return;
            openedPAK.ModInfo = (textModName.Text, textCategory.SelectedIndex);
            if(lastOpen == "")
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Project File",
                            Extensions = new() { "dcm" }
                        }
                    }
                };
                lastOpen = await saveFileDialog.ShowAsync(this);
                if (lastOpen == null) return;
            }
            using BinaryWriter writer = new(File.OpenWrite(lastOpen)); openedPAK.pak.Write(writer);
        }
        public void LoadPAK(string path)
        {
            using(BinaryReader reader = new(File.OpenRead(path))) openedPAK = new(new PAKReader(reader));
            lastOpen = path;
            tabMainControl.IsEnabled = true;
            InitWorkshop();
        }
        public void InitWorkshop()
        {
            if(openedPAK == null || lastOpen == null)
            {
                savePAK.IsEnabled = false;
                tabMainControl.IsEnabled = false;
                menuExports.IsEnabled = false;
                return;
            }
            savePAK.IsEnabled = true;
            tabMainControl.IsEnabled = true;
            menuExports.IsEnabled = true;

            var (ModName, ModType) = openedPAK.ModInfo;
            textModName.Text = ModName;
            textCategory.SelectedIndex = ModType;

            InitAtlasList();
        }
    }
}
