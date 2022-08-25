using Avalonia.Controls;

namespace DCSkinGUI
{
    public partial class MainWindow : Window
    {
        public Dictionary<string, SysBitmap> atlasCache = new();
        public static MainWindow mainWindow = null!;
        public Dictionary<SysColor, SysColor> originalColorMap = null!;
        public Dictionary<string, ClipInfo> clips = null!;
        public List<ClipInfo> clipInfo = null!;
        public ClipInfo? currentClip = null;
        public List<FrameInfo> currentFrames = null!;
        public IFrameInfo? currentFrame = null;
        public bool isPlayingClip = false;
        public int fps = 24;
        public string projectPath = "";
        public MainWindow()
        {
            mainWindow = this;

            InitializeComponent();

            openProject.Click += OpenProject_Click;
            newProject.Click += NewProject_Click;
            exportSkinAtlas.Click += ExportSkinAtlas_Click;

            textSearchClipNames.TextChanged += RefreshClipNames;
            clipNames.SelectionChanged += ClipNames_SelectionChanged;

            checkOnlyModified.Checked += RefreshClipNames;
            checkOnlyModified.Unchecked += RefreshClipNames;
            checkOnlyOriginal.Checked += RefreshClipNames;
            checkOnlyOriginal.Unchecked += RefreshClipNames;

            animFramePanel.SelectionChanged += AnimFramePanel_SelectionChanged;

            btnStopClip.Click += BtnStopClip_Click;
            btnPlayClip.Click += BtnPlayClip_Click;

            textFPS.KeyDown += TextFPS_TextInput;

            btnOpenExporer.Click += BtnOpenExporer_Click;
            btnAppendFrame.Click += BtnAppendFrame_Click;

            new Thread(() =>
            {
                while(true)
                {
                    if(isPlayingClip)
                    {
                        var id = animFramePanel.SelectedIndex + 1;
                        if(id >= animFramePanel.ItemCount)
                        {
                            id = 0;
                        }
                        Dispatcher.UIThread.Post(() =>
                        {
                            animFramePanel.SelectedIndex = id;
                        });
                        Thread.Sleep(1000 / fps);
                        continue;
                    }
                    Thread.Sleep(0);
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        private async void ExportSkinAtlas_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            var outpath = await openFolderDialog.ShowAsync(this);
            if (string.IsNullOrEmpty(outpath)) return;
            Exporter.Export(projectPath, outpath);
        }

        private async void BtnAppendFrame_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (currentClip == null) return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filters = new()
            {
                new()
                {
                    Name = "Image",
                    Extensions = new() { "png" }
                }
            };
            openFileDialog.AllowMultiple = true;
            var files = await openFileDialog.ShowAsync(this);
            if(files == null || files.Length == 0) return;
            int id = 0;
            foreach (var v in files)
            {
                var path = GetModifiedFramePath(currentClip.Name, animFramePanel.ItemCount + id, forceReturn: true);
                File.Copy(v, path, true);
                currentClip.AppendedFrames.Add(new()
                {
                    Name = "Additional Frame " + (animFramePanel.ItemCount + id),
                    ModifiedBitmap = path
                });
                id++;
            }
            LoadClip();
        }

        private void BtnOpenExporer_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (currentFrame == null || currentClip == null) return;
            var framePath = GetModifiedFramePath(currentClip.Name, animFramePanel.SelectedIndex, currentFrame as FrameInfo, true);
            RefreshFrameList();
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{framePath}\"");
        }

        private void TextFPS_TextInput(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if(int.TryParse(textFPS.Text, out var result))
            {
                fps = result;
            }
            else
            {
                textFPS.Text = fps.ToString();
            }
        }

        private void BtnPlayClip_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            isPlayingClip = !isPlayingClip;
        }

