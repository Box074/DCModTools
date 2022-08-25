using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DCSkinGUI
{
    public partial class ProgressWindow : Window
    {
        public static string lastMsg = null!;
        public static double lastProgress;
        private static bool startup = false;
        [ThreadStatic]
        private static ProgressWindow _instance = null!;
        public ProgressWindow()
        {
            InitializeComponent();
        }
        public static void SetProgress(string msg, double progress)
        {
            if (msg.Equals(lastMsg) && lastProgress == progress) return;
            lastMsg = msg;
            lastProgress = progress;
            if (!startup)
            {
                startup = true;
                Dispatcher.UIThread.Post(() =>
                {
                    startup = false;
                    if(progress >= 99)
                    {
                        _instance?.Close();
                        _instance = null!;
                        return;
                    }
                    else
                    {
                        if(_instance == null)
                        {
                            _instance = new();
                            _instance.Topmost = true;
                            if(MainWindow.mainWindow != null)
                            {
                                _ = _instance.ShowDialog(MainWindow.mainWindow);
                            }
                            else
                            {
                                _instance.Show();
                            }
                        }
                    }
                    _instance.progressBar.Value = lastProgress;
                    _instance.processingName.Content = lastMsg;
                });
            }
        }
    }
}
