using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using System.Linq;
using System.Collections.Generic;
using Microsoft.UI.Dispatching;
using System.Windows.Input;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media.Animation;

namespace WinVault.Pages
{
    public sealed partial class CommandQueryPage : Page
    {
        #region 依赖属性

        public static readonly DependencyProperty IsCommandRunningProperty = DependencyProperty.Register(
            nameof(IsCommandRunning), typeof(bool), typeof(CommandQueryPage), new PropertyMetadata(default(bool)));

        public bool IsCommandRunning
        {
            get => (bool)GetValue(IsCommandRunningProperty);
            set => SetValue(IsCommandRunningProperty, value);
        }

        public static readonly DependencyProperty CommandOutputProperty = DependencyProperty.Register(
            nameof(CommandOutput), typeof(string), typeof(CommandQueryPage), new PropertyMetadata(default(string)));

        public string CommandOutput
        {
            get => (string)GetValue(CommandOutputProperty);
            set => SetValue(CommandOutputProperty, value);
        }

        public static readonly DependencyProperty HasSelectedCommandProperty = DependencyProperty.Register(
            nameof(HasSelectedCommand), typeof(bool), typeof(CommandQueryPage), new PropertyMetadata(false));

        public bool HasSelectedCommand
        {
            get => (bool)GetValue(HasSelectedCommandProperty);
            set => SetValue(HasSelectedCommandProperty, value);
        }

        public static readonly DependencyProperty SelectedCommandProperty = DependencyProperty.Register(
            nameof(SelectedCommand), typeof(CommandItem), typeof(CommandQueryPage), new PropertyMetadata(null));

        public CommandItem SelectedCommand
        {
            get => (CommandItem)GetValue(SelectedCommandProperty);
            set
            {
                SetValue(SelectedCommandProperty, value);
                HasSelectedCommand = value != null;
            }
        }

        #endregion

        // 命令集合
        public ObservableCollection<CommandItem> Commands { get; private set; } = [];
        
        // 过滤后的命令集合
        public ObservableCollection<CommandItem> FilteredCommands { get; private set; } = [];

        // 当前选择的分类
        private string CurrentCategory = "全部命令";
        
        // 当前搜索文本
        private string CurrentSearchText = string.Empty;

        // 添加防抖动计时器
        private DispatcherTimer? _debounceTimer;
        private const int DebounceDelay = 300; // 毫秒

        public CommandQueryPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled; // 缓存以加速导航
            
            // 初始化状态
            IsCommandRunning = false;
            CommandOutput = string.Empty;

            // 注册页面加载事件
            this.Loaded += CommandQueryPage_Loaded;
        }

        private async void CommandQueryPage_Loaded(object _, RoutedEventArgs __)
        {
            // 异步初始化命令集合
            await Task.Run(() => InitializeCommands());
            
            // 应用初始过滤
            await ApplyFiltersAsync();
            
            // 如果有命令，默认选中第一个
            if (FilteredCommands.Count > 0)
            {
                CommandListView.SelectedIndex = 0;
            }
        }

        #region 命令初始化与过滤

