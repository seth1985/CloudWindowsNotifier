using System.Text;
using System.Windows;
using System.IO;

namespace WindowsNotifier.OfflineAuthoring.App;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            DispatcherUnhandledException += (_, args) =>
            {
                WriteStartupError(args.Exception);
                args.Handled = true;
                System.Windows.MessageBox.Show(
                    $"An unexpected UI error occurred. Details written to:{Environment.NewLine}{GetStartupErrorPath()}",
                    "Offline Authoring - Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    WriteStartupError(ex);
                }
            };

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            WriteStartupError(ex);
            System.Windows.MessageBox.Show(
                $"Application startup failed. Details written to:{Environment.NewLine}{GetStartupErrorPath()}",
                "Offline Authoring - Startup Failure",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static void WriteStartupError(Exception ex)
    {
        try
        {
            var path = GetStartupErrorPath();
            var parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            var payload =
$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().FullName}{Environment.NewLine}" +
$"{ex.Message}{Environment.NewLine}" +
$"{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(path, payload, Encoding.UTF8);
        }
        catch
        {
            // Logging must not crash startup flow.
        }
    }

    private static string GetStartupErrorPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Windows Notifier",
            "OfflineAuthoring",
            "startup-error.log");
    }
}
