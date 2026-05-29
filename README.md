# Markviz

一个轻量级的 Windows Markdown 浏览器。基于 WPF + WebView2 + Markdig。

## 特性

- 拖拽 `.md` 文件即可预览
- 支持 GitHub 风格 Markdown（表格、任务列表、删除线、代码块等）
- 自适应浅色 / 深色主题
- 启动快、内存占用低
- 命令行参数支持，可注册文件关联

## 环境要求

- Windows 10 1809+ / Windows 11
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)（Win11 预装，Win10 大多已预装）
- 运行 framework-dependent 版本需要 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

## 开发

```powershell
cd app
dotnet restore
dotnet build
dotnet run --project src/Markviz
```

## 发布

**Framework-dependent（小，需用户装 .NET 8 Runtime）：**

```powershell
dotnet publish src/Markviz -c Release -r win-x64 --self-contained false -o publish
```

**Self-contained 单文件（~30MB，无需安装 .NET）：**

```powershell
dotnet publish src/Markviz -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

## License

MIT