        private void InitializeCommands()
        {
            Commands =
            [
                // 系统类命令
                new() { Name = "查看系统信息", Command = "systeminfo", Description = "显示本地计算机的详细配置信息，包括硬件和操作系统。", Category = "系统" },
                new() { Name = "查看Windows版本", Command = "winver", Description = "显示Windows版本信息。", Category = "系统" },
                new() { Name = "查看系统变量", Command = "set", Description = "显示当前的环境变量。", Category = "系统" },
                new() { Name = "系统文件检查", Command = "sfc /scannow", Description = "扫描所有受保护的系统文件并修复损坏的文件。", Category = "系统" },
                new() { Name = "查看系统启动信息", Command = "bcdedit", Description = "显示或修改启动配置数据。", Category = "系统" },
                new() { Name = "查看系统事件日志", Command = "eventvwr", Description = "打开事件查看器，查看系统、应用和安全日志。", Category = "系统" },
                new() { Name = "打开注册表编辑器", Command = "regedit", Description = "打开注册表编辑器。", Category = "系统" },
                new() { Name = "打开本地组策略", Command = "gpedit.msc", Description = "打开本地组策略编辑器。", Category = "系统" },
                new() { Name = "启动计算机管理", Command = "compmgmt.msc", Description = "打开计算机管理控制台。", Category = "系统" },
                new() { Name = "打开控制面板", Command = "control", Description = "打开控制面板。", Category = "系统" },
                new() { Name = "打开任务管理器", Command = "taskmgr", Description = "打开任务管理器。", Category = "系统" },
                
                // 网络类命令
                new() { Name = "查看IP配置", Command = "ipconfig /all", Description = "显示所有网络适配器的完整TCP/IP配置信息。", Category = "网络" },
                new() { Name = "刷新DNS缓存", Command = "ipconfig /flushdns", Description = "清除DNS解析器缓存。", Category = "网络" },
                new() { Name = "查看网络连接", Command = "netstat -ano", Description = "显示所有活动的TCP连接、端口监听、以太网统计信息、IP路由表和IPv4/v6统计信息。", Category = "网络" },
                new() { Name = "查看MAC地址", Command = "getmac /v", Description = "显示所有网络适配器的MAC地址。", Category = "网络" },
                new() { Name = "网络连接状态", Command = "netsh interface show interface", Description = "显示网络接口的状态。", Category = "网络" },
                new() { Name = "网络连接修复", Command = "netsh winsock reset", Description = "重置Winsock目录，解决网络连接问题。", Category = "网络" },
                new() { Name = "路由表信息", Command = "route print", Description = "显示路由表信息。", Category = "网络" },
                new() { Name = "Ping测试", Command = "ping www.baidu.com", Description = "测试与指定主机的连接。", Category = "网络" },
                new() { Name = "查看主机名", Command = "hostname", Description = "显示当前计算机的主机名。", Category = "网络" },
                new() { Name = "Tracert路径跟踪", Command = "tracert www.baidu.com", Description = "跟踪数据包到达目标主机的路径。", Category = "网络" },
                new() { Name = "查看无线网络", Command = "netsh wlan show networks", Description = "显示可用的无线网络。", Category = "网络" },
                
                // 磁盘类命令
                new() { Name = "查看磁盘信息", Command = "wmic logicaldisk get caption,size,freespace", Description = "使用WMIC命令获取逻辑磁盘的盘符、总大小和可用空间。", Category = "磁盘" },
                new() { Name = "磁盘错误检查", Command = "chkdsk C: /f", Description = "检查磁盘C:并修复错误。", Category = "磁盘" },
                new() { Name = "磁盘清理", Command = "cleanmgr", Description = "打开磁盘清理工具。", Category = "磁盘" },
                new() { Name = "磁盘碎片整理", Command = "dfrgui", Description = "打开磁盘碎片整理工具。", Category = "磁盘" },
                new() { Name = "磁盘管理", Command = "diskmgmt.msc", Description = "打开磁盘管理工具。", Category = "磁盘" },
                new() { Name = "列出目录内容", Command = "dir", Description = "显示当前目录中的文件和子目录。", Category = "磁盘" },
                new() { Name = "查看分区信息", Command = "diskpart", Description = "启动磁盘分区工具。", Category = "磁盘" },
                new() { Name = "查找大文件", Command = "dir C:\\ /s /a /o-s | findstr \"File(s)\"", Description = "按大小顺序列出目录下的文件。", Category = "磁盘" },
                new() { Name = "文件系统信息", Command = "fsutil fsinfo volumeinfo C:", Description = "显示C盘的文件系统信息。", Category = "磁盘" },
                
                // 安全类命令
                new() { Name = "查看防火墙状态", Command = "netsh advfirewall show allprofiles", Description = "显示Windows防火墙的所有配置文件的状态。", Category = "安全" },
                new() { Name = "启用防火墙", Command = "netsh advfirewall set allprofiles state on", Description = "启用所有防火墙配置文件。", Category = "安全" },
                new() { Name = "禁用防火墙", Command = "netsh advfirewall set allprofiles state off", Description = "禁用所有防火墙配置文件。", Category = "安全" },
                new() { Name = "打开防火墙高级设置", Command = "wf.msc", Description = "打开高级防火墙设置。", Category = "安全" },
                new() { Name = "打开证书管理", Command = "certmgr.msc", Description = "打开证书管理器。", Category = "安全" },
                new() { Name = "查看访问权限", Command = "icacls C:\\", Description = "显示文件或目录的访问控制列表。", Category = "安全" },
                new() { Name = "系统安全策略", Command = "secpol.msc", Description = "打开本地安全策略。", Category = "安全" },
                new() { Name = "Windows安全中心", Command = "wscui.cpl", Description = "打开Windows安全中心。", Category = "安全" },
                new() { Name = "查看防火墙规则", Command = "netsh advfirewall firewall show rule name=all", Description = "显示所有防火墙规则。", Category = "安全" },
                
                // 服务类命令
                new() { Name = "查看服务状态", Command = "sc query type= service state= all", Description = "查询所有服务的状态。", Category = "服务" },
                new() { Name = "打开服务管理", Command = "services.msc", Description = "打开服务管理控制台。", Category = "服务" },
                new() { Name = "启动服务", Command = "sc start 服务名", Description = "启动指定的服务（替换'服务名'为实际服务名）。", Category = "服务" },
                new() { Name = "停止服务", Command = "sc stop 服务名", Description = "停止指定的服务（替换'服务名'为实际服务名）。", Category = "服务" },
                new() { Name = "查看Windows更新服务", Command = "sc query wuauserv", Description = "查询Windows Update服务的状态。", Category = "服务" },
                new() { Name = "查看计划任务", Command = "schtasks /query /fo LIST /v", Description = "显示所有计划任务的详细信息。", Category = "服务" },
                new() { Name = "打开计划任务", Command = "taskschd.msc", Description = "打开任务计划程序。", Category = "服务" },
                new() { Name = "服务依赖关系", Command = "sc qc 服务名", Description = "显示指定服务的配置信息。", Category = "服务" },

                // 驱动类命令
                new() { Name = "查看驱动器信息", Command = "driverquery", Description = "显示所有已安装的设备驱动程序及其属性。", Category = "驱动" },
                new() { Name = "打开设备管理器", Command = "devmgmt.msc", Description = "打开设备管理器。", Category = "驱动" },
                new() { Name = "显示PnP设备", Command = "pnputil /enum-devices", Description = "枚举所有即插即用设备。", Category = "驱动" },
                new() { Name = "驱动程序详细信息", Command = "driverquery /v", Description = "显示驱动程序的详细信息。", Category = "驱动" },
                new() { Name = "驱动程序状态", Command = "driverquery /si", Description = "显示已签名的驱动程序。", Category = "驱动" },
                new() { Name = "查看USB设备", Command = "devmgr_show_nonpresent_devices=1 && devmgmt.msc", Description = "显示所有USB设备，包括不存在的设备。", Category = "驱动" },

                // 硬件类命令
                new() { Name = "查看CPU信息", Command = "wmic cpu get name,MaxClockSpeed,CurrentClockSpeed,NumberOfCores,NumberOfLogicalProcessors", Description = "显示CPU的详细信息。", Category = "硬件" },
                new() { Name = "查看内存信息", Command = "wmic memorychip get Capacity,Speed,DeviceLocator,PartNumber", Description = "显示内存的详细信息。", Category = "硬件" },
                new() { Name = "查看主板信息", Command = "wmic baseboard get Manufacturer,Product,SerialNumber,Version", Description = "显示主板的详细信息。", Category = "硬件" },
                new() { Name = "查看显卡信息", Command = "wmic path win32_VideoController get Name,AdapterRAM,DriverVersion", Description = "显示显卡的详细信息。", Category = "硬件" },
                new() { Name = "查看BIOS信息", Command = "wmic bios get Manufacturer,Name,SerialNumber,Version", Description = "显示BIOS的详细信息。", Category = "硬件" },
                new() { Name = "查看音频设备", Command = "wmic sounddev get Caption,DeviceID,Status", Description = "显示音频设备的信息。", Category = "硬件" },
                new() { Name = "打开DirectX诊断工具", Command = "dxdiag", Description = "打开DirectX诊断工具。", Category = "硬件" },
                new() { Name = "查看显示器信息", Command = "wmic desktopmonitor get ScreenHeight,ScreenWidth,Caption", Description = "显示显示器的分辨率和其他信息。", Category = "硬件" },

                // 日志类命令
                new() { Name = "查看系统日志", Command = "wevtutil qe System /c:5 /f:text", Description = "显示最近5条系统日志。", Category = "日志" },
                new() { Name = "查看应用程序日志", Command = "wevtutil qe Application /c:5 /f:text", Description = "显示最近5条应用程序日志。", Category = "日志" },
                new() { Name = "查看安全日志", Command = "wevtutil qe Security /c:5 /f:text", Description = "显示最近5条安全日志。", Category = "日志" },
                new() { Name = "清除系统日志", Command = "wevtutil cl System", Description = "清除系统日志。", Category = "日志" },
                new() { Name = "查看日志信息", Command = "wevtutil gli System", Description = "获取系统日志信息。", Category = "日志" },
                new() { Name = "打开事件查看器", Command = "eventvwr.msc", Description = "打开事件查看器。", Category = "日志" },
                new() { Name = "查看性能日志", Command = "perfmon /rel", Description = "打开可靠性监视器。", Category = "日志" },

                // 用户类命令
                new() { Name = "查看用户账户", Command = "net user", Description = "显示计算机上的用户账户列表。", Category = "用户" },
                new() { Name = "查看当前用户", Command = "whoami", Description = "显示当前登录的用户名。", Category = "用户" },
                new() { Name = "查看本地组", Command = "net localgroup", Description = "显示本地组列表。", Category = "用户" },
                new() { Name = "查看管理员组", Command = "net localgroup administrators", Description = "显示管理员组的成员。", Category = "用户" },
                new() { Name = "添加用户", Command = "net user 用户名 密码 /add", Description = "添加新用户（替换'用户名'和'密码'）。", Category = "用户" },
                new() { Name = "删除用户", Command = "net user 用户名 /delete", Description = "删除用户（替换'用户名'）。", Category = "用户" },
                new() { Name = "打开用户账户管理", Command = "lusrmgr.msc", Description = "打开本地用户和组管理器。", Category = "用户" },
                new() { Name = "查看用户权限", Command = "whoami /priv", Description = "显示当前用户的权限。", Category = "用户" },
                new() { Name = "查看用户组", Command = "whoami /groups", Description = "显示当前用户所属的组。", Category = "用户" },

                // 文件类命令
                new() { Name = "文件比较工具", Command = "fc file1.txt file2.txt", Description = "比较两个文件的内容（替换为实际文件名）。", Category = "文件" },
                new() { Name = "文本查找", Command = "findstr \"关键词\" *.txt", Description = "在文本文件中查找关键词。", Category = "文件" },
                new() { Name = "文件属性修改", Command = "attrib +r file.txt", Description = "将文件设为只读（替换为实际文件名）。", Category = "文件" },
                new() { Name = "复制文件", Command = "xcopy C:\\source D:\\destination /e /i /h", Description = "复制文件和目录，包括隐藏文件。", Category = "文件" },
                new() { Name = "移动文件", Command = "move C:\\source\\file.txt D:\\destination\\", Description = "移动文件到新位置。", Category = "文件" },
                new() { Name = "删除文件", Command = "del file.txt", Description = "删除指定文件。", Category = "文件" },
                new() { Name = "创建目录", Command = "mkdir newdir", Description = "创建新目录。", Category = "文件" },
                new() { Name = "重命名文件", Command = "ren oldname.txt newname.txt", Description = "重命名文件。", Category = "文件" },
                new() { Name = "查找大文件", Command = "forfiles /s /m *.* /c \"cmd /c if @fsize GTR 100000000 echo @path @fsize bytes\"", Description = "查找大于100MB的文件。", Category = "文件" },
                new() { Name = "共享文件夹", Command = "net share", Description = "显示共享资源信息。", Category = "文件" },

                // 其他类命令
                new() { Name = "查看进程列表", Command = "tasklist", Description = "显示当前运行的进程。", Category = "其他" },
                new() { Name = "结束进程", Command = "taskkill /im 进程名.exe", Description = "结束指定进程（替换'进程名.exe'）。", Category = "其他" },
                new() { Name = "时间同步", Command = "w32tm /resync", Description = "强制时间同步。", Category = "其他" },
                new() { Name = "查看系统字体", Command = "fc-list", Description = "列出系统中安装的字体。", Category = "其他" },
                new() { Name = "启动远程桌面", Command = "mstsc", Description = "打开远程桌面连接。", Category = "其他" },
                new() { Name = "系统休眠", Command = "shutdown /h", Description = "使系统进入休眠状态。", Category = "其他" },
                new() { Name = "系统重启", Command = "shutdown /r", Description = "重启系统。", Category = "其他" },
                new() { Name = "系统关机", Command = "shutdown /s", Description = "关闭系统。", Category = "其他" },
                new() { Name = "查看系统时间", Command = "time /t", Description = "显示系统时间。", Category = "其他" },
                new() { Name = "查看系统日期", Command = "date /t", Description = "显示系统日期。", Category = "其他" },
                new() { Name = "打开计算器", Command = "calc", Description = "打开计算器应用程序。", Category = "其他" },
                new() { Name = "打开记事本", Command = "notepad", Description = "打开记事本应用程序。", Category = "其他" },
                
                // 高级命令
                new() { Name = "DISM修复Windows映像", Command = "DISM /Online /Cleanup-Image /RestoreHealth", Description = "使用DISM工具修复Windows映像。", Category = "系统" },
                new() { Name = "PowerShell查看版本", Command = "powershell $PSVersionTable", Description = "显示PowerShell版本信息。", Category = "系统" },
                new() { Name = "快速启动开关", Command = "powercfg /hibernate off", Description = "关闭快速启动功能（需要管理员权限）。", Category = "系统" },
                new() { Name = "电源计划信息", Command = "powercfg /list", Description = "列出所有电源计划。", Category = "系统" },
                new() { Name = "优化SSD性能", Command = "fsutil behavior set DisableDeleteNotify 0", Description = "启用TRIM功能，优化SSD性能。", Category = "磁盘" },
                new() { Name = "网络连接修复", Command = "netsh int ip reset", Description = "重置TCP/IP协议栈。", Category = "网络" },
                new() { Name = "网络堆栈重置", Command = "netsh int reset all", Description = "重置所有网络适配器和协议。", Category = "网络" },
                new() { Name = "DNS查询工具", Command = "nslookup www.baidu.com", Description = "查询域名的DNS信息。", Category = "网络" },
                new() { Name = "Wi-Fi信息导出", Command = "netsh wlan export profile key=clear", Description = "导出所有Wi-Fi配置文件，包括密码。", Category = "网络" },
                new() { Name = "RDP端口查询", Command = "reg query \"HKLM\\System\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp\" /v PortNumber", Description = "查询远程桌面服务使用的端口。", Category = "网络" },
                new() { Name = "检查电脑开机时间", Command = "systeminfo | find \"Boot Time\"", Description = "显示系统启动时间。", Category = "系统" },
                new() { Name = "查看Win32服务", Command = "sc queryex type= service", Description = "查询所有Win32服务的详细信息。", Category = "服务" },
                new() { Name = "查看USB设备历史", Command = "reg query HKLM\\SYSTEM\\CurrentControlSet\\Enum\\USBSTOR", Description = "显示曾经连接过的USB存储设备信息。", Category = "硬件" },
                new() { Name = "查看硬盘S.M.A.R.T信息", Command = "wmic diskdrive get status", Description = "显示硬盘的S.M.A.R.T状态。", Category = "硬件" },
                new() { Name = "清除DNS缓存", Command = "ipconfig /flushdns", Description = "清除DNS解析器缓存。", Category = "网络" },
                new() { Name = "PowerShell执行策略", Command = "powershell Get-ExecutionPolicy", Description = "查看PowerShell执行策略设置。", Category = "安全" },
                new() { Name = "查看高级TCP设置", Command = "netsh int tcp show global", Description = "显示全局TCP参数设置。", Category = "网络" },
                new() { Name = "检查硬盘错误", Command = "chkntfs C:", Description = "检查C盘是否需要进行磁盘检查。", Category = "磁盘" },
                new() { Name = "电池健康报告", Command = "powercfg /batteryreport", Description = "生成电池健康和使用情况报告。", Category = "硬件" },
                new() { Name = "系统文件完整性检查", Command = "sfc /verifyonly", Description = "验证系统文件完整性但不修复。", Category = "系统" },
                new() { Name = "显示详细启动信息", Command = "bcdedit /enum", Description = "显示详细的启动配置数据。", Category = "系统" },
                new() { Name = "内存诊断工具", Command = "mdsched.exe", Description = "打开Windows内存诊断工具。", Category = "硬件" },
                new() { Name = "打开PowerShell", Command = "powershell", Description = "启动Windows PowerShell。", Category = "系统" },
                new() { Name = "查看所有网络适配器", Command = "wmic nic get name,index", Description = "列出所有网络适配器的名称和索引。", Category = "网络" },
                new() { Name = "清除临时文件", Command = "del /q /f /s %temp%\\*", Description = "删除临时文件夹中的所有文件。", Category = "文件" },
                new() { Name = "查看网络共享", Command = "net share", Description = "显示当前系统的共享资源。", Category = "网络" },
                new() { Name = "查看环境变量", Command = "set", Description = "显示所有环境变量。", Category = "系统" },
                
                // 优化类别
                new() { Name = "清理系统垃圾", Command = "cleanmgr /sageset:1 & cleanmgr /sagerun:1", Description = "设置并运行磁盘清理工具，清除系统垃圾文件。", Category = "优化" },
                new() { Name = "禁用不必要服务", Command = "services.msc", Description = "打开服务管理器，可以禁用不必要的服务来提高性能。", Category = "优化" },
                new() { Name = "禁用启动项", Command = "msconfig", Description = "打开系统配置实用程序，可以禁用不必要的启动项。", Category = "优化" },
                new() { Name = "优化网络设置", Command = "netsh interface tcp set global autotuning=normal", Description = "优化TCP自动调整级别，提高网络性能。", Category = "优化" },
                new() { Name = "优化电源计划", Command = "powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61", Description = "添加Ultimate Performance电源计划（高性能）。", Category = "优化" },
                new() { Name = "清除系统更新缓存", Command = "net stop wuauserv && rd /s /q C:\\Windows\\SoftwareDistribution && net start wuauserv", Description = "停止Windows Update服务，清除更新缓存，然后重启服务。", Category = "优化" },
                new() { Name = "检查并优化SSD", Command = "defrag /C /O /G", Description = "对SSD执行TRIM操作，优化性能。", Category = "优化" },
                new() { Name = "优化游戏性能", Command = "bcdedit /set useplatformclock false", Description = "禁用高精度事件计时器，可能提升游戏性能（需管理员权限）。", Category = "优化" },
                new() { Name = "启用快速启动", Command = "powercfg /h on", Description = "启用系统休眠和快速启动功能。", Category = "优化" },
                new() { Name = "优化虚拟内存", Command = "rundll32.exe sysdm.cpl,EditVirtualMemory", Description = "打开虚拟内存设置对话框，可调整分页文件大小。", Category = "优化" },
                
                // 故障排除类别
                new() { Name = "蓝屏检查工具", Command = "verifier", Description = "打开驱动程序验证程序管理器，可以诊断驱动问题。", Category = "故障排除" },
                new() { Name = "系统扫描修复", Command = "dism /online /cleanup-image /scanhealth", Description = "扫描系统映像是否存在损坏。", Category = "故障排除" },
                new() { Name = "修复系统引导", Command = "bootrec /rebuildbcd", Description = "重建BCD存储，修复启动问题。", Category = "故障排除" },
                new() { Name = "内存泄漏诊断", Command = "perfmon /rel", Description = "打开可靠性监视器，检查系统错误和崩溃历史。", Category = "故障排除" },
                new() { Name = "网络连接重置", Command = "ipconfig /release && ipconfig /renew", Description = "释放并更新IP地址，解决网络连接问题。", Category = "故障排除" },
                new() { Name = "解决打印机问题", Command = "printui /s /t2", Description = "打开打印机疑难解答界面。", Category = "故障排除" },
                new() { Name = "系统文件检查器", Command = "sfc /scannow", Description = "扫描并修复系统文件问题。", Category = "故障排除" },
                new() { Name = "检查磁盘错误", Command = "chkdsk /f /r", Description = "检查并修复磁盘错误，恢复坏扇区（重启后执行）。", Category = "故障排除" },
                new() { Name = "问题步骤记录器", Command = "psr.exe", Description = "启动问题步骤记录器，可以记录屏幕活动和生成报告。", Category = "故障排除" },
                new() { Name = "驱动程序问题检查", Command = "msdt.exe -id DeviceDiagnostic", Description = "打开设备诊断工具，诊断硬件和设备驱动程序问题。", Category = "故障排除" },
                new() { Name = "网络适配器重置", Command = "netcfg -d", Description = "重置所有网络适配器（需要管理员权限）。", Category = "故障排除" },
                new() { Name = "DirectX诊断工具", Command = "dxdiag", Description = "启动DirectX诊断工具，诊断显卡和声卡问题。", Category = "故障排除" },
                new() { Name = "应用商店修复", Command = "wsreset.exe", Description = "重置Microsoft Store缓存，解决应用商店问题。", Category = "故障排除" },
                new() { Name = "组件存储清理", Command = "dism /online /cleanup-image /startcomponentcleanup", Description = "清理组件存储，释放磁盘空间。", Category = "故障排除" }
            ];
        }

