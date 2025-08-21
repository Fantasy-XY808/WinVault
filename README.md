<div align="right">

**Language / 语言:** [🇺🇸 English](README_EN.md) | [🇨🇳 中文](README.md)

</div>

<p align="center">
<img src="https://capsule-render.vercel.app/api?type=waving&color=timeGradient&height=300&section=header&text={Welcome to our project}&fontSize=90&fontAlign=50&fontAlignY=30&desc={WinVault}&descAlign=50&descSize=30&descAlignY=60&animation=twinkling" />
</p>

<div align="center">

# 🏛️ WinVault 🏛️  
## 现代化 Windows 系统管理利器

```ascii
██╗    ██╗██╗███╗   ██╗██╗   ██╗ █████╗ ██╗   ██╗██╗  ████████╗ 
██║    ██║██║████╗  ██║██║   ██║██╔══██╗██║   ██║██║  ╚══██╔══╝ 
██║ █╗ ██║██║██╔██╗ ██║██║   ██║███████║██║   ██║██║     ██║   
██║███╗██║██║██║╚██╗██║╚██╗ ██╔╝██╔══██║██║   ██║██║     ██║   
╚███╔███╔╝██║██║ ╚████║ ╚████╔╝ ██║  ██║╚██████╔╝███████╗██║   
 ╚══╝╚══╝ ╚═╝╚═╝  ╚═══╝  ╚═══╝  ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝   
```

<p align="center">
  <img src="https://readme-typing-svg.herokuapp.com?font=Fira+Code&size=20&duration=3000&pause=1000&color=00D4FF&center=true&vCenter=true&width=500&lines=🏛️+专业Windows管理工具;⚡+WinUI+3+现代化界面;🔧+开源免费使用;💎+持续更新维护" alt="Dynamic Typing" />
