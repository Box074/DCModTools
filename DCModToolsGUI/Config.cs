

namespace DCModToolsGUI
{
    public class Config
    {
        public static readonly string ConfigPath = Path.Combine(((Environment.ProcessPath != null) ? Path.GetDirectoryName(Environment.ProcessPath) : null) ?? "", "Config.json");
        public static readonly Config config;
        static Config()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath)) ?? new();
                }
                catch (Exception)
                {
                    config = new();
                }
            }
            else
            {
                config = new();
            }

#pragma warning disable CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
#pragma warning restore CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public string originalExpanedResPath = "";
        public async Task<string> GetOriginalResPath(Window? parent = null)
        {
            CHECK:
            if(string.IsNullOrEmpty(originalExpanedResPath))
            {
                OpenFolderDialog openFolderDialog = new()
                {
                    Title = "Open Original Expanded Res Folder"
                };
                var str = await openFolderDialog.ShowAsync(parent ?? MainWindow.mainWindow);
                if (str == null)
                {
                    var result = await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error", "Original Res is required, do you want to quit the program?", MessageBox.Avalonia.Enums.ButtonEnum.YesNo, 
                        MessageBox.Avalonia.Enums.Icon.None, WindowStartupLocation.CenterScreen).ShowDialog(parent ?? MainWindow.mainWindow);
                    if(result == MessageBox.Avalonia.Enums.ButtonResult.Ok)
                    {
                        Environment.Exit(0);
                    }
                    goto CHECK;
                }
                originalExpanedResPath = str;
            }

            return originalExpanedResPath;
        }
    }
}