        // 异步应用过滤器
        private async Task ApplyFiltersAsync()
        {
            // 在后台线程执行过滤操作
            await Task.Run(() => 
            {
                var filteredCommands = new List<CommandItem>();
                var query = Commands.AsEnumerable();
                
                // 应用分类过滤
                if (CurrentCategory != "全部命令")
                {
                    query = query.Where(c => c.Category == CurrentCategory);
                }
                
                // 应用搜索过滤
                if (!string.IsNullOrEmpty(CurrentSearchText))
                {
                    string searchText = CurrentSearchText.Trim().ToLowerInvariant();
                    
                    // 支持多个搜索词，以空格分隔
                    string[] searchTerms = searchText.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    
                    if (searchTerms.Length > 0)
                    {
                        query = query.Where(c => 
                        {
                            // 检查命令是否匹配所有搜索词（任意属性都可以）
                            return searchTerms.All(term =>
                                c.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) == true ||
                                c.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) == true ||
                                c.Command?.Contains(term, StringComparison.OrdinalIgnoreCase) == true ||
                                c.Category?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
                        });
                    }
                }
                
                // 转换为列表
                filteredCommands = query.ToList();
                
                // 在UI线程更新集合
                App.CurrentWindow?.DispatcherQueue?.TryEnqueue(() => 
                {
                    FilteredCommands.Clear();
                    foreach (var command in filteredCommands)
                    {
                        FilteredCommands.Add(command);
                    }
                });
            });
        }

        // 带防抖动的过滤应用
        private void ApplyFilters()
        {
            // 取消之前的计时器
            _debounceTimer?.Stop();
            
            // 创建新计时器
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DebounceDelay)
            };
            
            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer?.Stop();
                await ApplyFiltersAsync();
            };
            
            _debounceTimer.Start();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs _args)
        {
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                CurrentCategory = selectedItem.Content?.ToString() ?? "全部命令";
                ApplyFilters();
            }
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                CurrentSearchText = sender.Text ?? string.Empty;
                ApplyFilters();
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs _args)
        {
            CurrentSearchText = _args.QueryText ?? string.Empty;
            ApplyFilters();
        }

        private void CommandListView_SelectionChanged(object sender, SelectionChangedEventArgs _args)
        {
            if (CommandListView.SelectedItem is CommandItem selectedCommand)
            {
                SelectedCommand = selectedCommand;
            }
            else
            {
                SelectedCommand = new();
            }
        }

        #endregion

        #region 命令执行

        private async void CommandInputTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs _e)
        {
            if (_e.Key == Windows.System.VirtualKey.Enter)
            {
                await ExecuteCommand(CommandInputTextBox.Text);
            }
        }

        private async void ExecuteCommand_Click(object _, RoutedEventArgs __)
        {
            await ExecuteCommand(CommandInputTextBox.Text);
        }

        private async void RunSelectedCommand_Click(object _, RoutedEventArgs __)
        {
            if (SelectedCommand != null)
            {
                CommandInputTextBox.Text = SelectedCommand.Command ?? string.Empty;
                await ExecuteCommand(SelectedCommand.Command ?? string.Empty);
            }
        }

        private async Task ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            IsCommandRunning = true;
            CommandOutput = string.Empty;
            ExecutionInfoBar.IsOpen = false;

            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {command}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                // 尝试设置 GBK 编码（936），失败则使用系统默认编码
                        try
                        {
                    psi.StandardOutputEncoding = System.Text.Encoding.GetEncoding(936);
                        }
                catch
                        {
                            psi.StandardOutputEncoding = System.Text.Encoding.Default;
                        }

                using var process = Process.Start(psi) ?? throw new InvalidOperationException("进程启动失败");

                // 并行抓取标准输出/错误
                var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
                var errorTask  = process.StandardError.ReadToEndAsync(cts.Token);

                // 等待进程退出（带超时）
                await process.WaitForExitAsync(cts.Token);

                var output = await outputTask;
                var error  = await errorTask;

                UpdateOutputText(output, error);

                            ExecutionInfoBar.Title = "命令执行完成";
                            ExecutionInfoBar.Message = $"命令 '{command}' 已执行完成";
                            ExecutionInfoBar.Severity = InfoBarSeverity.Success;
                            ExecutionInfoBar.IsOpen = true;
            }
            catch (OperationCanceledException)
                {
                    CommandOutput += "\n命令执行超时，已自动终止。";
                    ExecutionInfoBar.Title = "命令执行超时";
                    ExecutionInfoBar.Message = "命令执行时间过长，已自动终止";
                    ExecutionInfoBar.Severity = InfoBarSeverity.Warning;
                    ExecutionInfoBar.IsOpen = true;
            }
            catch (Exception ex)
                {
                    CommandOutput = $"错误: {ex.Message}";
                    ExecutionInfoBar.Title = "命令执行失败";
                    ExecutionInfoBar.Message = ex.Message;
                    ExecutionInfoBar.Severity = InfoBarSeverity.Error;
                    ExecutionInfoBar.IsOpen = true;
            }
            finally
            {
                    IsCommandRunning = false;
            }
        }
        
        // 更新输出文本的辅助方法
        private void UpdateOutputText(string output, string error)
        {
            App.CurrentWindow?.DispatcherQueue?.TryEnqueue(() =>
            {
                CommandOutput = (string.IsNullOrEmpty(output) ? "" : output) + 
                               (string.IsNullOrEmpty(error) ? "" : error);
                
                // 查找命令输出区域的ScrollViewer并滚动到底部
                if (CommandOutputText != null)
                {
                    var parent = VisualTreeHelper.GetParent(CommandOutputText);
                    while (parent != null && !(parent is ScrollViewer))
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                    
                    if (parent is ScrollViewer scrollViewer)
                    {
                        scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null, false);
                    }
                }
            });
        }

        #endregion

        #region UI事件处理

        private void CopyCommand_Click(object _, RoutedEventArgs __)
        {
            if (SelectedCommand != null)
            {
                DataPackage dataPackage = new()
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                
                dataPackage.SetText(SelectedCommand.Command ?? string.Empty);
                Clipboard.SetContent(dataPackage);
                
                ShowCopiedFeedback("命令已复制到剪贴板");
            }
        }

        private void CopyOutput_Click(object _, RoutedEventArgs __)
        {
            if (!string.IsNullOrEmpty(CommandOutput))
            {
                DataPackage dataPackage = new()
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                
                dataPackage.SetText(CommandOutput);
                Clipboard.SetContent(dataPackage);
                
                ShowCopiedFeedback("输出已复制到剪贴板");
            }
        }

        private void ClearOutput_Click(object _, RoutedEventArgs __)
        {
            CommandOutput = string.Empty;
        }
        
        private void ShowCopiedFeedback(string message)
            {
            CopiedTip.Subtitle = message;
            CopiedTip.IsOpen = true;
        }

        #endregion
    }

    public class CommandItem
    {
        public string? Name { get; set; }
        public string? Command { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
    }
}