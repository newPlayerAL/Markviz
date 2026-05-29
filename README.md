# Markviz

> English | [简体中文](README.zh-CN.md)

A lightweight Markdown viewer for Windows.

## Features

- Drag and drop a `.md` file to preview it instantly
- GitHub-flavored Markdown (tables, task lists, strikethrough, fenced code blocks, …)
- Automatic light / dark theme
- Fast startup, low memory footprint
- Command-line argument support and `.md` file association
- Bilingual UI (English / Chinese) with first-run auto-detect

## Requirements

- Windows 10 1809+ / Windows 11
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (preinstalled on Windows 11, and on most Windows 10 machines)
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (required to run the framework-dependent release)

## Development

```powershell
cd app
dotnet restore
dotnet build
dotnet run --project src/Markviz
```

## Publishing

**Framework-dependent (small, requires the .NET 8 Desktop Runtime):**

```powershell
dotnet publish src/Markviz -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

**Self-contained single file (~30 MB, no runtime install needed):**

```powershell
dotnet publish src/Markviz -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

## License

[MIT](LICENSE)
