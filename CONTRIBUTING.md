# 🤝 贡献指南

感谢您对WinVault项目的关注！我们欢迎所有形式的贡献，包括但不限于代码、文档、测试、反馈和建议。

## 📋 目录

- [🚀 快速开始](#-快速开始)
- [🔧 开发环境设置](#-开发环境设置)
- [📝 贡献类型](#-贡献类型)
- [🌿 分支策略](#-分支策略)
- [💻 代码规范](#-代码规范)
- [🧪 测试要求](#-测试要求)
- [📤 提交流程](#-提交流程)
- [🐛 问题报告](#-问题报告)
- [💡 功能建议](#-功能建议)

## 🚀 快速开始

1. **Fork项目** - 点击右上角的Fork按钮
2. **克隆仓库** - `git clone https://github.com/YOUR_USERNAME/WinVault.git`
3. **创建分支** - `git checkout -b feature/your-feature-name`
4. **进行更改** - 编写代码、文档或测试
5. **提交更改** - `git commit -m "Add your feature"`
6. **推送分支** - `git push origin feature/your-feature-name`
7. **创建PR** - 在GitHub上创建Pull Request

## 🔧 开发环境设置

### 必需软件
- **Visual Studio 2022** (17.8+) 或 **Visual Studio Code**
- **.NET 8.0 SDK**
- **Windows 10 SDK** (10.0.19041.0+)
- **Git**

### 推荐工具
- **Windows Terminal**
- **PowerShell 7+**
- **GitHub Desktop** (可选)

### 环境配置
```bash
# 克隆项目
git clone https://github.com/Fantasy-XY808/WinVault.git
cd WinVault

# 还原依赖
dotnet restore

# 构建项目
dotnet build -c Debug -p:Platform=x64

# 运行项目
dotnet run --project WinVault.csproj
```

## 📝 贡献类型

### 🐛 Bug修复
- 修复现有功能的问题
- 改进错误处理
- 性能优化

### ✨ 新功能
- 添加新的系统管理功能
- 改进用户界面
- 增强用户体验

### 📚 文档改进
- 更新README文档
- 添加代码注释
- 编写使用教程

### 🧪 测试
- 编写单元测试
- 添加集成测试
- 改进测试覆盖率

## 🌿 分支策略

### 主要分支
- **`main`** - 稳定的生产版本
- **`develop`** - 开发分支，包含最新功能

### 功能分支命名规范
- **功能**: `feature/功能名称`
- **修复**: `bugfix/问题描述`
- **文档**: `docs/文档类型`
- **测试**: `test/测试类型`

示例：
```bash
feature/hardware-monitoring
bugfix/memory-leak-fix
docs/api-documentation
test/unit-tests-services
```

## 💻 代码规范

### C# 编码规范
- 遵循 [Microsoft C# 编码约定](https://docs.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- 使用 PascalCase 命名类、方法、属性
- 使用 camelCase 命名局部变量、参数
- 添加有意义的注释，支持中英双语

### XAML 规范
- 使用清晰的元素命名
- 保持合理的缩进和格式
- 使用资源字典管理样式

### 代码示例
```csharp
/// <summary>
/// 系统信息服务 / System Information Service
/// 提供硬件和系统信息查询功能 / Provides hardware and system information query functionality
/// </summary>
public class SystemInfoService : ISystemInfoService
{
    private readonly ILogger<SystemInfoService> _logger;
    
    public SystemInfoService(ILogger<SystemInfoService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// 获取CPU信息 / Get CPU Information
    /// </summary>
    /// <returns>CPU详细信息 / CPU detailed information</returns>
    public async Task<CpuInfo> GetCpuInfoAsync()
    {
        try
        {
            // 实现逻辑
            return new CpuInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取CPU信息失败 / Failed to get CPU information");
            throw;
        }
    }
}
```

## 🧪 测试要求

### 单元测试
- 为新功能编写单元测试
- 测试覆盖率应达到80%以上
- 使用MSTest框架

### 测试命名规范
```csharp
[TestMethod]
public void GetCpuInfo_WhenCalled_ShouldReturnValidCpuInfo()
{
    // Arrange
    // Act
    // Assert
}
```

### 运行测试
```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test Tests/WinVault.Tests.csproj

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

## 📤 提交流程

### 提交信息格式
```
类型(范围): 简短描述

详细描述（可选）

相关Issue: #123
```

### 提交类型
- **feat**: 新功能
- **fix**: Bug修复
- **docs**: 文档更新
- **style**: 代码格式调整
- **refactor**: 代码重构
- **test**: 测试相关
- **chore**: 构建过程或辅助工具的变动

### 示例
```bash
feat(hardware): 添加GPU温度监控功能

- 实现GPU温度实时监控
- 添加温度警告阈值设置
- 优化温度显示界面

相关Issue: #45
```

## 🐛 问题报告

使用GitHub Issues报告问题时，请包含：

### 必需信息
- **问题描述**: 清晰描述遇到的问题
- **重现步骤**: 详细的重现步骤
- **预期行为**: 期望的正确行为
- **实际行为**: 实际发生的行为
- **环境信息**: 操作系统、.NET版本等

### 问题模板
```markdown
## 问题描述
简要描述问题

## 重现步骤
1. 打开应用程序
2. 点击...
3. 看到错误...

## 预期行为
应该显示...

## 实际行为
实际显示...

## 环境信息
- OS: Windows 11 22H2
- .NET: 8.0
- WinVault版本: 1.0.0

## 附加信息
其他相关信息、截图等
```

## 💡 功能建议

使用GitHub Discussions提出功能建议：

1. **搜索现有建议** - 避免重复
2. **详细描述功能** - 包含使用场景
3. **说明价值** - 解释为什么需要这个功能
4. **提供示例** - 如果可能，提供界面设计或代码示例

## 📞 联系方式

- **GitHub Issues**: 问题报告和Bug反馈
- **GitHub Discussions**: 功能建议和讨论
- **Pull Requests**: 代码贡献

## 🙏 致谢

感谢所有为WinVault项目做出贡献的开发者！您的每一份贡献都让这个项目变得更好。

---

**让我们一起打造更强大的WinVault系统宝库！** 🏛️
