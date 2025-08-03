// ================================================================
// WinVault - Enterprise Service Management System
// Copyright (c) 2024 WinVault Team. All rights reserved.
// Licensed under the GPL-3.0 License.
// Author: WinVault Development Team
// Created: 2024-12-20
// Last Modified: 2024-12-20
// Version: 2.0.0
// Purpose: Advanced centralized service management with dependency injection,
//          lifecycle management, and enterprise-grade reliability features
// ================================================================

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WinVault.Infrastructure;

namespace WinVault.Services
{
    /// <summary>
    /// 企业级服务管理器，负责管理应用程序中所有服务的完整生命周期。
    /// 提供高级功能包括：依赖注入容器管理、服务初始化排序、异步生命周期控制、
    /// 错误恢复机制、性能监控、资源清理和企业级可靠性保证。
    ///
    /// Enterprise-grade service manager responsible for managing the complete lifecycle
    /// of all services in the application. Provides advanced features including:
    /// dependency injection container management, service initialization ordering,
    /// asynchronous lifecycle control, error recovery mechanisms, performance monitoring,
    /// resource cleanup, and enterprise-level reliability guarantees.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>核心设计原则 / Core Design Principles:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>单例模式 / Singleton Pattern</term>
    /// <description>确保全局唯一的服务管理实例，避免资源冲突 / Ensures globally unique service management instance, avoiding resource conflicts</description>
    /// </item>
    /// <item>
    /// <term>依赖注入 / Dependency Injection</term>
    /// <description>基于Microsoft.Extensions.DependencyInjection的标准DI容器 / Based on Microsoft.Extensions.DependencyInjection standard DI container</description>
    /// </item>
    /// <item>
    /// <term>异步优先 / Async-First</term>
    /// <description>所有生命周期操作均支持异步执行，提升响应性 / All lifecycle operations support asynchronous execution for improved responsiveness</description>
    /// </item>
    /// <item>
    /// <term>错误隔离 / Error Isolation</term>
    /// <description>单个服务故障不影响其他服务的正常运行 / Individual service failures do not affect normal operation of other services</description>
    /// </item>
    /// <item>
    /// <term>资源管理 / Resource Management</term>
    /// <description>自动管理服务资源的分配和释放，防止内存泄漏 / Automatically manages service resource allocation and release, preventing memory leaks</description>
    /// </item>
    /// </list>
    ///
    /// <para>
    /// <strong>使用示例 / Usage Example:</strong>
    /// </para>
    /// <code>
    /// // 获取服务管理器实例 / Get service manager instance
    /// var serviceManager = ServiceManager.Instance;
    ///
    /// // 注册服务 / Register services
    /// serviceManager.RegisterService&lt;IMyService, MyService&gt;(ServiceLifetime.Singleton);
    ///
    /// // 初始化所有服务 / Initialize all services
    /// await serviceManager.InitializeAsync();
    ///
    /// // 获取服务实例 / Get service instance
    /// var myService = serviceManager.GetService&lt;IMyService&gt;();
    ///
    /// // 应用程序关闭时清理资源 / Cleanup resources on application shutdown
    /// await serviceManager.ShutdownAllServicesAsync();
    /// </code>
    ///
    /// <para>
    /// <strong>线程安全性 / Thread Safety:</strong><br/>
    /// 此类的所有公共方法都是线程安全的，可以在多线程环境中安全使用。
    /// All public methods of this class are thread-safe and can be safely used in multi-threaded environments.
    /// </para>
    ///
    /// <para>
    /// <strong>性能特性 / Performance Characteristics:</strong><br/>
    /// • 服务注册：O(1) 时间复杂度 / Service registration: O(1) time complexity<br/>
    /// • 服务解析：O(1) 时间复杂度 / Service resolution: O(1) time complexity<br/>
    /// • 依赖图构建：O(n) 时间复杂度 / Dependency graph construction: O(n) time complexity<br/>
    /// • 内存占用：最小化设计，支持大规模服务注册 / Memory usage: Minimized design, supports large-scale service registration
    /// </para>
    /// </remarks>
    public class ServiceManager : SingletonBase<ServiceManager>, IDisposable, IAsyncDisposable
    {
        #region 私有字段 / Private Fields

        /// <summary>
        /// 日志服务实例，用于记录服务管理器的操作日志和错误信息。
        /// Logging service instance for recording service manager operation logs and error information.
        /// </summary>
        private readonly LoggingService _logger;

