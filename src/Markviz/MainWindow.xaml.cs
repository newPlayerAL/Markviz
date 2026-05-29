using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Markdig;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace Markviz;

public partial class MainWindow : Window
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    private string? _currentFile;
    private string? _userDataFolder;
    private bool _showingWelcome;
    private bool _webViewReady;

    public MainWindow()
    {
        InitializeComponent();
        ApplyLanguage();
        UpdateAssocMenuState();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Per-process temp folder so WebView2 doesn't pollute the exe dir or LocalAppData.
        // Cleaned up in OnClosed; OS sweeps stragglers if we crash.
        _userDataFolder = Path.Combine(
            Path.GetTempPath(),
            "Markviz_" + Guid.NewGuid().ToString("N"));

        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: _userDataFolder);
        await WebView.EnsureCoreWebView2Async(env);
        _webViewReady = true;

        // When a file is dropped on WebView2, its default behavior is to open
        // the file (typically in a new popup window). Intercept both paths and
        // load the file ourselves instead.
        WebView.CoreWebView2.NewWindowRequested += OnFileNavigationIntercept;
        WebView.CoreWebView2.NavigationStarting += OnFileNavigationIntercept;

        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && File.Exists(args[1]))
        {
            LoadFile(args[1]);
        }
        else
        {
            ShowWelcome();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        WebView?.Dispose();
        try
        {
            if (_userDataFolder != null && Directory.Exists(_userDataFolder))
                Directory.Delete(_userDataFolder, recursive: true);
        }
        catch
        {
            // Best-effort: WebView2 may still hold file handles briefly after dispose.
        }
    }

    private void ApplyLanguage()
    {
        FileMenu.Header        = L.MenuFile;
        OpenMenuItem.Header    = L.MenuOpen;
        ExitMenuItem.Header    = L.MenuExit;
        ToolsMenu.Header       = L.MenuTools;
        RegisterMenuItem.Header   = L.MenuRegisterAssoc;
        UnregisterMenuItem.Header = L.MenuUnregisterAssoc;
        LanguageMenu.Header    = L.MenuLanguage;
        HelpMenu.Header        = L.MenuHelp;
        AboutMenuItem.Header   = L.MenuAbout;

        LangZhMenuItem.IsChecked = L.Current == L.Zh;
        LangEnMenuItem.IsChecked = L.Current == L.En;
    }

    private void ShowWelcome()
    {
        _showingWelcome = true;
        _currentFile = null;
        Title = "Markviz";
        var body = Markdown.ToHtml(L.WelcomeMarkdown, Pipeline);
        WebView.NavigateToString(WrapHtml(body));
    }

    private void LoadFile(string path)
    {
        try
        {
            var text = File.ReadAllText(path);
            var body = Markdown.ToHtml(text, Pipeline);
            WebView.NavigateToString(WrapHtml(body));
            _currentFile = path;
            _showingWelcome = false;
            Title = $"{Path.GetFileName(path)} - Markviz";
        }
        catch (Exception ex)
        {
            MessageBox.Show(L.MsgOpenFail + ex.Message, L.DialogTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string WrapHtml(string body) => $$"""
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8">
        <style>
            :root {
                color-scheme: light dark;
            }
            body {
                font-family: -apple-system, "Segoe UI", "Microsoft YaHei", sans-serif;
                max-width: 860px;
                margin: 2em auto;
                padding: 0 2em;
                line-height: 1.65;
                color: #24292f;
                background: #ffffff;
            }
            h1, h2 { border-bottom: 1px solid #eaecef; padding-bottom: .3em; }
            h1 { font-size: 2em; }
            h2 { font-size: 1.5em; }
            code {
                background: #f6f8fa;
                padding: .2em .4em;
                border-radius: 3px;
                font-family: Consolas, "Courier New", monospace;
                font-size: 90%;
            }
            pre {
                background: #f6f8fa;
                padding: 1em;
                border-radius: 6px;
                overflow-x: auto;
            }
            pre code { background: none; padding: 0; font-size: 100%; }
            blockquote {
                border-left: 4px solid #dfe2e5;
                color: #6a737d;
                padding: 0 1em;
                margin: 0;
            }
            table { border-collapse: collapse; margin: 1em 0; }
            th, td { border: 1px solid #dfe2e5; padding: 6px 13px; }
            th { background: #f6f8fa; }
            img { max-width: 100%; }
            a { color: #0969da; }
            hr { border: 0; border-top: 1px solid #eaecef; }
            @media (prefers-color-scheme: dark) {
                body { color: #c9d1d9; background: #0d1117; }
                h1, h2, hr { border-color: #30363d; }
                code, pre { background: #161b22; }
                blockquote { border-color: #30363d; color: #8b949e; }
                th, td { border-color: #30363d; }
                th { background: #161b22; }
                a { color: #58a6ff; }
            }
        </style>
        </head>
        <body>{{body}}</body>
        </html>
        """;

    private void OnFileNavigationIntercept(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        if (TryGetLocalFilePath(e.Uri, out var path))
        {
            e.Handled = true;
            Dispatcher.BeginInvoke(() => LoadFile(path));
        }
    }

    private void OnFileNavigationIntercept(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (TryGetLocalFilePath(e.Uri, out var path))
        {
            e.Cancel = true;
            Dispatcher.BeginInvoke(() => LoadFile(path));
        }
    }

    private static bool TryGetLocalFilePath(string uri, out string path)
    {
        path = string.Empty;
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var u) || !u.IsFile) return false;
        path = u.LocalPath;
        return File.Exists(path);
    }

    private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = L.OpenFileFilter };
        if (dlg.ShowDialog() == true)
        {
            LoadFile(dlg.FileName);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void RegisterAssoc_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            FileAssociation.Register();
            MessageBox.Show(L.MsgRegistered, L.DialogTitleAssoc, MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateAssocMenuState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(L.MsgRegisterFail + ex.Message, L.DialogTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UnregisterAssoc_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            FileAssociation.Unregister();
            MessageBox.Show(L.MsgUnregistered, L.DialogTitleAssoc, MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateAssocMenuState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(L.MsgUnregisterFail + ex.Message, L.DialogTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateAssocMenuState()
    {
        var registered = FileAssociation.IsRegistered();
        RegisterMenuItem.IsEnabled = !registered;
        UnregisterMenuItem.IsEnabled = registered;
    }

    private void SetLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem mi || mi.Tag is not string lang) return;
        if (lang == L.Current)
        {
            // Re-check the only enabled item if the user clicked the active one
            ApplyLanguage();
            return;
        }

        var settings = Settings.Load();
        settings.Language = lang;
        settings.Save();
        L.Current = lang;

        ApplyLanguage();
        if (_showingWelcome && _webViewReady) ShowWelcome();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(L.MsgAbout, L.DialogTitleAbout, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            LoadFile(files[0]);
        }
    }
}
