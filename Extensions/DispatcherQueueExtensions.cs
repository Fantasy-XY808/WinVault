// ================================================================
// WinVault
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Author: WinVault Team
// Created: 2024
// Purpose: Extension methods for DispatcherQueue
// ================================================================

using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace WinVault.Extensions
{
    /// <summary>
    /// 为DispatcherQueue提供扩展方法，简化UI线程操作。
    /// 提供异步排队、错误处理和优先级控制功能。
    /// Extension methods for DispatcherQueue to simplify UI thread operations.
    /// Provides asynchronous queuing, error handling, and priority control functionality.
    /// </summary>
    /// <remarks>
    /// 这些扩展方法解决了以下问题：
    /// 1. 简化UI线程调度操作
    /// 2. 提供异步等待支持
    /// 3. 统一的错误处理机制
    /// 4. 支持不同优先级的任务调度
    /// 
    /// These extension methods solve the following problems:
    /// 1. Simplify UI thread dispatch operations
    /// 2. Provide async/await support
    /// 3. Unified error handling mechanism
    /// 4. Support task scheduling with different priorities
    /// </remarks>
    public static class DispatcherQueueExtensions
    {
        /// <summary>
        /// 将一个操作异步排队到 UI 线程。
        /// Asynchronously queues an operation to the UI thread.
        /// </summary>
        /// <param name="dispatcherQueue">调度器队列 / Dispatcher queue</param>
        /// <param name="action">要执行的操作 / Action to execute</param>
        /// <param name="priority">优先级 / Priority</param>
        /// <returns>异步任务，表示操作是否成功入队 / Async task indicating whether the operation was successfully queued</returns>
        public static Task<bool> TryEnqueueAsync(
            this DispatcherQueue dispatcherQueue,
            Action action,
            DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            
            // 使用同步方法将任务排队
            // Use synchronous method to queue the task
            bool result = dispatcherQueue.TryEnqueue(priority, () =>
            {
                try
                {
                    action();
                    taskCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            
            if (!result)
            {
                taskCompletionSource.SetResult(false);
            }
            
            return taskCompletionSource.Task;
        }
        
        /// <summary>
        /// 将同步回调异步排队到UI线程。
        /// Asynchronously queues a synchronous callback to the UI thread.
        /// </summary>
        /// <param name="dispatcher">调度器 / Dispatcher</param>
        /// <param name="callback">回调方法 / Callback method</param>
        /// <param name="priority">优先级 / Priority</param>
        /// <returns>表示操作完成的任务 / Task representing operation completion</returns>
        public static Task EnqueueAsync(this DispatcherQueue dispatcher, DispatcherQueueHandler callback, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));
            
            var taskCompletionSource = new TaskCompletionSource<bool>();
            
            if (!dispatcher.TryEnqueue(priority, () =>
            {
                try
                {
                    callback();
                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            }))
            {
                taskCompletionSource.TrySetException(new InvalidOperationException("Failed to enqueue task to dispatcher"));
            }
            
            return taskCompletionSource.Task;
        }
        
        /// <summary>
        /// 将异步回调排队到UI线程。
        /// Queues an asynchronous callback to the UI thread.
        /// </summary>
        /// <param name="dispatcher">调度器 / Dispatcher</param>
        /// <param name="asyncCallback">异步回调方法 / Async callback method</param>
        /// <param name="priority">优先级 / Priority</param>
        /// <returns>表示操作完成的任务 / Task representing operation completion</returns>
        public static Task EnqueueAsync(this DispatcherQueue dispatcher, Func<Task> asyncCallback, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));
            
            var taskCompletionSource = new TaskCompletionSource<bool>();
            
            if (!dispatcher.TryEnqueue(priority, async () =>
            {
                try
                {
                    await asyncCallback();
                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            }))
            {
                taskCompletionSource.TrySetException(new InvalidOperationException("Failed to enqueue task to dispatcher"));
            }
            
            return taskCompletionSource.Task;
        }
    }
}
