using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WinVault
{
    /// <summary>
    /// 启动诊断工具类
    /// 用于诊断和记录应用程序启动过程中的问题
    /// </summary>
    public static class StartupDiagnostics
    {
        private static readonly string LogFilePath = "startup_diagnostics.log";
        private static readonly object LogLock = new object();

        /// <summary>
        /// 记录启动诊断信息
        /// </summary>
        /// <param name="message">诊断消息</param>
        public static void Log(string message)
        {
            try
            {
                lock (LogLock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogFilePath, logEntry, Encoding.UTF8);
                }
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        /// <summary>
        /// 记录异常信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="context">异常上下文</param>
        public static void LogException(Exception ex, string context = "")
        {
            try
            {
                var message = $"异常信息 / Exception Info:";
                if (!string.IsNullOrEmpty(context))
                {
                    message += $" 上下文 / Context: {context}";
                }
                message += $"{Environment.NewLine}类型 / Type: {ex.GetType().Name}";
                message += $"{Environment.NewLine}消息 / Message: {ex.Message}";
                message += $"{Environment.NewLine}堆栈 / Stack Trace: {ex.StackTrace}";
                
                if (ex.InnerException != null)
                {
                    message += $"{Environment.NewLine}内部异常 / Inner Exception: {ex.InnerException.Message}";
                }
                
                Log(message);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        /// <summary>
        /// 记录系统环境信息
        /// </summary>
        public static void LogSystemInfo()
        {
            try
            {
                var info = new StringBuilder();
                info.AppendLine("=== 系统环境信息 / System Environment Info ===");
                info.AppendLine($"操作系统 / OS: {Environment.OSVersion}");
                info.AppendLine($"运行时版本 / Runtime Version: {Environment.Version}");
                info.AppendLine($"工作目录 / Working Directory: {Environment.CurrentDirectory}");
                info.AppendLine($"用户域 / User Domain: {Environment.UserDomainName}");
                info.AppendLine($"用户名 / User Name: {Environment.UserName}");
                info.AppendLine($"处理器数量 / Processor Count: {Environment.ProcessorCount}");
                info.AppendLine($"系统页面大小 / System Page Size: {Environment.SystemPageSize}");
                info.AppendLine($"是否64位进程 / Is 64-bit Process: {Environment.Is64BitProcess}");
                info.AppendLine($"是否64位操作系统 / Is 64-bit OS: {Environment.Is64BitOperatingSystem}");
                info.AppendLine($"CLR版本 / CLR Version: {Environment.Version}");
                
                // 记录环境变量
                info.AppendLine("=== 关键环境变量 / Key Environment Variables ===");
                var keyVars = new[] { "PATH", "TEMP", "TMP", "USERPROFILE", "APPDATA", "LOCALAPPDATA" };
                foreach (var var in keyVars)
                {
                    var value = Environment.GetEnvironmentVariable(var);
                    info.AppendLine($"{var}: {value}");
                }
                
                Log(info.ToString());
            }
            catch (Exception ex)
            {
                LogException(ex, "记录系统信息时出错");
            }
        }

        /// <summary>
        /// 记录应用程序信息
        /// </summary>
        public static void LogAppInfo()
        {
            try
            {
                var info = new StringBuilder();
                info.AppendLine("=== 应用程序信息 / Application Info ===");
                info.AppendLine($"应用程序基目录 / App Base: {AppContext.BaseDirectory}");
                info.AppendLine($"目标框架 / Target Framework: {AppContext.TargetFrameworkName}");
                info.AppendLine($"应用程序名称 / App Name: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}");
                info.AppendLine($"应用程序版本 / App Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
                
                Log(info.ToString());
            }
            catch (Exception ex)
            {
                LogException(ex, "记录应用程序信息时出错");
            }
        }

        /// <summary>
        /// 检查关键文件是否存在
        /// </summary>
        public static void CheckCriticalFiles()
        {
            try
            {
                var info = new StringBuilder();
                info.AppendLine("=== 关键文件检查 / Critical Files Check ===");
                
                var criticalFiles = new[]
                {
                    "WinVault.exe",
                    "WinVault.dll",
                    "Microsoft.WindowsAppSDK.dll",
                    "Microsoft.WinUI.dll"
                };
                
                foreach (var file in criticalFiles)
                {
                    var exists = File.Exists(file);
                    info.AppendLine($"{file}: {(exists ? "存在 / Exists" : "缺失 / Missing")}");
                }
                
                Log(info.ToString());
            }
            catch (Exception ex)
            {
                LogException(ex, "检查关键文件时出错");
            }
        }

        /// <summary>
        /// 记录内存信息
        /// </summary>
        public static void LogMemoryInfo()
        {
            try
            {
                var info = new StringBuilder();
                info.AppendLine("=== 内存信息 / Memory Info ===");
                
                var gcInfo = GC.GetGCMemoryInfo();
                info.AppendLine($"总内存 / Total Memory: {GC.GetTotalMemory(false):N0} bytes");
                info.AppendLine($"堆内存 / Heap Memory: {gcInfo.HeapSizeBytes:N0} bytes");
                info.AppendLine($"总堆内存 / Total Heap Memory: {gcInfo.TotalCommittedBytes:N0} bytes");
                
                Log(info.ToString());
            }
            catch (Exception ex)
            {
                LogException(ex, "记录内存信息时出错");
            }
        }

        /// <summary>
        /// 执行完整的启动诊断
        /// </summary>
        public static void RunFullDiagnostics()
        {
            try
            {
                Log("=== 开始启动诊断 / Starting Startup Diagnostics ===");
                LogSystemInfo();
                LogAppInfo();
                CheckCriticalFiles();
                LogMemoryInfo();
                Log("=== 启动诊断完成 / Startup Diagnostics Completed ===");
            }
            catch (Exception ex)
            {
                LogException(ex, "执行完整诊断时出错");
            }
        }
    }
}
