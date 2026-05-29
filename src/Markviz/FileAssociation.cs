using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Markviz;

/// <summary>
/// Registers / unregisters per-user file associations for .md / .markdown.
/// Writes to HKCU\Software\Classes so no admin rights are required.
/// </summary>
internal static class FileAssociation
{
    private const string ProgId = "Markviz.md";
    private static readonly string[] Extensions = [".md", ".markdown"];
    // ProgIds from previous versions of the app — cleaned up on every register/unregister
    // so leftover entries from an old install don't shadow the current one.
    private static readonly string[] LegacyProgIds = ["MarkdownViewer.md"];
    private const string ClassesRoot = @"Software\Classes\";

    public static void Register()
    {
        RemoveLegacy();

        var exe = Environment.ProcessPath
            ?? throw new InvalidOperationException(L.ErrExecutablePath);

        using (var progIdKey = Registry.CurrentUser.CreateSubKey(ClassesRoot + ProgId))
        {
            progIdKey.SetValue("", L.ProgIdDescription);
            progIdKey.SetValue("FriendlyTypeName", L.ProgIdDescription);

            using var iconKey = progIdKey.CreateSubKey("DefaultIcon");
            iconKey.SetValue("", $"\"{exe}\",0");

            using var commandKey = progIdKey.CreateSubKey(@"shell\open\command");
            commandKey.SetValue("", $"\"{exe}\" \"%1\"");
        }

        foreach (var ext in Extensions)
        {
            using var extKey = Registry.CurrentUser.CreateSubKey(ClassesRoot + ext);
            extKey.SetValue("", ProgId);

            // Add to OpenWithProgids so the app shows up in Windows' "Open with" list
            // even if the default stays whatever the user already set.
            using var openWithKey = extKey.CreateSubKey("OpenWithProgids");
            openWithKey.SetValue(ProgId, Array.Empty<byte>(), RegistryValueKind.None);
        }

        NotifyShell();
    }

    public static void Unregister()
    {
        RemoveProgId(ProgId);
        RemoveLegacy();
        NotifyShell();
    }

    public static bool IsRegistered()
    {
        using var key = Registry.CurrentUser.OpenSubKey(ClassesRoot + ProgId);
        return key != null;
    }

    private static void RemoveLegacy()
    {
        foreach (var legacy in LegacyProgIds)
        {
            RemoveProgId(legacy);
        }
    }

    private static void RemoveProgId(string progId)
    {
        Registry.CurrentUser.DeleteSubKeyTree(ClassesRoot + progId, throwOnMissingSubKey: false);

        foreach (var ext in Extensions)
        {
            using var extKey = Registry.CurrentUser.OpenSubKey(ClassesRoot + ext, writable: true);
            if (extKey == null) continue;

            using (var openWithKey = extKey.OpenSubKey("OpenWithProgids", writable: true))
            {
                openWithKey?.DeleteValue(progId, throwOnMissingValue: false);
            }

            if (extKey.GetValue("") as string == progId)
            {
                extKey.DeleteValue("", throwOnMissingValue: false);
            }
        }
    }

    private static void NotifyShell()
    {
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
    }

    private const int SHCNE_ASSOCCHANGED = 0x08000000;
    private const int SHCNF_IDLIST = 0x0000;

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
}