        private void BtnStopClip_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            isPlayingClip = false;
            animFramePanel.SelectedIndex = 0;
        }

        private void AnimFramePanel_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (animFramePanel.SelectedItem is not IFrameInfo frame) return;
            skinPreview.Source = frame.Preview;
            normalPreview.Source = frame.NormalPreview;
            currentFrame = frame;
        }

        private void RefreshClipNames(object? sender, EventArgs e)
        {
            RefreshClipNames();
        }

        public void RefreshClipNames()
        {
            if (this.clips == null || clipInfo == null) return;
            IEnumerable<ClipInfo> clips = clipInfo;
            if(!string.IsNullOrWhiteSpace(textSearchClipNames.Text))
            {
                clips = clips.Where(x => x.Name.Contains(textSearchClipNames.Text, StringComparison.OrdinalIgnoreCase));
            }
            if(checkOnlyModified.IsChecked ?? false)
            {
                clips = clips.Where(x => x.ModifiedLevel >= 1);
            }
            if(checkOnlyOriginal.IsChecked ?? false)
            {
                clips = clips.Where(x => x.ModifiedLevel <= 1);
            }
            clipNames.Items = clips.ToArray();
        }

        private void ClipNames_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (clips == null || clipInfo == null) return;
            isPlayingClip = false;
            if (clipNames.SelectedItem is not ClipInfo clip) return;
            currentClip = clip;
            LoadClip();
        }


        private void NewProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void OpenProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenFolder();
        }
        public async void OpenFolder()
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.Title = "Open Project Folder";
            var dir = await openFolderDialog.ShowAsync(this);
            if(string.IsNullOrEmpty(dir)) return;
            projectPath = dir;
            InitWorkshop();
        }
        public async Task<System.Drawing.Bitmap> GetAtlasPngAsync(string texName)
        {
            var mhskin = await Config.config.GetOriginalResPath(this);
            if (!atlasCache.TryGetValue(texName, out var atlas))
            {
                if (originalColorMap == null)
                {
                    var colormappng = (SysBitmap)SysBitmap.FromFile(Path.Combine(mhskin, "beheadedModHelper_default_s.png"));
                    originalColorMap = ReadColorMap(colormappng);
                    colormappng.Dispose();
                }
                atlas = (SysBitmap)SysBitmap.FromFile(Path.Combine(mhskin, texName));
                if(!texName.EndsWith("_n.png"))
                {
                    Colorize(atlas, originalColorMap);
                }
                atlasCache[texName] = atlas;
            }
            return atlas;
        }
        public async void LoadClip()
        {
            if (currentClip == null) return;
            var mhskin = await Config.config.GetOriginalResPath(this);
            if(!(currentClip.Frames?.TryGetTarget(out var frames) ?? false))
            {
                frames = new();
                int id = 0;
                currentClip.Frames = new(frames);
                foreach(var f in currentClip.Tiles)
                {
                    if(f.bitmap == null)
                    {
                        f.bitmap = f.CopyBitmapFromAtlas(await GetAtlasPngAsync(f.texName));
                    }
                    var frame = new FrameInfo()
                    {
                        Tile = f,
                        ModifiedBitmap = ""
                    };
                    frame.ModifiedBitmap = GetModifiedFramePath(currentClip.Name, id);
                    id++;
                    frames.Add(frame);
                }
            }
            animFramePanel.Items = ((IEnumerable<IFrameInfo>)frames).Concat(currentClip.AppendedFrames).Select(x => { x.Refresh(); return x; });
            currentFrames = frames;
        }
        public string GetModifiedFramePath(string clipName, int frame, FrameInfo? info = null, bool forceReturn = false)
        {
            var path = Path.Combine(projectPath, clipName, frame.ToString() + ".png");
            if(!File.Exists(path))
            {
                if(info == null)
                {
                    if(forceReturn) Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    return forceReturn ? path : "";
                }
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                info.ModifiedBitmap = path;
                var stream = File.OpenWrite(path);
                info.ColorizeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();
                stream = File.OpenWrite(info.ModifiedNormal);
                info.NormalizeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();
            }
            return path;
        }
        public void Colorize(SysBitmap img, Dictionary<SysColor, SysColor> colorMap)
        {
            ProgressWindow.SetProgress("Colorize the atlas", 0);
            var all = img.Height * img.Width;
            var i = 0;
            for(int y = 0; y < img.Height; y++)
            {
                for(int x = 0; x < img.Width; x++)
                {
                    if(colorMap.TryGetValue(img.GetPixel(x, y), out SysColor color))
                    {
                        img.SetPixel(x, y, color);
                        i++;
                    }
                    ProgressWindow.SetProgress($"Colorize the atlas", 100 - ((all - i) / (double)all * 100));
                }
            }
            ProgressWindow.SetProgress("Colorize the atlas", 100);
        }
        public static Dictionary<SysColor, SysColor> ReadColorMap(SysBitmap pattle)
        {
            var map = new Dictionary<SysColor, SysColor>();
            var rows = pattle.Size.Height;
            for (int j = 0; j < rows; j++)
            {
                int green = (int)Math.Round(255.0D / rows * (0.5D + j));
                for (int i = 0; i < 256; i++)
                {
                    map[SysColor.FromArgb(i, green, 0)] = pattle.GetPixel(i, j);
                }
            }
            return map;
        }
        public void RefreshFrameList()
        {
            if (currentClip == null) return;
            var lastSelect = animFramePanel.SelectedIndex;
            animFramePanel.Items = null;
            animFramePanel.Items = ((IEnumerable<IFrameInfo>)currentFrames).Concat(currentClip.AppendedFrames).Select(x => { x.Refresh(); return x; });
            animFramePanel.SelectedIndex = lastSelect;
        }
        public async void InitWorkshop()
        {
            
            var respath = await Config.config.GetOriginalResPath();
            _ = Task.Run(async () =>
            {
                ProgressWindow.SetProgress("Open ModHelperSkin.atlas", 0);
                FileStream fs = File.OpenRead(Path.Combine(respath, "beheadedModHelper.atlas"));
                var origSkin = AtlasHelper.ReadAtlas(fs);
                fs.Close();
                ProgressWindow.SetProgress("Build ModHelperSkin Color Map", 3);
                
                var tileCount = origSkin.Select(x => x.Value.Count).Sum();
                int processed = 0;
                clips = new();
                clipInfo = new();
                foreach (var v in origSkin.SelectMany(x => x.Value))
                {
                    ProgressWindow.SetProgress("Build clip info: " + v.name, processed / (double)tileCount * 50);
                    var clipName = v.name[..v.name.LastIndexOf('_')];
                    var frame = int.Parse(v.name[(v.name.LastIndexOf('_') + 1)..], System.Globalization.NumberStyles.Integer);
                    if (!clips.TryGetValue(clipName, out var clip))
                    {
                        List<Tile> cliplist = new();
                        clip = new(){
                            Name = clipName,
                            Tiles = cliplist,
                            ModifiedLevel = 2
                        };
                        clips.Add(clipName, clip);
                    }
                    var framePath = GetModifiedFramePath(clipName, frame);

                    if (string.IsNullOrEmpty(framePath) || !File.Exists(framePath))
                    {
                        clip.ModifiedLevel = 1;
                    }
                    else
                    {
                        v.bitmap = v.CopyBitmapFromAtlas(await GetAtlasPngAsync(v.texName));
                        var ms = new MemoryStream();
                        v.bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        var crc = new Adler32().Make(ms);
                        ms.Close();
                        var stream = File.OpenRead(framePath);
                        if (new Adler32().Make(stream) != crc)
                        {
                            clip.ModifiedCount++;
                        }
                        stream.Close();
                    }
                    if (clip.Tiles.Count <= frame)
                    {
                        clip.Tiles.AddRange(Enumerable.Repeat((Tile)null!, frame - clip.Tiles.Count + 1));
                    }
                    clip.Tiles[frame] = v;
                    
                    //Thread.Sleep(1);
                    processed++;
                }
                clipInfo = clips.Select(x =>
                {
                    var extFrame = "";
                    while(!string.IsNullOrEmpty((extFrame = GetModifiedFramePath(x.Key, x.Value.Tiles.Count + x.Value.AppendedFrames.Count))))
                    {
                        x.Value.AppendedFrames.Add(new()
                        {
                            Name = "Additional Frame " + (x.Value.Tiles.Count + x.Value.AppendedFrames.Count),
                            ModifiedBitmap = extFrame
                        });
                        x.Value.ModifiedCount++;
                        if (x.Value.ModifiedLevel == 2) continue;
                        x.Value.ModifiedLevel = 1;
                    }
                    x.Value.ModifiedLevel = x.Value.ModifiedLevel == 1 ? (x.Value.ModifiedCount == 0 ? 0 : 1) : x.Value.ModifiedLevel;
                    return x.Value;
                    }).ToList();
                Dispatcher.UIThread.Post(() =>
                {
                    gridMaskbottom.IsEnabled = true;
                    gridMaskTop.IsEnabled = true;
                    clipNames.Items = clipInfo;
                    //GC.Collect(2);
                    ProgressWindow.SetProgress("", 120);
                });
            });
        }
    }
}
