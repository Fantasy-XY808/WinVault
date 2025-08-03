using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinVault.Constants;
using WinVault.Infrastructure;
using WinVault.Pages;

namespace WinVault.Services
{
    /// <summary>
    /// 导航服务类，用于统一管理应用程序的页面导航
    /// </summary>
    public class NavigationService : SingletonBase<NavigationService>
    {
        private static Frame? _frame;
        private readonly Dictionary<string, Type> _pageTypes = new Dictionary<string, Type>();
        private readonly SettingsService _settingsService;

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private NavigationService()
        {
            _settingsService = SettingsService.Instance;
            RegisterPages();
        }

        /// <summary>
        /// 初始化导航服务
        /// </summary>
        /// <param name="frame">导航框架</param>
        public void Initialize(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        /// <summary>
        /// 注册页面路由
        /// </summary>
        private void RegisterPages()
        {
            // 注册所有页面
            _pageTypes[AppConstants.NavigationRoutes.Home] = typeof(HomePage);
            _pageTypes[AppConstants.NavigationRoutes.Hardware] = typeof(HardwarePage);
            _pageTypes[AppConstants.NavigationRoutes.Services] = typeof(ServicesPage);
            _pageTypes[AppConstants.NavigationRoutes.CommandQuery] = typeof(CommandQueryPage);
            _pageTypes[AppConstants.NavigationRoutes.QuickSettings] = typeof(QuickSettingsPage);
            _pageTypes[AppConstants.NavigationRoutes.ExeTools] = typeof(ExeToolsPage);
            _pageTypes[AppConstants.NavigationRoutes.About] = typeof(AboutPage);
            // _pageTypes[AppConstants.NavigationRoutes.Settings] = typeof(SettingsPage); // 暂时注释
        }

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <param name="pageKey">页面键</param>
        /// <param name="parameter">页面参数</param>
        /// <returns>是否导航成功</returns>
        public bool NavigateTo(string pageKey, object? parameter = null)
        {
            EnsureFrameInitialized();
            
            if (_pageTypes.TryGetValue(pageKey, out var pageType))
            {
                // 保存最后一次打开的页面
                _settingsService.SaveSetting(AppConstants.SettingsKeys.LastOpenedPage, pageKey);
                
                // 使用连续过渡动画导航到页面
                return _frame!.Navigate(pageType, parameter, new EntranceNavigationTransitionInfo());
            }
            return false;
        }

        /// <summary>
        /// 返回上一页
        /// </summary>
        /// <returns>是否可以返回</returns>
        public bool GoBack()
        {
            EnsureFrameInitialized();
            
            if (_frame!.CanGoBack)
            {
                _frame.GoBack();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否可以返回
        /// </summary>
        public bool CanGoBack
        {
            get
            {
                EnsureFrameInitialized();
                return _frame!.CanGoBack;
            }
        }

        /// <summary>
        /// 清除导航历史
        /// </summary>
        public void ClearNavigationHistory()
        {
            EnsureFrameInitialized();
            
            while (_frame!.CanGoBack)
            {
                _frame.BackStack.RemoveAt(_frame.BackStack.Count - 1);
            }
        }

        /// <summary>
        /// 导航到主页
        /// </summary>
        public void NavigateToHome()
        {
            NavigateTo(AppConstants.NavigationRoutes.Home);
        }

        /// <summary>
        /// 导航到最后一次打开的页面或主页
        /// </summary>
        public void NavigateToLastOpenedPageOrHome()
        {
            string? lastPage = _settingsService.GetSetting<string>(AppConstants.SettingsKeys.LastOpenedPage, null);
            if (!string.IsNullOrEmpty(lastPage) && _pageTypes.ContainsKey(lastPage))
            {
                NavigateTo(lastPage);
            }
            else
            {
                NavigateToHome();
            }
        }
        
        /// <summary>
        /// 确保Frame已初始化
        /// </summary>
        private void EnsureFrameInitialized()
        {
            if (_frame == null)
            {
                throw new InvalidOperationException("NavigationService未初始化。请先调用Initialize方法。");
            }
        }
    }
} 