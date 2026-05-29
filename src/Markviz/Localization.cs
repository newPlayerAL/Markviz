using System;
using System.Globalization;
using System.Threading;

namespace Markviz;

/// <summary>
/// All user-facing strings, indexed by current language.
/// Hand-coded (vs .resx) for compile-time safety and simplicity.
/// To add a language: extend Pick() to take a third arg, or refactor to a Dictionary.
/// </summary>
internal static class L
{
    public const string Zh = "zh";
    public const string En = "en";

    public static event Action? CultureChanged;

    private static string _current = En;

    /// <summary>Current language code: "zh" or "en".</summary>
    public static string Current
    {
        get => _current;
        set
        {
            var normalized = value == Zh ? Zh : En;
            if (_current == normalized) return;
            _current = normalized;

            var culture = new CultureInfo(normalized == Zh ? "zh-CN" : "en-US");
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            CultureChanged?.Invoke();
        }
    }

    /// <summary>Detect appropriate default from the OS UI culture.</summary>
    public static string DetectFromSystem()
    {
        var name = CultureInfo.CurrentUICulture.Name;
        return name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? Zh : En;
    }

    private static string Pick(string en, string zh) => _current == Zh ? zh : en;

    // ===== Menus =====
    public static string MenuFile           => Pick("_File", "文件(_F)");
    public static string MenuOpen           => Pick("_Open...", "打开(_O)...");
    public static string MenuExit           => Pick("E_xit", "退出(_X)");
    public static string MenuTools          => Pick("_Tools", "工具(_T)");
    public static string MenuRegisterAssoc  => Pick("Register .md / .markdown file association", "注册 .md / .markdown 文件关联");
    public static string MenuUnregisterAssoc=> Pick("Unregister file association", "取消文件关联");
    public static string MenuLanguage       => Pick("_Language", "语言(_L)");
    public static string MenuHelp           => Pick("_Help", "帮助(_H)");
    public static string MenuAbout          => Pick("_About", "关于(_A)");

    // ===== Dialog titles =====
    public static string DialogTitleError       => Pick("Error", "错误");
    public static string DialogTitleAssoc       => Pick("File Association", "文件关联");
    public static string DialogTitleAbout       => Pick("About", "关于");

    // ===== Dialog messages =====
    public static string MsgRegistered => Pick(
        "Registered. .md / .markdown files can now be opened with Markviz.\n\n" +
        "If the icon in Explorer hasn't updated yet, restart explorer.exe or sign out and back in.",
        "已注册。现在 .md / .markdown 文件可以用 Markviz 打开。\n\n" +
        "如果资源管理器中图标暂未更新，可重启 explorer.exe 或注销重登。");

    public static string MsgUnregistered    => Pick("File association cleared.", "已取消文件关联。");
    public static string MsgRegisterFail    => Pick("Registration failed: ", "注册失败：");
    public static string MsgUnregisterFail  => Pick("Unregistration failed: ", "取消失败：");
    public static string MsgOpenFail        => Pick("Failed to open file: ", "打开文件失败：");

    public static string MsgAbout => Pick(
        "Markviz\n\nA lightweight Windows Markdown viewer\nBuilt with WPF + WebView2 + Markdig",
        "Markviz\n\n一个轻量级的 Windows Markdown 浏览器\nBuilt with WPF + WebView2 + Markdig");

    // ===== File dialog filter =====
    public static string OpenFileFilter => Pick(
        "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*",
        "Markdown 文件 (*.md;*.markdown)|*.md;*.markdown|所有文件 (*.*)|*.*");

    // ===== File-association registry =====
    public static string ProgIdDescription => Pick("Markdown Document", "Markdown 文档");

    // ===== Errors =====
    public static string ErrExecutablePath => Pick(
        "Could not determine the current executable path.",
        "无法获取当前可执行文件路径。");

    // ===== Welcome page (Markdown source) =====
    public static string WelcomeMarkdown => Pick(WelcomeEn, WelcomeZh);

    private const string WelcomeEn = """
        # Markviz

        Welcome! Open a Markdown file with any of these:

        - **Drag and drop** a `.md` file into the window
        - **Keyboard shortcut**: `Ctrl + O`
        - **Menu**: File → Open
        - **Command line**: `Markviz.exe path\to\file.md`

        ## Supported syntax

        - GitHub Flavored Markdown (tables, task lists, strikethrough)
        - Code blocks, blockquotes, nested lists
        - Auto links, footnotes

        ```csharp
        // For example, a code block like this
        var html = Markdown.ToHtml(text);
        ```
        """;

    private const string WelcomeZh = """
        # Markviz

        欢迎使用！可以通过以下方式打开 Markdown 文件：

        - **拖拽**：把 `.md` 文件拖到窗口中
        - **快捷键**：`Ctrl + O`
        - **菜单**：文件 → 打开
        - **命令行**：`Markviz.exe path\to\file.md`

        ## 支持的语法

        - GitHub Flavored Markdown（表格、任务列表、删除线）
        - 代码块、引用、嵌套列表
        - 自动链接、脚注

        ```csharp
        // 比如这样的代码块
        var html = Markdown.ToHtml(text);
        ```
        """;
}
