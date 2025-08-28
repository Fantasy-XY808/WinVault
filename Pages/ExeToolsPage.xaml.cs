using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinVault.ViewModels;

namespace WinVault.Pages
{
    public sealed partial class ExeToolsPage : Page
    {
        private ExeToolsViewModel ViewModel { get; } = new ExeToolsViewModel();

        // 获取基础目录 - 使用Assets/exetools目录
        private string BaseDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "exetools");

        public ExeToolsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private async void ExeToolsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 显示加载状态
                LoadingPanel.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
                StatsPanel.Visibility = Visibility.Collapsed;
                
                // 加载工具列表
                ViewModel.LoadTools(BaseDir);
                
                // 设置数据源
                ToolCategoriesRepeater.ItemsSource = ViewModel.ToolCategories;
                
                // 检查是否有工具
                var hasTools = ViewModel.ToolCategories.Count > 0 && 
                              ViewModel.ToolCategories.Any(c => c.HasTools);
                
                if (hasTools)
                {
                    // 显示统计信息
                    var totalCategories = ViewModel.ToolCategories.Count;
                    var totalTools = ViewModel.ToolCategories.Sum(c => c.ToolCount);
                    
                    CategoryCountText.Text = $"{totalCategories} 个分类";
                    ToolCountText.Text = $"{totalTools} 个工具";
                    
                    StatsPanel.Visibility = Visibility.Visible;
                    EmptyStatePanel.Visibility = Visibility.Collapsed;
                    
                    // 添加进入动画
                    await AnimateContentIn();
                }
                else
                {
                    // 显示空状态
                    EmptyStateMessage.Text = $"在目录中未找到任何工具。\n\n请将工具软件放置在：\n{BaseDir}\n\n支持的文件类型：.exe、.bat、.cmd、.ps1";
                    EmptyStatePanel.Visibility = Visibility.Visible;
                    StatsPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExeToolsPage加载错误: {ex.Message}");
                EmptyStateMessage.Text = $"加载工具时出错：{ex.Message}\n\n请检查目录：{BaseDir}";
                EmptyStatePanel.Visibility = Visibility.Visible;
                StatsPanel.Visibility = Visibility.Collapsed;
            }
            finally
            {
                // 隐藏加载状态
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async Task AnimateContentIn()
        {
            // 内容区域淡入动画
            ContentPanel.Opacity = 0;
            ContentPanel.Translation = new System.Numerics.Vector3(0, 50, 0);
            
            var storyboard = new Storyboard();
            
            // 透明度动画
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(opacityAnimation, ContentPanel);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            storyboard.Children.Add(opacityAnimation);
            
            storyboard.Begin();
            
            // 等待动画完成
            await Task.Delay(200);
            
            // 重置位移
            ContentPanel.Translation = new System.Numerics.Vector3(0, 0, 0);
        }

        private async void ToolCard_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string filePath)
            {
                try
                {
                    var toolItem = element.DataContext as ToolItem;
                    if (toolItem == null) return;

                    if (toolItem.IsWebLink)
                    {
                        // 打开网页链接
                        var uri = new Uri(filePath);
                        await Windows.System.Launcher.LaunchUriAsync(uri);
                    }
                    else if (File.Exists(filePath))
                    {
                        // 运行可执行文件
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true,
                            WorkingDirectory = Path.GetDirectoryName(filePath)
                        };

                        // 对于PowerShell脚本，使用PowerShell执行
                        if (toolItem.IsPowerShell)
                        {
                            startInfo.FileName = "powershell.exe";
                            startInfo.Arguments = $"-ExecutionPolicy Bypass -File \"{filePath}\"";
                        }

                        Process.Start(startInfo);
                    }
                    else
                    {
                        await ShowErrorMessage($"文件不存在：{filePath}");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorMessage($"打开工具出错：{ex.Message}");
                }
            }
        }
        
        private void ToolCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                // 鼠标悬停动画
                var storyboard = new Storyboard();
                
                var scaleAnimation = new DoubleAnimation
                {
                    To = 1.05,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                
                Storyboard.SetTarget(scaleAnimation, border.RenderTransform);
                Storyboard.SetTargetProperty(scaleAnimation, "ScaleX");
                storyboard.Children.Add(scaleAnimation);
                
                var scaleAnimationY = new DoubleAnimation
                {
                    To = 1.05,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                
                Storyboard.SetTarget(scaleAnimationY, border.RenderTransform);
                Storyboard.SetTargetProperty(scaleAnimationY, "ScaleY");
                storyboard.Children.Add(scaleAnimationY);
                
                storyboard.Begin();
            }
        }
        
        private void ToolCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                // 鼠标离开动画
                var storyboard = new Storyboard();
                
                var scaleAnimation = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                
                Storyboard.SetTarget(scaleAnimation, border.RenderTransform);
                Storyboard.SetTargetProperty(scaleAnimation, "ScaleX");
                storyboard.Children.Add(scaleAnimation);
                
                var scaleAnimationY = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                
                Storyboard.SetTarget(scaleAnimationY, border.RenderTransform);
                Storyboard.SetTargetProperty(scaleAnimationY, "ScaleY");
                storyboard.Children.Add(scaleAnimationY);
                
                storyboard.Begin();
            }
        }
        
        private async Task ShowErrorMessage(string message)
        {
            var dialog = new ContentDialog
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