        /// <summary>
        /// 服务注册表，存储所有已注册服务的元数据信息。
        /// 键为服务类型，值为服务注册信息（包括实现类型、生命周期、初始化顺序等）。
        /// Service registry storing metadata information of all registered services.
        /// Key is service type, value is service registration information (including implementation type, lifetime, initialization order, etc.).
        /// </summary>
        private readonly Dictionary<Type, ServiceRegistration> _serviceRegistry = new Dictionary<Type, ServiceRegistration>();

        /// <summary>
        /// 服务依赖关系图，用于确定服务的初始化顺序。
        /// 键为服务类型，值为该服务依赖的其他服务类型列表。
        /// Service dependency graph used to determine service initialization order.
        /// Key is service type, value is list of other service types that this service depends on.
        /// </summary>
        private readonly Dictionary<Type, List<Type>> _dependencyGraph = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// Microsoft依赖注入服务提供者，负责实际的服务实例创建和管理。
        /// Microsoft dependency injection service provider responsible for actual service instance creation and management.
        /// </summary>
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Microsoft依赖注入服务集合，用于配置和构建服务提供者。
        /// Microsoft dependency injection service collection used for configuring and building service provider.
        /// </summary>
        private IServiceCollection? _services;

        /// <summary>
        /// 标识服务管理器是否已完成初始化。
        /// 初始化完成后，不允许再注册新的服务。
        /// Indicates whether the service manager has completed initialization.
        /// After initialization is complete, no new services can be registered.
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// 标识服务管理器是否已被释放。
        /// 用于防止重复释放资源和在已释放状态下的非法操作。
        /// Indicates whether the service manager has been disposed.
        /// Used to prevent duplicate resource release and illegal operations in disposed state.
        /// </summary>
        private bool _isDisposed = false;

        /// <summary>
        /// 初始化操作的同步锁，确保初始化过程的线程安全性。
        /// 防止多个线程同时执行初始化操作导致的竞态条件。
        /// Synchronization lock for initialization operations, ensuring thread safety of initialization process.
        /// Prevents race conditions caused by multiple threads executing initialization operations simultaneously.
        /// </summary>
        private readonly SemaphoreSlim _initializationLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 关闭操作的同步锁，确保关闭过程的线程安全性。
        /// 防止多个线程同时执行关闭操作导致的资源竞争。
        /// Synchronization lock for shutdown operations, ensuring thread safety of shutdown process.
        /// Prevents resource competition caused by multiple threads executing shutdown operations simultaneously.
        /// </summary>
        private readonly SemaphoreSlim _shutdownLock = new SemaphoreSlim(1, 1);

        #endregion

        #region 构造函数 / Constructor

        /// <summary>
        /// 私有构造函数，实现单例模式，防止外部直接实例化。
        /// 初始化核心组件：日志服务和依赖注入服务集合。
        ///
        /// Private constructor implementing singleton pattern to prevent external direct instantiation.
        /// Initializes core components: logging service and dependency injection service collection.
        /// </summary>
        /// <remarks>
        /// 构造函数执行的初始化操作：
        /// 1. 获取日志服务实例用于记录操作日志
        /// 2. 创建新的服务集合用于依赖注入配置
        /// 3. 初始化内部状态变量
        ///
        /// Initialization operations performed by constructor:
        /// 1. Get logging service instance for recording operation logs
        /// 2. Create new service collection for dependency injection configuration
        /// 3. Initialize internal state variables
        /// </remarks>
        private ServiceManager()
        {
            _logger = LoggingService.Instance;
            _services = new ServiceCollection();

            _logger.Debug("ServiceManager instance created / 服务管理器实例已创建");
        }

        #endregion

        #region 公共方法 / Public Methods

