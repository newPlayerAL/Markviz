using System;
using System.Windows;

namespace Markviz;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        InitializeLanguage();

        if (e.Args.Length > 0)
        {
            switch (e.Args[0])
            {
                case "--register":
                    try { FileAssociation.Register(); }
                    catch (Exception ex) { ReportCli(ex.Message); }
                    Shutdown();
                    return;
                case "--unregister":
                    try { FileAssociation.Unregister(); }
                    catch (Exception ex) { ReportCli(ex.Message); }
                    Shutdown();
                    return;
            }
        }

        new MainWindow().Show();
    }

    private static void InitializeLanguage()
    {
        var settings = Settings.Load();
        if (string.IsNullOrEmpty(settings.Language))
        {
            // First run: pick based on system UI culture, then persist so subsequent runs
            // don't depend on the OS setting.
            settings.Language = L.DetectFromSystem();
            settings.Save();
        }
        L.Current = settings.Language;
    }

    private static void ReportCli(string message)
    {
        MessageBox.Show(message, "Markviz", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
