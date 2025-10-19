# API Client Generation

## 🚀 自动生成 TypeScript 接口

### 使用方法

1. **启动 API 服务器** (在项目根目录)
   ```bash
   dotnet run --project TodoApp.Api
   ```

2. **生成 TypeScript 客户端**
   ```bash
   # 方法1: 使用 PowerShell 脚本
   powershell -ExecutionPolicy Bypass -File generate-api-simple.ps1
   
   # 方法2: 使用 pnpm 脚本
   cd TodoApp.Web
   pnpm run generate-api
   ```

3. **使用生成的客户端**
   ```typescript
   import { ApiClient, TodoTaskDto } from './api/generated/api-client';
   
   const apiClient = new ApiClient('http://localhost:5000');
   
   // 获取所有任务
   const tasks = await apiClient.todoTasks_GetTasks();
   
   // 创建新任务
   const newTask = await apiClient.todoTasks_CreateTask({
     title: '新任务',
     description: '任务描述',
     priority: 2
   });
   ```

### 📁 生成的文件

- `TodoApp.Web/src/api/generated/api-client.ts` - 自动生成的 TypeScript 客户端
- 包含所有 API 接口的类型定义
- 包含 HTTP 客户端类
- 包含所有 DTO 类型

### 🔄 更新流程

1. 修改 C# API 接口
2. 运行生成脚本
3. TypeScript 接口自动更新
4. 重新编译前端项目

### ⚙️ 配置选项

可以在 `nswag.json` 中配置：
- 输出文件路径
- TypeScript 版本
- 客户端类名
- 模板类型 (Angular, React, Vue 等)
