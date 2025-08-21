<div align="right">

**Language / 语言:** [🇺🇸 English](README_EN.md) | [🇨🇳 中文](README.md)

</div>

<div align="center">

# 🏛️ WinVault
## 现代化Windows系统管理工具

```ascii
██╗    ██╗██╗███╗   ██╗██╗   ██╗ █████╗ ██╗   ██╗██╗  ████████╗
██║    ██║██║████╗  ██║██║   ██║██╔══██╗██║   ██║██║  ╚══██╔══╝
██║ █╗ ██║██║██╔██╗ ██║██║   ██║███████║██║   ██║██║     ██║
██║███╗██║██║██║╚██╗██║╚██╗ ██╔╝██╔══██║██║   ██║██║     ██║
╚███╔███╔╝██║██║ ╚████║ ╚████╔╝ ██║  ██║╚██████╔╝███████╗██║
 ╚══╝╚══╝ ╚═╝╚═╝  ╚═══╝  ╚═══╝  ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝
```

<p align="center">
  <img src="https://readme-typing-svg.herokuapp.com?font=Fira+Code&size=20&duration=3000&pause=1000&color=00D4FF&center=true&vCenter=true&width=500&lines=🏛️+专业Windows管理工具;⚡+WinUI+3+现代界面;🔧+开源免费使用;💎+持续更新维护" alt="Dynamic Typing" />
