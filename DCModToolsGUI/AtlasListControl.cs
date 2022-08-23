

namespace DCModToolsGUI
{
    partial class MainWindow : Window
    {
        public List<AtlasInfoControl.AtlasInfo> atlas = new();
        public List<AtlasTexListControl.AtlasTexInfo> CurrentAtlasInfo = null!;
        public async void InitAtlasList()
        {
            if (openedPAK is null) return;
            var respath = Path.Combine(await Config.config.GetOriginalResPath(), "Atlas");
            atlas.Clear();
            foreach (var v in Directory.EnumerateFiles(respath, "*.atlas"))
            {
                if (Path.GetFileName(v).StartsWith("beheaded")) continue;
                var info = new AtlasInfoControl.AtlasInfo
                {
                    Name = Path.GetFileNameWithoutExtension(v),
                    Mode = AtlasInfoControl.AtlasInfo.ModifyMode.None
                };
                using (var stream = File.OpenRead(Path.Combine(respath, v))) info.Tiles = AtlasHelper.ReadAtlas(stream);
                _ = new AtlasInfoControl(info);
                atlas.Add(info);
            }
            foreach(var v in openedPAK.modifiedAtlas.directories)
            {
                var a = atlas.FirstOrDefault(x => x.Name == v.name);
                if (a != null)
                {
                    a.Mode = AtlasInfoControl.AtlasInfo.ModifyMode.Diff;
                }
            }
            RefreshAtlasList();
        }
        public void RefreshAtlasList()
        {
            GC.Collect(1);
            atlasViewer.IsVisible = CurrentAtlasInfo == null;
            atlasTexViewer.IsVisible = CurrentAtlasInfo != null;
            atlasBack.IsVisible = atlasTexViewer.IsVisible;
            if (atlasTexViewer.IsVisible)
            {
                RefreshAtlasTexList();
                return;
            }
            
            stackPanelAtlas.Children.Clear();
            foreach (var v in atlas)
            {
                if (checkAtlasOnlyModified.IsChecked ?? false)
                {
                    if (!v.IsModified) continue;
                }
                if (!string.IsNullOrWhiteSpace(textAtlasSearch.Text))
                {
                    if (!v.Name.Contains(textAtlasSearch.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                v.Bind.Refresh();
                stackPanelAtlas.Children.Add(v.Bind);
            }
        }
        public void RefreshAtlasTexList()
        {
            if (CurrentAtlasInfo == null) return;
            stackPanelAtlasTex.Children.Clear();
            foreach (var v in CurrentAtlasInfo)
            {
                if (checkAtlasOnlyModified.IsChecked ?? false)
                {
                    if (!v.IsModified) continue;
                }
                if (!string.IsNullOrWhiteSpace(textAtlasSearch.Text))
                {
                    if (!v.Name.Contains(textAtlasSearch.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                v.Bind.Refresh();
                stackPanelAtlasTex.Children.Add(v.Bind);
            }
        }
    }
}