</p>

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)  
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)  
[![WinUI](https://img.shields.io/badge/WinUI-3.0-brightgreen.svg)](https://docs.microsoft.com/en-us/windows/apps/winui/)  
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-lightgrey.svg)](https://www.microsoft.com/windows/)  
[![Release](https://img.shields.io/github/v/release/Fantasy-XY808/WinVault)](https://github.com/Fantasy-XY808/WinVault/releases)  
[![Downloads](https://img.shields.io/github/downloads/Fantasy-XY808/WinVault/total)](https://github.com/Fantasy-XY808/WinVault/releases)

<img src="Assets/Square150x150Logo.scale-200.png" alt="WinVault Logo" width="100" height="100" style="margin-top:20px;"/>

**让 Windows 系统管理变得简单而强大 — 现代化工具首选**

[📥 立即下载](https://github.com/Fantasy-XY808/WinVault/releases) | [📖 使用文档](#✨-功能介绍) | [🤝 参与贡献](#参与贡献) | [💬 问题反馈](https://github.com/Fantasy-XY808/WinVault/issues)

</div>

---

## ✨ 功能介绍

### 🖥️ 系统信息
- **硬件检测**：CPU、内存、显卡、存储设备详细信息一览无遗
- **实时监控**：温度、使用率、性能指标动态展示
- **系统报告**：一键生成完整系统配置报告，方便诊断与分享

### ⚙️ 服务管理
- **服务控制**：启动、停止、重启 Windows 服务，操作简便
- **依赖关系**：清晰查看服务间依赖与被依赖关系，避免误操作
- **启动类型**：灵活修改服务启动类型，满足不同需求

### 🔧 实用工具
- **命令行工具**：CMD 和 PowerShell 常用命令图形化操作，零门槛使用
- **快速设置**：一键直达系统设置，节省查找时间
- **系统清理**：智能清理临时文件和系统垃圾，释放磁盘空间

### 🎨 界面特色
- **现代设计**：基于 WinUI 3 的 Fluent Design，界面美观流畅
- **响应式布局**：兼容多种屏幕尺寸，使用体验自适应
- **主题支持**：支持浅色与深色主题自由切换，保护视力

---

## 🚀 技术特点

<div align="center">

| 技术栈 | 版本 | 说明 |
|:-------:|:----:|:----:|
| **WinUI 3** | 1.5+ | 现代 Windows 应用界面框架，流畅体验 |
| **.NET** | 8.0 | 高性能跨平台运行时，稳定高效 |
| **C#** | 12.0 | 现代编程语言特性，代码简洁易维护 |
| **MVVM** | — | 清晰分层架构，提升开发效率 |

<p align="center">
  <img src="https://skillicons.dev/icons?i=cs,dotnet,visualstudio,git,github,windows" alt="Tech icons" />
</p>

</div>

---

## 📥 快速开始

### 下载安装

**方式一：直接下载（推荐）**  
1. 访问 [Releases 页面](https://github.com/Fantasy-XY808/WinVault/releases)  
2. 下载最新版本的 `WinVault-Setup-x64.exe`  
3. 运行安装程序，按照向导完成安装  

**方式二：从源码构建**  
```bash
# 克隆项目
git clone https://github.com/Fantasy-XY808/WinVault.git
cd WinVault

# 恢复依赖并构建
dotnet restore
dotnet build -c Release

# 运行程序
dotnet run -c Release
```

---

### 系统要求

| 项目 | 最低要求 | 推荐配置 |
|:-----|:---------|:---------|
| **操作系统** | Windows 10 1809 及以上 | Windows 11 |
| **处理器** | x64 双核 1.8GHz | x64 四核 2.4GHz 及以上 |
| **内存** | 2GB RAM | 4GB+ RAM |
| **存储空间** | 100MB 可用空间 | 500MB 以上 |
| **运行时** | .NET 8.0 Runtime | .NET 8.0 SDK |

---

## 📸 界面预览

<div align="center">

> 界面截图即将发布，敬请期待！

![WinVault Preview](https://via.placeholder.com/600x400/2d2d2d/ffffff?text=WinVault+界面预览+%7C+即将发布)

</div>

---

## 📁 项目结构

```text
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

---

## 👥 开发团队

<div align="center">

### 核心贡献者

<table>
<tr>
<td align="center">
<a href="https://github.com/Fantasy-XY808">
<img src="https://github.com/Fantasy-XY808.png" width="100" alt="Fantasy-XY808"/>
<br />
<sub><b>Fantasy-XY808</b></sub>
</a>
<br />
<sub>项目创始人 & 主要开发者</sub>
</td>
<td align="center">
<a href="https://github.com/AngelShadow2017">
<img src="https://github.com/AngelShadow2017.png" width="100" alt="AngelShadow2017"/>
<br />
<sub><b>AngelShadow2017</b></sub>
</a>
<br />
<sub>项目前期维护者</sub>
</td>
<td align="center">
<a href="#">
<img src="https://pngimg.com/uploads/github/github_PNG20.png" width="100" alt="Contributor"/>
<br />
<sub><b>期待你的加入</b></sub>
</a>
<br />
<sub>欢迎更多贡献者</sub>
</td>
</tr>
</table>

[![Contributors](https://contrib.rocks/image?repo=Fantasy-XY808/WinVault)](https://github.com/Fantasy-XY808/WinVault/graphs/contributors)

### 特别感谢

- **Microsoft** - 提供 WinUI 3 框架和 .NET 平台支持  
- **开源社区** - 持续贡献技术支持与灵感  
- **所有用户** - 反馈建议让项目不断完善  

</div>

---

## 📈 开发进度

| 状态 | 功能 |
|:-----|:-----|
| ✅ 已完成 | 基础架构搭建、硬件信息模块、服务管理模块、现代化UI界面 |
| 📋 计划中 | 系统美化功能、插件系统、多语言支持、自动更新机制 |
| 🚧 开发中 | 系统优化工具、性能监控面板、网络诊断功能 |

---

## 🤝 参与贡献

欢迎任何形式的贡献！

### 如何贡献

1. **Fork** 本项目  
2. 新建功能分支 (`git checkout -b feature/AmazingFeature`)  
3. 提交代码 (`git commit -m 'Add some AmazingFeature'`)  
4. 推送分支 (`git push origin feature/AmazingFeature`)  
5. 创建 **Pull Request**

### 贡献指南

- 🐛 **Bug 报告**：发现问题请提交 Issue  
- 💡 **功能建议**：有好点子欢迎提出  
- 📝 **文档改进**：帮助完善文档内容  
- 🔧 **代码贡献**：欢迎直接提交 PR  

---

## 📜 许可证

本项目采用 [GPL-3.0](LICENSE) 许可证，详情请查看 LICENSE 文件。

---

## 📬 联系我们

- **GitHub Issues**：[提交问题](https://github.com/Fantasy-XY808/WinVault/issues)  
- **GitHub Discussions**：[参与讨论](https://github.com/Fantasy-XY808/WinVault/discussions)  
- **Email**：通过 GitHub 联系开发者

---

<div align="center">

⭐ **如果本项目对你有帮助，欢迎给我们一个 Star 支持！** ⭐

Made with ❤️ by [Fantasy-XY808](https://github.com/Fantasy-XY808) and contributors

[![Star History Chart](https://api.star-history.com/svg?repos=Fantasy-XY808/WinVault&type=Date)](https://star-history.com/#Fantasy-XY808/WinVault&Date)

<p align="center">
<img src="https://capsule-render.vercel.app/api?type=waving&color=timeGradient&height=300&section=footer&text=WinVault&fontSize=90&fontAlign=50&fontAlignY=70&desc=By Fantasy_XY808 and others&descAlign=50&descSize=30&descAlignY=40&animation=twinkling" />
</p>

</div>
```
