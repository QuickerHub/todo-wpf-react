# TodoApp - WPF + WebView2 + ASP.NET Core + React 混合桌面应用

## 项目概述

本项目是一个现代化的混合桌面应用程序，结合了：
- **WPF** 作为桌面应用框架
- **WebView2** 用于嵌入现代 Web 界面
- **ASP.NET Core** 作为后端 API 服务器
- **React** 作为前端 UI 框架

## 技术栈

- **桌面应用**: WPF (.NET 8.0)
- **后端 API**: ASP.NET Core (.NET 8.0)
- **前端 UI**: React 19 + TypeScript + Vite
- **包管理器**: pnpm (必须使用)

## 项目结构

```
todo-wpf-react/
├── TodoApp.Desktop/          # WPF 桌面应用程序
├── TodoApp.Api/             # ASP.NET Core API 后端
├── TodoApp.Web/             # React 前端应用
└── TodoApp.sln              # Visual Studio 解决方案
```

## 环境要求

### 必需软件
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [pnpm](https://pnpm.io/installation) (推荐包管理器)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) 或 [Visual Studio Code](https://code.visualstudio.com/)

### 安装 pnpm
```bash
npm install -g pnpm
```

## 快速开始

### 1. 克隆项目
```bash
git clone <repository-url>
cd todo-wpf-react
```

### 2. 安装依赖
```bash
# 安装前端依赖
cd TodoApp.Web
pnpm install

# 返回根目录
cd ..
```

### 3. 启动开发环境

#### 方法一：分别启动（推荐用于开发）

**启动后端 API：**
```bash
cd TodoApp.Api
dotnet run
```

**启动前端开发服务器：**
```bash
cd TodoApp.Web
pnpm run dev
```

**启动桌面应用：**
```bash
cd TodoApp.Desktop
dotnet run
```

#### 方法二：使用 PowerShell 脚本（一键启动）

创建 `start-dev.ps1` 脚本：
```powershell
# 启动后端 API
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd TodoApp.Api; dotnet run"

# 等待 2 秒
Start-Sleep -Seconds 2

# 启动前端开发服务器
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd TodoApp.Web; pnpm run dev"

# 等待 3 秒
Start-Sleep -Seconds 3

# 启动桌面应用
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd TodoApp.Desktop; dotnet run"
```

运行脚本：
```powershell
.\start-dev.ps1
```

## 开发调试

### 前端调试

1. **启动 Vite 开发服务器**
   ```bash
   cd TodoApp.Web
   pnpm run dev
   ```
   - 服务器运行在 `http://localhost:5173`
   - 支持热重载和快速刷新

2. **调试 React 组件**
   - 使用浏览器开发者工具
   - 安装 React Developer Tools 扩展

### 后端调试

1. **启动 ASP.NET Core API**
   ```bash
   cd TodoApp.Api
   dotnet run
   ```
   - API 运行在 `http://localhost:5000` (或动态端口)
   - 访问 `http://localhost:5000/swagger` 查看 API 文档

2. **Visual Studio 调试**
   - 在 Visual Studio 中打开 `TodoApp.sln`
   - 设置 `TodoApp.Api` 为启动项目
   - 按 F5 开始调试

### 桌面应用调试

1. **启动 WPF 应用**
   ```bash
   cd TodoApp.Desktop
   dotnet run
   ```

2. **Visual Studio 调试**
   - 设置 `TodoApp.Desktop` 为启动项目
   - 按 F5 开始调试
   - 可以设置断点调试 C# 代码

### 完整调试流程

1. **启动后端 API**
   ```bash
   cd TodoApp.Api
   dotnet run
   ```

2. **启动前端开发服务器**
   ```bash
   cd TodoApp.Web
   pnpm run dev
   ```

3. **启动桌面应用**
   ```bash
   cd TodoApp.Desktop
   dotnet run
   ```

4. **调试说明**
   - 前端修改会自动热重载
   - 后端修改需要重启 API 服务器
   - 桌面应用修改需要重新编译

## 生产构建

### 构建前端
```bash
cd TodoApp.Web
pnpm run build
```

### 发布桌面应用
```bash
# 发布整个应用
dotnet publish TodoApp.Desktop -c Release -o publish

# 或使用 Visual Studio 发布功能
```

## 常见问题

### 1. pnpm 安装失败
```bash
# 确保已安装 Node.js
node --version

# 全局安装 pnpm
npm install -g pnpm

# 验证安装
pnpm --version
```

### 2. 端口冲突
- 前端默认端口：5173
- 后端默认端口：5000
- 如果端口被占用，Vite 会自动选择其他端口

### 3. WebView2 问题
- 确保系统已安装 WebView2 运行时
- 下载地址：https://developer.microsoft.com/en-us/microsoft-edge/webview2/

### 4. 依赖安装问题
```bash
# 清理缓存
pnpm store prune

# 重新安装
rm -rf node_modules
pnpm install
```

## 开发建议

1. **使用 pnpm** 而不是 npm 或 yarn
2. **开发时保持三个服务都运行**：API、前端、桌面应用
3. **使用 Visual Studio** 进行 C# 代码调试
4. **使用浏览器开发者工具** 调试前端代码
5. **定期清理构建缓存** 避免奇怪的问题

## 贡献指南

1. Fork 项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建 Pull Request

## 许可证

[添加许可证信息]
