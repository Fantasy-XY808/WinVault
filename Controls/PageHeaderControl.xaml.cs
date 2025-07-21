using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace WinVault.Controls
{
    /// <summary>
    /// 页面标题栏控件
    /// 提供统一的页面标题、描述、状态显示功能
    /// </summary>
    public sealed partial class PageHeaderControl : UserControl
    {
        /// <summary>
        /// 页面标题依赖属性
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageHeaderControl),
                new PropertyMetadata(string.Empty, OnTitleChanged));

        /// <summary>
        /// 页面描述依赖属性
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(PageHeaderControl),
                new PropertyMetadata(string.Empty, OnDescriptionChanged));

        /// <summary>
        /// 页面图标依赖属性
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(PageHeaderControl),
                new PropertyMetadata("\uE7C3", OnIconChanged));

        /// <summary>
        /// 状态文本依赖属性
        /// </summary>
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(PageHeaderControl),
                new PropertyMetadata("正常", OnStatusChanged));

        /// <summary>
        /// 状态类型依赖属性
        /// </summary>
        public static readonly DependencyProperty StatusTypeProperty =
            DependencyProperty.Register(nameof(StatusType), typeof(StatusType), typeof(PageHeaderControl),
                new PropertyMetadata(StatusType.Success, OnStatusTypeChanged));

        /// <summary>
        /// 初始化页面标题栏控件
        /// </summary>
        public PageHeaderControl()
        {
            this.InitializeComponent();
            UpdateTimestamp();
        }

        /// <summary>
        /// 页面标题
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// 页面描述
        /// </summary>
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        /// 页面图标
        /// </summary>
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// 状态文本
        /// </summary>
        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        /// <summary>
        /// 状态类型
        /// </summary>
        public StatusType StatusType
        {
            get => (StatusType)GetValue(StatusTypeProperty);
            set => SetValue(StatusTypeProperty, value);
        }

        /// <summary>
        /// 标题变更处理
        /// </summary>
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageHeaderControl control)
            {
                control.PageTitle.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// 描述变更处理
        /// </summary>
        private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageHeaderControl control)
            {
                control.PageDescription.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// 图标变更处理
        /// </summary>
        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageHeaderControl control)
            {
                control.PageIcon.Glyph = e.NewValue?.ToString() ?? "\uE7C3";
            }
        }

        /// <summary>
        /// 状态变更处理
        /// </summary>
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageHeaderControl control)
            {
                control.StatusText.Text = e.NewValue?.ToString() ?? "正常";
                control.UpdateTimestamp();
            }
        }

        /// <summary>
        /// 状态类型变更处理
        /// </summary>
        private static void OnStatusTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageHeaderControl control)
            {
                control.UpdateStatusAppearance((StatusType)e.NewValue);
            }
        }

        /// <summary>
        /// 更新状态外观
        /// </summary>
        private void UpdateStatusAppearance(StatusType statusType)
        {
            var (background, icon) = statusType switch
            {
                StatusType.Success => ("#28A745", "\uE73E"),
                StatusType.Warning => ("#FFC107", "\uE7BA"),
                StatusType.Error => ("#DC3545", "\uE783"),
                StatusType.Info => ("#17A2B8", "\uE946"),
                _ => ("#28A745", "\uE73E")
            };

            StatusIndicator.Background = new SolidColorBrush(ColorHelper.FromArgb(255,
                Convert.ToByte(background.Substring(1, 2), 16),
                Convert.ToByte(background.Substring(3, 2), 16),
                Convert.ToByte(background.Substring(5, 2), 16)));
            StatusIcon.Glyph = icon;
        }

        /// <summary>
        /// 更新时间戳
        /// </summary>
        private void UpdateTimestamp()
        {
            TimestampText.Text = DateTime.Now.ToString("HH:mm 更新");
        }
    }

    /// <summary>
    /// 状态类型枚举
    /// </summary>
    public enum StatusType
    {
        Success,
        Warning,
        Error,
        Info
    }

    /// <summary>
    /// 颜色辅助类
    /// </summary>
    public static class ColorHelper
    {
        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return Color.FromArgb(a, r, g, b);
        }
    }
}