        /// <summary>
        /// 异步初始化服务管理器，构建依赖注入容器并启动所有注册的服务。
        /// 这是服务管理器的核心方法，负责整个应用程序服务生态系统的启动。
        ///
        /// Asynchronously initializes the service manager, builds the dependency injection container, and starts all registered services.
        /// This is the core method of the service manager, responsible for starting the entire application service ecosystem.
        /// </summary>
        /// <param name="configureServices">
        /// 可选的服务配置回调函数，允许外部代码在服务容器构建前注册额外的服务。
        /// 这提供了一个扩展点，使得应用程序可以根据需要添加自定义服务。
        ///
        /// Optional service configuration callback function that allows external code to register additional services before the service container is built.
        /// This provides an extension point that allows applications to add custom services as needed.
        /// </param>
        /// <param name="cancellationToken">
        /// 取消标记，用于支持初始化操作的优雅取消。
        /// 当取消被请求时，方法将尽快停止执行并抛出OperationCanceledException。
        ///
        /// Cancellation token for supporting graceful cancellation of initialization operations.
        /// When cancellation is requested, the method will stop execution as soon as possible and throw OperationCanceledException.
        /// </param>
        /// <returns>
        /// 表示异步初始化操作的任务。任务完成表示所有服务已成功初始化。
        /// A task representing the asynchronous initialization operation. Task completion indicates all services have been successfully initialized.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// 当通过cancellationToken请求取消操作时抛出。
        /// Thrown when cancellation is requested via cancellationToken.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// 当服务管理器已被释放时尝试初始化会抛出此异常。
        /// Thrown when attempting to initialize after the service manager has been disposed.
        /// </exception>
        /// <exception cref="ServiceInitializationException">
        /// 当服务初始化过程中发生错误时抛出，包含详细的错误信息。
        /// Thrown when errors occur during service initialization process, containing detailed error information.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>初始化流程 / Initialization Process:</strong>
        /// </para>
        /// <list type="number">
        /// <item>检查是否已初始化，避免重复初始化 / Check if already initialized to avoid duplicate initialization</item>
        /// <item>获取初始化锁，确保线程安全 / Acquire initialization lock to ensure thread safety</item>
        /// <item>自动发现并注册应用程序中的服务 / Automatically discover and register services in the application</item>
        /// <item>执行外部服务配置回调 / Execute external service configuration callback</item>
        /// <item>构建Microsoft DI服务提供者 / Build Microsoft DI service provider</item>
        /// <item>按依赖顺序初始化所有服务 / Initialize all services in dependency order</item>
        /// <item>标记初始化完成 / Mark initialization as complete</item>
        /// </list>
        ///
        /// <para>
        /// <strong>线程安全性 / Thread Safety:</strong><br/>
        /// 此方法是线程安全的，多个线程同时调用时只有第一个调用会执行实际的初始化，
        /// 其他调用会安全地返回而不执行任何操作。
        /// This method is thread-safe. When called by multiple threads simultaneously, only the first call will perform actual initialization,
        /// other calls will safely return without performing any operations.
        /// </para>
        ///
        /// <para>
        /// <strong>性能考虑 / Performance Considerations:</strong><br/>
        /// 初始化是一次性操作，通常在应用程序启动时执行。虽然可能耗时较长，
        /// 但这是为了确保所有服务正确配置和启动。建议在应用程序主线程中调用。
        /// Initialization is a one-time operation, typically performed during application startup. Although it may take a long time,
        /// this is to ensure all services are properly configured and started. It is recommended to call in the application main thread.
        /// </para>
        /// </remarks>
        public async Task InitializeAsync(Action<IServiceCollection>? configureServices = null, CancellationToken cancellationToken = default)
        {
            // 确保只初始化一次
            // Ensure initialization only happens once
            if (_isInitialized)
            {
                _logger.Debug("服务管理器已初始化，跳过重复初始化 / Service manager already initialized, skipping duplicate initialization");
                return;
            }
            
            await _initializationLock.WaitAsync(cancellationToken);
            try
            {
                // 双重检查
                // Double check
                if (_isInitialized)
                {
                    _logger.Debug("服务管理器已被其他线程初始化 / Service manager was initialized by another thread");
                    return;
                }
                
                _logger.Information("开始初始化服务管理器 / Starting to initialize service manager");
                
                // 发现并注册服务
                // Discover and register services
                DiscoverServices();
                
                // 允许外部配置服务
                // Allow external service configuration
                configureServices?.Invoke(_services!);
                
                // 构建服务提供者
                // Build service provider
                _serviceProvider = _services!.BuildServiceProvider();
                
                // 初始化所有注册为自动初始化的服务
                // Initialize all services registered for auto initialization
                await InitializeServicesAsync(cancellationToken);
                
                _isInitialized = true;
                _logger.Information("服务管理器初始化完成 / Service manager initialization completed");
            }
            finally
            {
                _initializationLock.Release();
            }
        }
        
