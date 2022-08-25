using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DCTCommon.Atlas;
using DCTCommon.PAK;

namespace DCModToolsGUI
{
    public partial class AtlasTexListControl : UserControl
    {
        public class AtlasTexInfo : IAtlasInfo
        {
            public Tile tile = null!;
            public Bitmap preview = null!;
            public Bitmap? original;
            public bool IsModified { get; set; } = false;
            public AtlasInfoControl.AtlasInfo AtlasInfo { get; set; } = null!;
            public AtlasTexListControl Bind { get; set; } = null!;
            public string Name => tile.name + "  " + tile.index;
        }
        public DirectoryData atlasData = null!;
        public AtlasTexListControl()
        {
            InitializeComponent();
            grid.PointerEnter += AtlasInfoControl_PointerEnter;
            grid.PointerLeave += AtlasInfoControl_PointerLeave;

            btnExport.Click += BtnExport_Click;
            btnEdit.Click += BtnEdit_Click;
        }

        private async void BtnEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filters = new()
                {
                    new()
                    {
                        Name = "Image",
                        Extensions = new()
                        {
                            "png"
                        }
                    }
                },
                AllowMultiple = false
            };
            var files = await ofd.ShowAsync(MainWindow.mainWindow);
            if (files == null || files.Length == 0) return;
            var file = files[0];
            texInfo.preview = new Bitmap(file);
            texInfo.IsModified = true;
            Refresh();
            atlasData.GetFile(texInfo.Name).data = File.ReadAllBytes(file);
        }

        private async void BtnExport_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filters = new()
                {
                    new()
                    {
                        Name = "Image",
                        Extensions = new()
                        {
                            "png"
                        }
                    }
                },
                InitialFileName = texInfo.Name
            };
            var save = await saveFileDialog.ShowAsync(MainWindow.mainWindow);
            if(!string.IsNullOrEmpty(save))
            {
                (texInfo.preview ?? texInfo.original)?.Save(save);
            }
        }
        public void Restore()
        {
            atlasData.DeleteFile(texInfo.Name);
            texInfo.IsModified = false;
            texInfo.preview = texInfo.original!;

            Refresh();
        }
        public AtlasTexInfo texInfo = null!;
        public AtlasTexListControl(AtlasTexInfo atlasTex): this()
        {
            texInfo = atlasTex;
            texInfo.Bind = this;
            atlasData = MainWindow.mainWindow.openedPAK!.GetAtlasDir(texInfo.AtlasInfo.Name);
        }
        private void AtlasInfoControl_PointerLeave(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            grid.Background = texInfo.IsModified ? Brushes.Green : null;
        }

        private void AtlasInfoControl_PointerEnter(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            grid.Background = texInfo.IsModified ? Brushes.DarkGreen : Brushes.DarkGray;
        }
        public void Refresh()
        {
            grid.Background = texInfo.IsModified ? Brushes.Green : null;
            preview.Source = texInfo.preview;
            atlasName.Content = texInfo.tile.name;
            atlasModified.Content = texInfo.IsModified ? "Modified" : "Original";
            atlasTileCount.Content = $"Width: {texInfo.preview.Size.Width} Height: {texInfo.preview.Size.Height}";
            btnRestore.IsEnabled = texInfo.IsModified;
        }
    }
}
