# WPF + WebView2 + ASP.NET Core + React 混合框架快速创建指南

## 概述

本指南将帮助您快速创建一个混合桌面应用程序，该应用程序结合了：
- **WPF** 作为桌面应用程序框架
- **WebView2** 用于嵌入现代 Web 界面
- **ASP.NET Core** 作为后端 API 服务器
- **React** 作为前端 UI 框架

这种架构允许您在桌面应用程序中享受现代 Web 技术的优势，同时保持原生桌面应用的性能。

## 项目结构

```
HybridApp/
├── AppWPF/                    # WPF 桌面应用程序
│   ├── AppWPF.csproj
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── WebServer.cs
├── ServerAspnet/              # ASP.NET Core API 服务器
│   ├── ServerAspnet.csproj
│   ├── Program.cs
│   └── Controllers/
├── ClientReact/               # React 前端应用
│   ├── package.json
│   ├── pnpm-lock.yaml
│   ├── vite.config.js
│   └── src/
└── HybridApp.sln             # Visual Studio 解决方案文件
```

## 快速创建步骤

### 1. 创建解决方案和项目

#### 1.1 创建解决方案
```bash
dotnet new sln -n HybridApp
```

#### 1.2 创建 WPF 项目
```bash
dotnet new wpf -n AppWPF
dotnet sln add AppWPF/AppWPF.csproj
```

#### 1.3 创建 ASP.NET Core 项目
```bash
dotnet new webapi -n ServerAspnet
dotnet sln add ServerAspnet/ServerAspnet.csproj
```

#### 1.4 创建 React 项目
```bash
mkdir -p ClientReact
cd ClientReact
pnpm create vite@latest . -- --template react-ts
pnpm install
```

> **注意**: 本指南使用 pnpm 作为包管理器。如果您没有安装 pnpm，请先运行 `npm install -g pnpm` 进行安装。

### 2. 配置 WPF 项目

#### 2.1 更新 AppWPF.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>  

  <ItemGroup>
    <PackageReference Include="Microsoft.Web.WebView2" Version="1.0.3065.39" />
  </ItemGroup>  

  <ItemGroup>
    <ProjectReference Include="..\ServerAspnet\ServerAspnet.csproj" />
  </ItemGroup>
</Project>
```

#### 2.2 更新 MainWindow.xaml
```xml
<Window x:Class="AppWPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:wv2="clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf"
        Title="Hybrid App" Height="600" Width="800">
    <Grid>
        <wv2:WebView2 Name="webView" />
    </Grid>
</Window>
```

#### 2.3 更新 MainWindow.xaml.cs
```csharp
using System.Windows;

namespace AppWPF;

public partial class MainWindow : Window
{
    private readonly WebServer _server = new WebServer();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closed += (sender, e) => _server.Stop();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Starting web server...");
        var port = _server.Start();
        await webView.EnsureCoreWebView2Async();

#if DEBUG
        webView.CoreWebView2.Navigate("http://localhost:5173");
#else
        webView.CoreWebView2.Navigate($"http://localhost:{port}");
#endif
    }
}
```

#### 2.4 创建 WebServer.cs
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AppWPF
{
    public class WebServer
    {
        private WebApplication? _app;

        public int Start()
        {
            try
            {
                Console.WriteLine("Starting web server...");
                ServerAspnet.Program.Main(new string[] { "run_async" });

                var port = ServerAspnet.Program.port;
                Console.WriteLine($"Started on port {port}");
                return port;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Server error: {ex.Message}");
                return 0;
            }
        }

        public void Stop() => _app?.StopAsync();
    }
}
```

### 3. 配置 ASP.NET Core 项目

#### 3.1 更新 ServerAspnet.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>

  <Target Name="BuildReactApp" BeforeTargets="BeforeBuild" Condition="'$(Configuration)' == 'Release'">
    <RemoveDir Directories="wwwroot" Condition="Exists('wwwroot')" />
    
    <Message Importance="high" Text="Building React App..." />
    <Exec Command="pnpm install" WorkingDirectory="../ClientReact" />
    <Exec Command="pnpm run build" WorkingDirectory="../ClientReact" />
    <ItemGroup>
      <ReactBuildFiles Include="../ClientReact/dist/**" />
    </ItemGroup>

    <MakeDir Directories="wwwroot" />
    <Copy SourceFiles="@(ReactBuildFiles)"
          DestinationFiles="@(ReactBuildFiles->'wwwroot\%(RecursiveDir)%(Filename)%(Extension)')" />
  </Target>
</Project>
```

#### 3.2 更新 Program.cs
```csharp
using System.Net;
using System.Net.NetworkInformation;

namespace ServerAspnet
{
    public class Program
    {
        public static int port = 0;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
#if DEBUG
                        .WithOrigins("http://localhost:5173")
#endif
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

#if DEBUG
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
#endif