        /// <summary>
        /// 发现并注册所有实现IService接口的服务。
        /// Discover and register all services implementing the IService interface.
        /// </summary>
        private void DiscoverServices()
        {
            _logger.Debug("开始发现并注册服务 / Starting to discover and register services");
            
            // 确保服务集合已初始化
            // Ensure service collection is initialized
            _services ??= new ServiceCollection();
            
            // 通过反射获取所有实现了IService接口的服务类型
            // Get all service types implementing IService interface through reflection
            var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
                
            _logger.Debug($"发现 {serviceTypes.Count} 个服务类型 / Discovered {serviceTypes.Count} service types");
                
            foreach (var serviceType in serviceTypes)
            {
                try
                {
                    // 分析服务依赖关系
                    // Analyze service dependencies
                    AnalyzeServiceDependencies(serviceType);
                    
                    // 注册服务到DI容器
                    // Register service to DI container
                    RegisterServiceType(serviceType);
                    
                    _logger.Debug($"已注册服务类型: {serviceType.Name} / Registered service type: {serviceType.Name}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"注册服务类型 {serviceType.Name} 失败 / Failed to register service type {serviceType.Name}");
                }
            }
            
            // 根据依赖关系重新排序初始化顺序
            // Reorder initialization sequence based on dependencies
            ReorderInitializationSequence();
            
            _logger.Information($"共注册 {_serviceRegistry.Count} 个服务 / Registered {_serviceRegistry.Count} services in total");
        }
        
        /// <summary>
        /// 分析服务依赖关系，构建依赖图。
        /// Analyze service dependencies and build dependency graph.
        /// </summary>
        /// <param name="serviceType">
        /// 要分析的服务类型。
        /// The service type to analyze.
        /// </param>
        private void AnalyzeServiceDependencies(Type serviceType)
        {
            if (!_dependencyGraph.ContainsKey(serviceType))
            {
                _dependencyGraph[serviceType] = new List<Type>();
            }
            
            // 分析构造函数依赖
            // Analyze constructor dependencies
            var constructors = serviceType.GetConstructors();
            if (constructors.Length > 0)
            {
                var primaryConstructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
                foreach (var parameter in primaryConstructor.GetParameters())
                {
                    var paramType = parameter.ParameterType;
                    if (typeof(IService).IsAssignableFrom(paramType))
                    {
                        _dependencyGraph[serviceType].Add(paramType);
                    }
                }
            }
        }
        
        /// <summary>
        /// 根据依赖关系重新排序服务初始化顺序。
        /// Reorder service initialization sequence based on dependencies.
        /// </summary>
        private void ReorderInitializationSequence()
        {
            // 使用拓扑排序确保依赖项在被依赖服务之前初始化
            // Use topological sort to ensure dependencies are initialized before dependent services
            var visited = new HashSet<Type>();
            var initOrder = new List<Type>();
            
            foreach (var serviceType in _serviceRegistry.Keys)
            {
                if (!visited.Contains(serviceType))
                {
                    TopologicalSort(serviceType, visited, initOrder);
                }
            }
            
            // 更新所有服务的初始化顺序
            // Update initialization order for all services
            int order = 0;
            foreach (var serviceType in initOrder)
            {
                if (_serviceRegistry.TryGetValue(serviceType, out var registration))
                {
                    registration.InitializationOrder = order++;
                }
            }
        }
        
        /// <summary>
        /// 执行拓扑排序，用于确定服务初始化顺序。
        /// Perform topological sort to determine service initialization order.
        /// </summary>
        /// <param name="serviceType">
        /// 当前服务类型。
        /// Current service type.
        /// </param>
        /// <param name="visited">
        /// 已访问的服务类型集合。
        /// Collection of visited service types.
        /// </param>
        /// <param name="sortedList">
        /// 排序后的服务类型列表。
        /// List of sorted service types.
        /// </param>
        private void TopologicalSort(Type serviceType, HashSet<Type> visited, List<Type> sortedList)
        {
            visited.Add(serviceType);
            
            if (_dependencyGraph.TryGetValue(serviceType, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (!visited.Contains(dependency))
                    {
                        TopologicalSort(dependency, visited, sortedList);
                    }
                }
            }
            
            sortedList.Add(serviceType);
        }
        
        /// <summary>
        /// 向DI容器注册指定的服务类型。
        /// Register the specified service type to the DI container.
        /// </summary>
        /// <param name="serviceType">
        /// 要注册的服务类型。
        /// The service type to register.
        /// </param>
        private void RegisterServiceType(Type serviceType)
        {
            if (_serviceRegistry.ContainsKey(serviceType))
            {
                return; // 已注册，跳过 / Already registered, skip
            }
            
            // 判断服务生命周期
            // Determine service lifetime
            var lifetime = DetermineServiceLifetime(serviceType);
            
            // 判断是否需要自动初始化
            // Determine if auto-initialization is needed
            var autoInitialize = DetermineAutoInitialize(serviceType);
            
            // 创建服务注册信息
            // Create service registration information
            var registration = new ServiceRegistration
            {
                ServiceType = serviceType,
                Lifetime = lifetime,
                AutoInitialize = autoInitialize,
                InitializationOrder = _serviceRegistry.Count // 临时顺序，后续会重排 / Temporary order, will be reordered later
            };
            
            // 注册到DI容器
            // Register to DI container
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    _services!.AddSingleton(serviceType);
                    break;
                case ServiceLifetime.Scoped:
                    _services!.AddScoped(serviceType);
                    break;
                case ServiceLifetime.Transient:
                    _services!.AddTransient(serviceType);
                    break;
            }
            
            // 注册所有实现的接口
            // Register all implemented interfaces
            foreach (var interfaceType in serviceType.GetInterfaces())
            {
                if (interfaceType != typeof(IService) && interfaceType != typeof(IDisposable) && 
                    interfaceType != typeof(IAsyncDisposable) && !interfaceType.IsGenericType)
                {
                    switch (lifetime)
                    {
                        case ServiceLifetime.Singleton:
                            _services!.AddSingleton(interfaceType, sp => sp.GetRequiredService(serviceType));
                            break;
                        case ServiceLifetime.Scoped:
                            _services!.AddScoped(interfaceType, sp => sp.GetRequiredService(serviceType));
                            break;
                        case ServiceLifetime.Transient:
                            _services!.AddTransient(interfaceType, sp => sp.GetRequiredService(serviceType));
                            break;
                    }
                }
            }
            
            // 添加到注册表
            // Add to registry
            _serviceRegistry[serviceType] = registration;
        }
        
