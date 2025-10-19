# TodoApp.Core

## 📁 项目结构

```
TodoApp.Core/
├── Models/
│   ├── TodoTask.cs          # Todo 任务实体
│   ├── TodoTag.cs           # Todo 标签实体
│   └── AutoSaveItem.cs      # 自动保存项基类
├── Extensions/
│   ├── GuidExtensions.cs   # Guid 扩展方法
│   └── PropertyChangedExtensions.cs # 属性变更扩展方法
└── TodoApp.Core.csproj     # 项目文件
```

## 🎯 项目职责

### 核心功能
- **实体模型**: 定义业务实体类
- **自动保存**: 提供属性变更时自动保存的基类
- **扩展方法**: 提供常用的扩展方法
- **共享类型**: 供多个项目使用的公共类型
- **业务逻辑**: 核心业务规则和验证

### 依赖关系
- **Panacean.Data**: 数据访问层
- **CommunityToolkit.Mvvm**: MVVM 工具包
- **System.Text.Json**: JSON 序列化
- **被依赖**: 被 API 和 Desktop 项目引用

## 📋 使用方式

### 在 API 项目中使用
```csharp
using TodoApp.Core.Models;

// 使用实体类
var task = new TodoTask { Title = "学习 C#", Priority = 2 };
var tag = new TodoTag { Name = "工作", Color = "#007bff" };
```

### 在 Desktop 项目中使用
```csharp
using TodoApp.Core.Models;

// 使用实体类进行数据绑定
var task = new TodoTask { Title = "桌面任务" };
```

### 使用自动保存功能
```csharp
// 继承 AutoSaveItem 实现自动保存
public class MyItem : AutoSaveItem<MyItem>
{
    public string Name { get; set; } = string.Empty;
    
    public override IDataService<MyItem, string> GetDataService()
    {
        // 返回数据服务实例
        return dataService;
    }
}

// 使用
var item = new MyItem { Name = "测试" };
item.PostInit(); // 启用自动保存
item.Name = "新名称"; // 自动保存到数据库
```

## 🔄 开发流程

1. **定义实体**: 在 `Models/` 目录下创建实体类
2. **添加扩展方法**: 在 `Extensions/` 目录下添加扩展方法
3. **更新引用**: 其他项目会自动获得新类型
4. **类型安全**: 编译时类型检查确保一致性

## 📦 包管理

- **目标框架**: .NET 8.0
- **包类型**: 类库 (Class Library)
- **依赖项**: CommunityToolkit.Mvvm, System.Text.Json

## 🚀 特性

### AutoSaveItem<TItem>
- **自动保存**: 属性变更时自动保存到数据库
- **节流控制**: 50ms 节流避免频繁保存
- **ID 生成**: 自动生成 Base62 格式的 ID
- **生命周期管理**: 支持资源释放和初始化

### 扩展方法
- **ToBase62()**: 将 Guid 转换为 Base62 字符串
- **ObserveChange()**: 观察属性变更事件