            var app = builder.Build();

#if DEBUG
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
#endif

#if !DEBUG
            // Dynamic port assignment
            port = GetAvailablePort(5000);
            builder.WebHost.UseUrls($"http://localhost:{port}");
#endif

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.UseDefaultFiles(); // Serve index.html by default
            app.UseStaticFiles();  // Serve files from wwwroot

            // Demo API endpoint
            app.MapGet("/api/demo", () => new { message = "It works! 🎉" });

            if (args.Any(x => x == "run_async"))
                app.RunAsync();
            else
                app.Run();
        }

        public static int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            // Get active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(
                from n in connections
                where n.LocalEndPoint.Port >= startingPort
                select n.LocalEndPoint.Port
            );

            // Get active TCP listeners
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints where n.Port >= startingPort select n.Port);

            // Get active UDP listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints where n.Port >= startingPort select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }
    }
}
```

### 4. 配置 React 项目

#### 4.1 更新 package.json
```json
{
  "name": "hybrid-client-ui",
  "private": true,
  "version": "0.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "lint": "eslint .",
    "preview": "vite preview"
  },
  "dependencies": {
    "react": "^19.0.0",
    "react-dom": "^19.0.0",
    "react-router-dom": "^7.3.0"
  },
  "devDependencies": {
    "@vitejs/plugin-react": "^4.3.4",
    "vite": "^6.2.0"
  }
}
```

#### 4.2 更新 vite.config.js
```javascript
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
    plugins: [react()],
    server: {
        proxy: {
            "/api": "http://localhost:5000", // Redirect API requests to C# backend
        },
    },
});
```

#### 4.3 创建简单的 React 应用
```jsx
// src/App.jsx
import { Link } from 'react-router-dom';

function App() {
  return (
    <div>
      <nav>
        <Link to="/">Home</Link>
        <Link to="/about">About</Link>
      </nav>
      <main>
        <h1>Welcome to Hybrid App!</h1>
        <p>This is a React app running inside a WPF WebView2 control.</p>
      </main>
    </div>
  );
}

export default App;
```

### 5. 开发脚本

#### 5.1 开发环境启动脚本 (dev.ps1)
```powershell
# Start ASP.NET Core server
Start-Process dotnet -ArgumentList "run --configuration Debug --project AppWPF" -WindowStyle Hidden

# Start Vite development server
pnpm --prefix ClientReact run dev
```

#### 5.2 发布脚本 (publish.ps1)
```powershell
# Clean previous builds
$wwwrootPath = ".\ServerAspnet\wwwroot"
if (Test-Path $wwwrootPath) {
    Remove-Item -Path $wwwrootPath -Recurse -Force
}

$publishPath = ".\publish"
if (Test-Path $publishPath) {
    Remove-Item -Path $publishPath -Recurse -Force
}

# Publish the application
dotnet publish AppWPF -c Release -o publish
```

## 运行和测试

### 开发模式
1. 运行 `dev.ps1` 脚本
2. WPF 应用程序将启动并显示 WebView2 控件
3. 在开发模式下，WebView2 会连接到 Vite 开发服务器 (localhost:5173)
4. 前端代码修改会自动热重载

### 生产模式
1. 运行 `publish.ps1` 脚本
2. React 应用会被构建并复制到 ASP.NET Core 的 wwwroot 目录
3. WPF 应用程序会启动内置的 ASP.NET Core 服务器
4. WebView2 会连接到本地服务器

## 关键特性

### 1. 动态端口分配
- 生产模式下自动分配可用端口
- 避免端口冲突

### 2. 开发/生产环境分离
- 开发模式：连接到 Vite 开发服务器，支持热重载
- 生产模式：使用内置 ASP.NET Core 服务器

### 3. 自动构建集成
- 发布时自动构建 React 应用
- 自动复制构建文件到正确位置

### 4. CORS 配置
- 开发模式支持跨域请求
- 生产模式使用同源策略

## 扩展功能

### 添加 SignalR 支持
```csharp
// 在 Program.cs 中添加
builder.Services.AddSignalR();
app.MapHub<YourHub>("/yourHub");
```

### 添加更多 API 端点
```csharp
// 在 Controllers 文件夹中创建新的控制器
[ApiController]
[Route("api/[controller]")]
public class YourController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello from API!" });
    }
}
```

### 添加前端路由
```jsx
// 在 React 应用中添加路由
import { BrowserRouter, Routes, Route } from 'react-router-dom';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
      </Routes>
    </BrowserRouter>
  );
}
```

## 总结

这个混合框架提供了：
- **现代 Web 技术**：React + Vite 提供快速开发和热重载
- **强大后端**：ASP.NET Core 提供完整的 API 支持
- **原生桌面体验**：WPF + WebView2 提供原生桌面应用性能
- **灵活部署**：支持开发和生产两种模式

通过这个架构，您可以创建既具有现代 Web 界面又保持桌面应用性能的混合应用程序。