</p>

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![WinUI](https://img.shields.io/badge/WinUI-3.0-brightgreen.svg)](https://docs.microsoft.com/en-us/windows/apps/winui/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-lightgrey.svg)](https://www.microsoft.com/windows/)
[![Release](https://img.shields.io/github/v/release/Fantasy-XY808/WinVault)](https://github.com/Fantasy-XY808/WinVault/releases)
[![Downloads](https://img.shields.io/github/downloads/Fantasy-XY808/WinVault/total)](https://github.com/Fantasy-XY808/WinVault/releases)

<img src="Assets/Square150x150Logo.scale-200.png" alt="WinVault Logo" width="100" height="100">

**一个让Windows系统管理变得简单而强大的现代化工具**

[📥 立即下载](https://github.com/Fantasy-XY808/WinVault/releases) | [📖 使用文档](#功能介绍) | [🤝 参与贡献](#参与贡献) | [💬 问题反馈](https://github.com/Fantasy-XY808/WinVault/issues)

</div>

---

## ✨ 功能介绍

### 🖥️ **系统信息**
- **硬件检测** - CPU、内存、显卡、存储设备详细信息
- **实时监控** - 温度、使用率、性能指标
- **系统报告** - 生成详细的系统配置报告

### ⚙️ **服务管理**
- **服务控制** - 启动、停止、重启Windows服务
- **依赖关系** - 查看服务依赖和被依赖关系
- **启动类型** - 修改服务启动类型和配置

### 🔧 **实用工具**
- **命令行工具** - 常用CMD和PowerShell命令图形化
- **快速设置** - 一键访问Windows系统设置
- **系统清理** - 清理临时文件和系统垃圾

### 🎨 **界面特色**
- **现代设计** - 基于WinUI 3的Fluent Design
- **响应式布局** - 适配不同屏幕尺寸
- **主题支持** - 支持浅色/深色主题切换

## 🚀 技术特点

<div align="center">

| 技术 | 版本 | 说明 |
|:---:|:---:|:---:|
| **WinUI 3** | 1.5+ | 现代Windows应用界面框架 |
| **.NET** | 8.0 | 高性能跨平台运行时 |
| **C#** | 12.0 | 现代编程语言特性 |
| **MVVM** | - | 清晰的架构模式 |

<p align="center">
  <img src="https://skillicons.dev/icons?i=cs,dotnet,visualstudio,git,github,windows" />
</p>

</div>

## 📥 快速开始

### 下载安装

**方式一：直接下载（推荐）**
1. 访问 [Releases页面](https://github.com/Fantasy-XY808/WinVault/releases)
2. 下载最新版本的 `WinVault-Setup-x64.exe`
3. 运行安装程序，按提示完成安装

**方式二：从源码构建**
```bash
# 克隆项目
git clone https://github.com/Fantasy-XY808/WinVault.git
cd WinVault

# 构建项目
dotnet restore
dotnet build -c Release

# 运行
dotnet run -c Release
```

### 系统要求

| 项目 | 最低要求 | 推荐配置 |
|:---:|:---:|:---:|
| **操作系统** | Windows 10 1809+ | Windows 11 |
| **处理器** | x64 双核 1.8GHz | x64 四核 2.4GHz+ |
| **内存** | 2GB RAM | 4GB+ RAM |
| **存储** | 100MB 可用空间 | 500MB+ |
| **运行时** | .NET 8.0 Runtime | .NET 8.0 SDK |

## 📸 界面预览

<div align="center">

> 界面截图正在准备中，敬请期待...

![WinVault Preview](https://via.placeholder.com/600x400/2d2d2d/ffffff?text=WinVault+界面预览+%7C+即将发布)

</div>

## 📁 项目结构

```
WinVault/
├── 📱 Pages/              # 应用页面
├── 🎛️ Controls/           # 自定义控件
├── 📊 ViewModels/         # 视图模型
├── 🔧 Services/           # 业务服务
├── 📋 Models/             # 数据模型
├── 🛠️ Utilities/          # 工具类
├── 📦 Assets/             # 资源文件
├── 🧪 Tests/              # 测试项目
└── 📄 README.md           # 项目说明
```

## 👥 开发团队

<div align="center">

### 核心贡献者

<table>
<tr>
<td align="center">
<a href="https://github.com/Fantasy-XY808">
<img src="https://github.com/Fantasy-XY808.png" width="100px;" alt="Fantasy-XY808"/>
<br />
<sub><b>Fantasy-XY808</b></sub>
</a>
<br />
<sub>项目创始人 & 主要开发者</sub>
<td align="center">
<a href="https://github.com/AngelShadow2017">
<img src="https://github.com/AngelShadow2017.png" width="100px;" alt="AngelShadow2017"/>
<br />
<sub><b>AngelShadow2017</b></sub>
</a>
<br />
<sub>项目前期维护者</sub>
</td>
<td align="center">
<a href="#">
<img src="https://via.placeholder.com/100x100/f0f0f0/666?text=You" width="100px;" alt="Contributor"/>
<br />
<sub><b>等待你的加入</b></sub>
</a>
<br />
<sub>期待更多贡献者</sub>
</td>
</tr>
</table>

[![Contributors](https://contrib.rocks/image?repo=Fantasy-XY808/WinVault)](https://github.com/Fantasy-XY808/WinVault/graphs/contributors)

### 特别感谢

- **Microsoft** - 提供WinUI 3框架和.NET平台
- **开源社区** - 提供技术支持和灵感
- **所有用户** - 提供反馈和建议

</div>

## 📈 开发进度

### ✅ 已完成
- [x] 基础架构搭建
- [x] 硬件信息模块
- [x] 服务管理模块
- [x] 现代化UI界面

### 🚧 开发中
- [/] 系统优化工具
- [/] 性能监控面板
- [/] 网络诊断功能

### 📋 计划中
- [ ] 插件系统
- [ ] 多语言支持
- [ ] 自动更新机制

## 参与贡献

我们欢迎任何形式的贡献！

### 如何贡献

1. **Fork** 这个项目
2. 创建你的功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交你的更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开一个 **Pull Request**

### 贡献指南

- 🐛 **Bug报告**: 发现问题请提交Issue
- � **功能建议**: 有好想法请告诉我们
- 📝 **文档改进**: 帮助完善文档
- 🔧 **代码贡献**: 直接提交PR

## 许可证

本项目采用 [GPL-3.0](LICENSE) 许可证 - 详情请查看 LICENSE 文件

## 联系我们

- **GitHub Issues**: [提交问题](https://github.com/Fantasy-XY808/WinVault/issues)
- **GitHub Discussions**: [参与讨论](https://github.com/Fantasy-XY808/WinVault/discussions)
- **Email**: 通过GitHub联系

---

<div align="center">

**如果这个项目对你有帮助，请给个 ⭐ Star 支持一下！**

**Made with ❤️ by [Fantasy-XY808](https://github.com/Fantasy-XY808) and contributors**

[![Star History Chart](https://api.star-history.com/svg?repos=Fantasy-XY808/WinVault&type=Date)](https://star-history.com/#Fantasy-XY808/WinVault&Date)

</div>
