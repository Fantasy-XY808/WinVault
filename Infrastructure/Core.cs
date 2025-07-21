using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace WinVault.Infrastructure
{
    /// <summary>
    /// 服务接口 - 定义所有服务必须实现的基本契约
    /// Service interface - Defines basic contract that all services must implement
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 服务名称 - 用于标识和日志记录
        /// Service name - Used for identification and logging
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// 服务是否已初始化
        /// Whether the service is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 服务状态变更事件
        /// Service state change event
        /// </summary>
        event EventHandler<ServiceStateChangedEventArgs>? StateChanged;

        /// <summary>
        /// 异步初始化服务
        /// Asynchronously initialize service
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步关闭服务
        /// Asynchronously shutdown service
        /// </summary>
        Task ShutdownAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 服务状态变更事件参数
    /// Service state change event arguments
    /// </summary>
    public class ServiceStateChangedEventArgs : EventArgs
    {
        public string ServiceName { get; }
        public string OldState { get; }
        public string NewState { get; }
        public DateTime Timestamp { get; }

        public ServiceStateChangedEventArgs(string serviceName, string oldState, string newState)
        {
            ServiceName = serviceName;
            OldState = oldState;
            NewState = newState;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 服务注册接口
    /// </summary>
    public interface IServiceRegistration
    {
        void RegisterServices(IServiceCollection services);
    }

    /// <summary>
    /// 单例基类 - 提供线程安全的单例实现模式
    /// Singleton base class - Provides thread-safe singleton implementation pattern
    /// </summary>
    /// <typeparam name="T">单例类型 Singleton type</typeparam>
    public abstract class SingletonBase<T> where T : class
    {
        /// <summary>
        /// 线程安全的延迟初始化实例
        /// Thread-safe lazy initialization instance
        /// </summary>
        private static readonly Lazy<T> _instance = new(() =>
        {
            // 使用反射创建实例，支持私有构造函数
            // Use reflection to create instance, supporting private constructors
            return (T)Activator.CreateInstance(typeof(T), true)!;
        });

        /// <summary>
        /// 获取单例实例
        /// Get singleton instance
        /// </summary>
        public static T Instance => _instance.Value;

        /// <summary>
        /// 受保护的构造函数，防止外部直接实例化
        /// Protected constructor to prevent external direct instantiation
        /// </summary>
        protected SingletonBase() { }
    }

    /// <summary>
    /// 服务基类 - 提供IService接口的默认实现
    /// Service base class - Provides default implementation of IService interface
    /// </summary>
    public abstract class ServiceBase : IService
    {
        /// <summary>
        /// 服务是否已初始化
        /// Whether the service is initialized
        /// </summary>
        protected bool _isInitialized = false;

        /// <summary>
        /// 服务名称 - 子类必须实现
        /// Service name - Must be implemented by subclasses
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        /// 服务是否已初始化
        /// Whether the service is initialized
        /// </summary>
        public virtual bool IsInitialized => _isInitialized;

        /// <summary>
        /// 服务状态变更事件
        /// Service state change event
        /// </summary>
        public virtual event EventHandler<ServiceStateChangedEventArgs>? StateChanged;

        /// <summary>
        /// 异步初始化服务
        /// Asynchronously initialize service
        /// </summary>
        public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized) return Task.CompletedTask;

            OnInitialize();
            _isInitialized = true;
            OnStateChanged("NotInitialized", "Initialized");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 异步关闭服务
        /// Asynchronously shutdown service
        /// </summary>
        public virtual Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized) return Task.CompletedTask;

            OnCleanup();
            _isInitialized = false;
            OnStateChanged("Initialized", "Shutdown");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 初始化实现 - 子类可重写
        /// Initialize implementation - Can be overridden by subclasses
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 清理实现 - 子类可重写
        /// Cleanup implementation - Can be overridden by subclasses
        /// </summary>
        protected virtual void OnCleanup() { }

        /// <summary>
        /// 触发状态变更事件
        /// Trigger state change event
        /// </summary>
        protected virtual void OnStateChanged(string oldState, string newState)
        {
            StateChanged?.Invoke(this, new ServiceStateChangedEventArgs(ServiceName, oldState, newState));
        }
    }

    /// <summary>
    /// ViewModel基类
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// 命令实现
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 泛型命令实现
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (parameter is T typedParameter)
                return _canExecute?.Invoke(typedParameter) ?? true;
            return _canExecute?.Invoke(default) ?? true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T typedParameter)
                _execute(typedParameter);
            else
                _execute(default);
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