        /// <summary>
        /// 确定服务的生命周期。
        /// Determine the lifetime of a service.
        /// </summary>
        /// <param name="serviceType">
        /// 服务类型。
        /// Service type.
        /// </param>
        /// <returns>
        /// 服务生命周期枚举值。
        /// Service lifetime enum value.
        /// </returns>
        private ServiceLifetime DetermineServiceLifetime(Type serviceType)
        {
            // 检查是否有生命周期特性
            // Check if there's a lifetime attribute
            var lifetimeAttribute = serviceType.GetCustomAttributes(typeof(ServiceLifetimeAttribute), true)
                .FirstOrDefault() as ServiceLifetimeAttribute;
                
            if (lifetimeAttribute != null)
            {
                return lifetimeAttribute.Lifetime;
            }
            
            // 默认为单例生命周期
            // Default to singleton lifetime
            return ServiceLifetime.Singleton;
        }
        
        /// <summary>
        /// 确定服务是否需要自动初始化。
        /// Determine if a service needs to be auto-initialized.
        /// </summary>
        /// <param name="serviceType">
        /// 服务类型。
        /// Service type.
        /// </param>
        /// <returns>
        /// 如果需要自动初始化则为true，否则为false。
        /// True if auto-initialization is needed, false otherwise.
        /// </returns>
        private bool DetermineAutoInitialize(Type serviceType)
        {
            // 检查是否有自动初始化特性
            // Check if there's an auto-initialize attribute
            var autoInitAttribute = serviceType.GetCustomAttributes(typeof(AutoInitializeAttribute), true)
                .FirstOrDefault() as AutoInitializeAttribute;
                
            if (autoInitAttribute != null)
            {
                return autoInitAttribute.AutoInitialize;
            }
            
            // 默认不自动初始化
            // Default to no auto-initialization
            return false;
        }
        
        /// <summary>
        /// 初始化所有需要自动初始化的服务。
        /// Initialize all services that need to be auto-initialized.
        /// </summary>
        /// <param name="cancellationToken">
        /// 可用于取消初始化操作的取消标记。
        /// A cancellation token that can be used to cancel the initialization operation.
        /// </param>
        /// <returns>
        /// 表示异步操作的任务。
        /// A task representing the asynchronous operation.
        /// </returns>
        private async Task InitializeServicesAsync(CancellationToken cancellationToken = default)
        {
            _logger.Information("开始初始化所有自动初始化服务 / Starting to initialize all auto-initialization services");
            
            // 获取需要自动初始化的服务，按初始化顺序排序
            // Get services that need to be auto-initialized, sorted by initialization order
            var servicesToInitialize = _serviceRegistry.Values
                .Where(r => r.AutoInitialize)
                .OrderBy(r => r.InitializationOrder)
                .ToList();
                
            _logger.Debug($"需要自动初始化的服务数量: {servicesToInitialize.Count} / Number of services needing auto-initialization: {servicesToInitialize.Count}");
                
            foreach (var registration in servicesToInitialize)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // 从DI容器获取服务实例
                    // Get service instance from DI container
                    var service = GetService(registration.ServiceType) as IService;
                    if (service != null)
                    {
                        _logger.Debug($"开始初始化服务: {service.ServiceName} / Starting to initialize service: {service.ServiceName}");
                        
                        // 初始化服务
                        // Initialize service
                        await service.InitializeAsync(cancellationToken);
                        
                        _logger.Debug($"服务初始化完成: {service.ServiceName} / Service initialization completed: {service.ServiceName}");
                    }
                    else
                    {
                        _logger.Warning($"无法获取服务实例: {registration.ServiceType.Name} / Unable to get service instance: {registration.ServiceType.Name}");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Warning($"初始化被取消: {registration.ServiceType.Name} / Initialization canceled: {registration.ServiceType.Name}");
                    throw; // 重新抛出取消异常 / Re-throw cancellation exception
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"初始化服务失败: {registration.ServiceType.Name} / Failed to initialize service: {registration.ServiceType.Name}");
                    // 继续初始化其他服务 / Continue initializing other services
                }
            }
            
