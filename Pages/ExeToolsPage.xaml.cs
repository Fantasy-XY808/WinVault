using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WinVault.Pages
{
    public sealed partial class ExeToolsPage : Page
    {
        public class ToolItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string FilePath { get; set; }
            public string IconSource { get; set; } = "";
            public Visibility IconVisibility { get; set; } = Visibility.Collapsed;
            public string OriginalName { get; set; } // 保存原始文件名，用于显示
            public string Version { get; set; } = ""; // 版本信息
            public string Publisher { get; set; } = ""; // 发布者

            public ToolItem(string name, string description, string filePath, string? originalName = null)
            {
                Name = originalName ?? name;
                OriginalName = originalName ?? name;
                Description = description;
                FilePath = filePath;
                
                // 尝试获取文件版本信息
                try
                {
                    var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                    if (!string.IsNullOrEmpty(fileVersionInfo.FileVersion))
                    {
                        Version = fileVersionInfo.FileVersion;
                    }
                    if (!string.IsNullOrEmpty(fileVersionInfo.CompanyName))
                    {
                        Publisher = fileVersionInfo.CompanyName;
                    }
                    
                    // 更新描述
                    if (!string.IsNullOrEmpty(fileVersionInfo.FileDescription))
                    {
                        Description = fileVersionInfo.FileDescription;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"获取文件版本信息失败: {ex.Message}");
                }
            }
        }

        public class ToolCategory
        {
            public string CategoryName { get; set; }
            public ObservableCollection<ToolItem> Tools { get; set; } = new ObservableCollection<ToolItem>();
            public string EnglishName { get; set; } // 保存英文目录名

            public ToolCategory(string name, string englishName)
            {
                CategoryName = name;
                EnglishName = englishName;
            }
        }

        private ObservableCollection<ToolCategory> ToolCategories = new ObservableCollection<ToolCategory>();

        // 目录映射关系
        private readonly Dictionary<string, string> FolderMapping = new Dictionary<string, string>
        {
            { "显卡工具", "GPU_Tools" },
            { "外设工具", "Peripheral_Tools" },
            { "其他工具", "Other_Tools" },
            { "内存工具", "Memory_Tools" },
            { "压力测试", "Stress_Test_Tools" },
            { "处理器工具", "CPU_Tools" },
            { "系统诊断", "Diagnostic_Tools" },
            { "游戏工具", "Gaming_Tools" },
            { "存储工具", "Storage_Tools" },
            { "显示器工具", "Display_Tools" }
        };

        // 分类中文名称
        private readonly Dictionary<string, string> CategoryNames = new Dictionary<string, string>
        {
            { "GPU_Tools", "显卡工具" },
            { "Peripheral_Tools", "外设工具" },
            { "Other_Tools", "其他工具" },
            { "Memory_Tools", "内存工具" },
            { "Stress_Test_Tools", "压力测试" },
            { "CPU_Tools", "处理器工具" },
            { "Diagnostic_Tools", "系统诊断" },
            { "Gaming_Tools", "游戏工具" },
            { "Storage_Tools", "存储工具" },
            { "Display_Tools", "显示器工具" }
        };

        // 获取基础目录 - 使用应用程序目录下的tools文件夹
        private string BaseDir
        {
            get
            {
                // 获取应用程序目录
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // 在发布模式和开发模式下都能正确找到tools目录
                string toolsDir = Path.Combine(appDir, "tools");
                
                // 如果tools目录不存在，尝试在上级目录查找
                if (!Directory.Exists(toolsDir))
                {
                    toolsDir = Path.Combine(appDir, "..", "tools");
                }
                
                // 如果还是不存在，尝试在上上级目录查找
                if (!Directory.Exists(toolsDir))
                {
                    toolsDir = Path.Combine(appDir, "..", "..", "tools");
                }
                
                // 如果还是找不到，使用默认的exetools目录
                if (!Directory.Exists(toolsDir))
                {
                    toolsDir = Path.Combine(appDir, "exetools");
                    
                    // 如果exetools也不存在，创建它
                    if (!Directory.Exists(toolsDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(toolsDir);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"创建工具目录失败: {ex.Message}");
                        }
                    }
                }
                
                return Path.GetFullPath(toolsDir);
            }
        }

        public ExeToolsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private async void ExeToolsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 显示加载指示器
                LoadingRing.IsActive = true;
                
                // 加载工具列表
                await LoadToolCategories();
                
                // 设置ItemsSource
                ToolCategoriesRepeater.ItemsSource = ToolCategories;
                
                // 检查是否有工具
                bool hasTools = ToolCategories.Count > 0 && ToolCategories.Any(c => c.Tools.Count > 0);
                
                // 显示或隐藏无工具提示
                NoToolsMessage.Visibility = hasTools ? Visibility.Collapsed : Visibility.Visible;

                // 如果没有工具，显示详细信息
                if (!hasTools)
                {
                    NoToolsMessage.Text = $"没有找到工具软件。\n\n请将工具软件放置在以下目录中：\n{BaseDir}\n\n目录结构示例：\n{BaseDir}\\显卡工具\\GPU-Z.exe\n{BaseDir}\\处理器工具\\CPU-Z.exe";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExeToolsPage加载错误: {ex.Message}");
                NoToolsMessage.Text = $"加载工具时出错: {ex.Message}\n路径: {BaseDir}";
                NoToolsMessage.Visibility = Visibility.Visible;
            }
            finally
            {
                // 隐藏加载指示器
                LoadingRing.IsActive = false;
            }
        }

        private async Task LoadToolCategories()
        {
            // 清空现有分类
            ToolCategories.Clear();
            
            try
            {
                // 确保基础目录存在
                if (!Directory.Exists(BaseDir))
                {
                    try
                    {
                        Directory.CreateDirectory(BaseDir);
                        Debug.WriteLine($"创建基础目录: {BaseDir}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"创建基础目录失败: {ex.Message}");
                        return;
                    }
                }

                // 创建所有分类目录
                foreach (var categoryPair in FolderMapping)
                {
                    string chineseDirName = categoryPair.Key;
                    string englishDirName = categoryPair.Value;
                    
                    string chineseDir = Path.Combine(BaseDir, chineseDirName);
                    string englishDir = Path.Combine(BaseDir, englishDirName);
                    
                    // 确保中文目录存在
                    if (!Directory.Exists(chineseDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(chineseDir);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"创建目录失败: {ex.Message}");
                        }
                    }
                    
                    // 创建分类
                    var category = new ToolCategory(chineseDirName, englishDirName);
                    
                    // 扫描中文目录下的工具
                    if (Directory.Exists(chineseDir))
                    {
                        await ScanDirectory(chineseDir, category, englishDirName);
                    }
                    
                    // 如果分类下有工具，添加到列表中
                    if (category.Tools.Count > 0)
                    {
                        ToolCategories.Add(category);
                    }
                }
                
                // 如果没有找到任何工具，尝试扫描整个基础目录
                if (ToolCategories.Count == 0)
                {
                    var otherCategory = new ToolCategory("其他工具", "Other_Tools");
                    await ScanDirectory(BaseDir, otherCategory, "Other_Tools", false);
                    
                    if (otherCategory.Tools.Count > 0)
                    {
                        ToolCategories.Add(otherCategory);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载工具分类出错: {ex.Message}");
                throw; // 重新抛出异常，让上层处理
            }
        }

        private async Task ScanDirectory(string directory, ToolCategory category, string englishCategoryName, bool recursive = true)
        {
            try
            {
                // 检查目录是否存在
                if (!Directory.Exists(directory))
                {
                    Debug.WriteLine($"目录不存在，无法扫描: {directory}");
                    return;
                }
                
                // 先扫描当前目录中的exe文件
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (Path.GetExtension(file).ToLower() == ".exe")
                    {
                        // 排除系统文件和库文件
                        string fileName = Path.GetFileName(file).ToLower();
                        if (!fileName.StartsWith("microsoft.") && 
                            !fileName.StartsWith("system.") && 
                            !fileName.Contains("setup") &&
                            !fileName.Contains("installer") &&
                            !fileName.Contains("uninstall"))
                        {
                            await ProcessExeFile(file, category, englishCategoryName);
                        }
                    }
                }

                // 如果需要递归扫描子目录
                if (recursive)
                {
                    // 然后递归扫描子目录
                    foreach (var subDir in Directory.GetDirectories(directory))
                    {
                        // 排除特定的系统目录和隐藏目录
                        string dirName = Path.GetFileName(subDir).ToLower();
                        if (!dirName.StartsWith(".") && 
                            !dirName.Equals("bin") && 
                            !dirName.Equals("obj") && 
                            !dirName.Equals("lib") &&
                            !dirName.Equals("libs") &&
                            !dirName.Equals("x86") &&
                            !dirName.Equals("x64"))
                        {
                            await ScanDirectory(subDir, category, englishCategoryName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描目录出错: {ex.Message}");
            }
        }

        private Task ProcessExeFile(string filePath, ToolCategory category, string englishCategoryName)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                // 创建工具项
                var toolItem = new ToolItem(
                    name: fileName,
                    description: "双击运行此工具",
                    filePath: filePath,
                    originalName: fileName
                );
                
                // 检查是否已经添加了相同路径的工具
                bool alreadyExists = false;
                foreach (var existingTool in category.Tools)
                {
                    if (string.Equals(existingTool.FilePath, toolItem.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyExists = true;
                        break;
                    }
                }
                
                // 只有不存在时才添加
                if (!alreadyExists)
                {
                    // 添加到分类中 - 改用同步方法
                    DispatcherQueue.TryEnqueue(() => category.Tools.Add(toolItem));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理可执行文件出错: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        private void ToolCard_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string filePath)
            {
                try
                {
                    // 检查文件是否存在
                    if (File.Exists(filePath))
                    {
                        // 启动进程
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true,
                            WorkingDirectory = Path.GetDirectoryName(filePath) // 设置工作目录为可执行文件所在目录
                        });
                    }
                    else
                    {
                        // 文件不存在，显示错误提示
                        ShowErrorMessage($"文件不存在: {filePath}");
                        Debug.WriteLine($"文件不存在: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"打开工具出错: {ex.Message}");
                    Debug.WriteLine($"打开工具出错: {ex.Message}");
                }
            }
        }
        
        private async void ShowErrorMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "错误",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            
            await dialog.ShowAsync();
        }
    }
} 