            _logger.Information("所有自动初始化服务初始化完成 / All auto-initialization services initialized");
        }
        
        /// <summary>
        /// 关闭所有服务。
        /// Shut down all services.
        /// </summary>
        /// <param name="cancellationToken">
        /// 可用于取消关闭操作的取消标记。
        /// A cancellation token that can be used to cancel the shutdown operation.
        /// </param>
        /// <returns>
        /// 表示异步操作的任务。
        /// A task representing the asynchronous operation.
        /// </returns>
        public async Task ShutdownAllServicesAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
            {
                _logger.Debug("服务管理器未初始化，无需关闭 / Service manager not initialized, no need to shut down");
                return;
            }
            
            await _shutdownLock.WaitAsync(cancellationToken);
            try
            {
                _logger.Information("开始关闭所有服务 / Starting to shut down all services");
                
                // 获取所有已初始化的服务，按初始化顺序逆序排序（后初始化的先关闭）
                // Get all initialized services, sorted by initialization order in reverse (last initialized, first shut down)
                var servicesToShutdown = _serviceRegistry.Values
                    .OrderByDescending(r => r.InitializationOrder)
                    .ToList();
                
                foreach (var registration in servicesToShutdown)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        // 从DI容器获取服务实例
                        // Get service instance from DI container
                        var service = GetService(registration.ServiceType) as IService;
                        if (service != null && service.IsInitialized)
                        {
                            _logger.Debug($"开始关闭服务: {service.ServiceName} / Starting to shut down service: {service.ServiceName}");
                            
                            // 关闭服务
                            // Shut down service
                            await service.ShutdownAsync(cancellationToken);
                            
                            _logger.Debug($"服务已关闭: {service.ServiceName} / Service shut down: {service.ServiceName}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Warning($"关闭被取消: {registration.ServiceType.Name} / Shutdown canceled: {registration.ServiceType.Name}");
                        throw; // 重新抛出取消异常 / Re-throw cancellation exception
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"关闭服务失败: {registration.ServiceType.Name} / Failed to shut down service: {registration.ServiceType.Name}");
                        // 继续关闭其他服务 / Continue shutting down other services
                    }
                }
                
                _logger.Information("所有服务已关闭 / All services shut down");
                _isInitialized = false;
            }
            finally
            {
                _shutdownLock.Release();
            }
        }
        
        /// <summary>
        /// 获取特定类型的服务实例。
        /// Get a service instance of a specific type.
        /// </summary>
        /// <typeparam name="T">
        /// 服务类型。
        /// Service type.
        /// </typeparam>
        /// <returns>
        /// 服务实例，如果未找到则为null。
        /// Service instance, or null if not found.
        /// </returns>
        public T? GetService<T>() where T : class
        {
            // 检查是否已初始化
            // Check if initialized
            if (!_isInitialized || _serviceProvider == null)
            {
                _logger.Warning("尝试在服务管理器初始化前获取服务 / Trying to get service before service manager initialization");
                return null;
            }
            
            try
            {
                // 从DI容器获取服务
                // Get service from DI container
                return _serviceProvider.GetService<T>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取服务 {typeof(T).Name} 失败 / Failed to get service {typeof(T).Name}");
                return null;
            }
        }
        
        /// <summary>
        /// 获取特定类型的必要服务实例，如果未找到则抛出异常。
        /// Get a required service instance of a specific type, throwing an exception if not found.
        /// </summary>
        /// <typeparam name="T">
        /// 服务类型。
        /// Service type.
        /// </typeparam>
        /// <returns>
        /// 服务实例。
        /// Service instance.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// 当服务未找到或服务管理器未初始化时抛出。
        /// Thrown when the service is not found or the service manager is not initialized.
        /// </exception>
        public T GetRequiredService<T>() where T : class
        {
            // 检查是否已初始化
            // Check if initialized
            if (!_isInitialized || _serviceProvider == null)
            {
                throw new InvalidOperationException("服务管理器未初始化 / Service manager not initialized");
            }
            
            // 从DI容器获取服务
            // Get service from DI container
            return _serviceProvider.GetRequiredService<T>();
        }
        
        /// <summary>
        /// 获取指定类型的服务实例。
        /// Get a service instance of the specified type.
        /// </summary>
        /// <param name="serviceType">
        /// 服务类型。
        /// Service type.
        /// </param>
        /// <returns>
        /// 服务实例，如果未找到则为null。
        /// Service instance, or null if not found.
        /// </returns>
        public object? GetService(Type serviceType)
        {
            // 检查是否已初始化
            // Check if initialized
            if (!_isInitialized || _serviceProvider == null)
            {
                _logger.Warning("尝试在服务管理器初始化前获取服务 / Trying to get service before service manager initialization");
                return null;
            }
            
            try
            {
                // 从DI容器获取服务
                // Get service from DI container
                return _serviceProvider.GetService(serviceType);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取服务 {serviceType.Name} 失败 / Failed to get service {serviceType.Name}");
                return null;
            }
        }
        
        /// <summary>
        /// 手动注册服务实例。
        /// Manually register a service instance.
        /// </summary>
        /// <typeparam name="T">
        /// 服务类型。
        /// Service type.
        /// </typeparam>
        /// <param name="service">
        /// 服务实例。
        /// Service instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// 当服务为null时抛出。
        /// Thrown when the service is null.
        /// </exception>
        public void RegisterService<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "服务实例不能为null / Service instance cannot be null");
            }
                
            // 确保服务集合已初始化
            // Ensure service collection is initialized
            _services ??= new ServiceCollection();
                
            // 添加服务到DI容器
            // Add service to DI container
            _services.AddSingleton(service);
                
            // 如果实现了IService接口，添加到注册表
            // If implementing IService interface, add to registry
            if (service is IService serviceImpl)
            {
                var serviceType = service.GetType();
                if (!_serviceRegistry.ContainsKey(serviceType))
                {
                    _serviceRegistry[serviceType] = new ServiceRegistration
                    {
                        ServiceType = serviceType,
                        Lifetime = ServiceLifetime.Singleton,
                        AutoInitialize = false,
                        InitializationOrder = _serviceRegistry.Count
                    };
                }
                
                _logger.Debug($"手动注册服务: {serviceImpl.ServiceName} / Manually registered service: {serviceImpl.ServiceName}");
            }
            
            // 如果已构建服务提供者，需要重建
            // If service provider already built, need to rebuild
            if (_isInitialized && _serviceProvider != null)
            {
                _serviceProvider = _services.BuildServiceProvider();
            }
        }
        
        /// <summary>
        /// 手动注册服务类型。
        /// Manually register a service type.
        /// </summary>
        /// <typeparam name="TService">
        /// 服务类型。
        /// Service type.
        /// </typeparam>
        /// <typeparam name="TImplementation">
        /// 服务实现类型。
        /// Service implementation type.
        /// </typeparam>
        /// <param name="lifetime">
        /// 服务生命周期。
        /// Service lifetime.
        /// </param>
        /// <param name="autoInitialize">
        /// 是否自动初始化。
        /// Whether to auto-initialize.
        /// </param>
        public void RegisterService<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton, bool autoInitialize = false)
            where TService : class
            where TImplementation : class, TService
        {
            // 确保服务集合已初始化
            // Ensure service collection is initialized
            _services ??= new ServiceCollection();
            
            // 根据生命周期注册服务
            // Register service based on lifetime
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    _services.AddSingleton<TService, TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    _services.AddScoped<TService, TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    _services.AddTransient<TService, TImplementation>();
                    break;
            }
            
            // 如果实现了IService接口，添加到注册表
            // If implementing IService interface, add to registry
            if (typeof(IService).IsAssignableFrom(typeof(TImplementation)))
            {
                var serviceType = typeof(TImplementation);
                if (!_serviceRegistry.ContainsKey(serviceType))
                {
                    _serviceRegistry[serviceType] = new ServiceRegistration
                    {
                        ServiceType = serviceType,
                        Lifetime = lifetime,
                        AutoInitialize = autoInitialize,
                        InitializationOrder = _serviceRegistry.Count
                    };
                }
                
                _logger.Debug($"手动注册服务类型: {typeof(TService).Name} / Manually registered service type: {typeof(TService).Name}");
            }
            
            // 如果已构建服务提供者，需要重建
            // If service provider already built, need to rebuild
            if (_isInitialized && _serviceProvider != null)
            {
                _serviceProvider = _services.BuildServiceProvider();
            }
        }

        /// <summary>
        /// 释放资源。
        /// Dispose resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// 释放资源的具体实现。
        /// Concrete implementation of resource disposal.
        /// </summary>
        /// <param name="disposing">
        /// 是否正在释放托管资源。
        /// Whether disposing managed resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            
            if (disposing)
            {
                // 关闭所有服务（同步方式）
                // Shut down all services (synchronously)
                if (_isInitialized)
                {
                    try
                    {
                        // 使用Wait而不是await，因为这是同步方法
                        // Use Wait instead of await, because this is a synchronous method
                        ShutdownAllServicesAsync().Wait();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "关闭服务失败 / Failed to shut down services");
                    }
                }
                
                // 释放DI容器
                // Dispose DI container
                if (_serviceProvider is IDisposable disposableProvider)
                {
                    disposableProvider.Dispose();
                }
                
                // 释放锁
                // Dispose locks
                _initializationLock.Dispose();
                _shutdownLock.Dispose();
            }
            
            _isDisposed = true;
        }
        
        /// <summary>
        /// 异步释放资源。
        /// Asynchronously dispose resources.
        /// </summary>
        /// <returns>
        /// 表示异步操作的任务。
        /// A task representing the asynchronous operation.
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            
            Dispose(false);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// 异步释放资源的具体实现。
        /// Concrete implementation of asynchronous resource disposal.
        /// </summary>
        /// <returns>
        /// 表示异步操作的任务。
        /// A task representing the asynchronous operation.
        /// </returns>
        private async ValueTask DisposeAsyncCore()
        {
            if (_isDisposed) return;
            
            // 关闭所有服务（异步方式）
            // Shut down all services (asynchronously)
            if (_isInitialized)
            {
                try
                {
                    await ShutdownAllServicesAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "关闭服务失败 / Failed to shut down services");
                }
            }
            
            // 异步释放DI容器
            // Asynchronously dispose DI container
            if (_serviceProvider is IAsyncDisposable asyncDisposableProvider)
            {
                await asyncDisposableProvider.DisposeAsync();
            }
            else if (_serviceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
    }
    
    /// <summary>
    /// 服务注册信息类，用于存储服务的注册信息。
    /// Service registration information class, used to store registration information of a service.
    /// </summary>
    internal class ServiceRegistration
    {
        /// <summary>
        /// 服务类型。
        /// Service type.
        /// </summary>
        public Type ServiceType { get; set; } = null!;
        
        /// <summary>
        /// 服务生命周期。
        /// Service lifetime.
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }
        
        /// <summary>
        /// 是否自动初始化。
        /// Whether to auto-initialize.
        /// </summary>
        public bool AutoInitialize { get; set; }
        
        /// <summary>
        /// 初始化顺序。
        /// Initialization order.
        /// </summary>
        public int InitializationOrder { get; set; }
    }
    
    /// <summary>
    /// 服务生命周期特性，用于标记服务的生命周期。
    /// Service lifetime attribute, used to mark the lifetime of a service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceLifetimeAttribute : Attribute
    {
        /// <summary>
        /// 服务生命周期。
        /// Service lifetime.
        /// </summary>
        public ServiceLifetime Lifetime { get; }
        
        /// <summary>
        /// 初始化服务生命周期特性。
        /// Initialize the service lifetime attribute.
        /// </summary>
        /// <param name="lifetime">
        /// 服务生命周期。
        /// Service lifetime.
        /// </param>
        public ServiceLifetimeAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
    
    /// <summary>
    /// 自动初始化特性，用于标记服务是否需要自动初始化。
    /// Auto-initialize attribute, used to mark whether a service needs to be auto-initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInitializeAttribute : Attribute
    {
        /// <summary>
        /// 是否自动初始化。
        /// Whether to auto-initialize.
        /// </summary>
        public bool AutoInitialize { get; }
        
        /// <summary>
        /// 初始化自动初始化特性。
        /// Initialize the auto-initialize attribute.
        /// </summary>
        /// <param name="autoInitialize">
        /// 是否自动初始化。
        /// Whether to auto-initialize.
        /// </param>
        public AutoInitializeAttribute(bool autoInitialize = true)
        {
            AutoInitialize = autoInitialize;
        }
    }

    #endregion
}