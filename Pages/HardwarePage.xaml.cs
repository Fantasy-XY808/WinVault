using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WinVault.Pages
{
    public sealed partial class HardwarePage : Page
    {
        // 硬件信息集合
        private ObservableCollection<HardwareInfo> HardwareItems { get; } = new();
        
        public HardwarePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            
            // 在UI初始化后加载硬件信息
            Loaded += HardwarePage_Loaded;
            
            // 监听分类选择变化
            HardwareCategoriesListView.SelectionChanged += HardwareCategoriesListView_SelectionChanged;
        }

        private void HardwarePage_Loaded(object sender, RoutedEventArgs e)
        {
            // 默认加载处理器信息
            LoadHardwareInfo("CPU");
        }

        private void HardwareCategoriesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HardwareCategoriesListView.SelectedItem is ListViewItem selectedItem)
            {
                string category = ((TextBlock)((StackPanel)selectedItem.Content).Children[1]).Text;
                HardwareCategoryTitle.Text = $"{category}信息";
                
                string categoryType = category switch
                {
                    "处理器" => "CPU",
                    "内存" => "Memory",
                    "磁盘" => "Disk",
                    "显卡" => "GPU",
                    "网络适配器" => "Network",
                    "电源信息" => "Power",
                    _ => "CPU"
                };
                
                LoadHardwareInfo(categoryType);
            }
        }
        
        private async void LoadHardwareInfo(string category)
        {
            // 清空当前列表并显示加载指示器
            HardwareItems.Clear();
            HardwareInfoListView.ItemsSource = null;
            
            // 模拟加载过程
            await Task.Delay(500); // 在实际应用中，这里会是真正的硬件信息获取
            
            // 添加模拟数据
            switch (category)
            {
                case "CPU":
                    AddCpuInfo();
                    break;
                case "Memory":
                    AddMemoryInfo();
                    break;
                case "Disk":
                    AddDiskInfo();
                    break;
                case "GPU":
                    AddGpuInfo();
                    break;
                case "Network":
                    AddNetworkInfo();
                    break;
                case "Power":
                    AddPowerInfo();
                    break;
            }
            
            // 绑定数据
            HardwareInfoListView.ItemsSource = HardwareItems;
        }
        
        // 模拟数据方法
        private void AddCpuInfo()
        {
            var cpu = new HardwareInfo { Name = "Intel Core i7-10700K" };
            cpu.Properties.Add(new KeyValuePair<string, string>("核心数", "8"));
            cpu.Properties.Add(new KeyValuePair<string, string>("线程数", "16"));
            cpu.Properties.Add(new KeyValuePair<string, string>("基础频率", "3.80 GHz"));
            cpu.Properties.Add(new KeyValuePair<string, string>("最大频率", "5.10 GHz"));
            cpu.Properties.Add(new KeyValuePair<string, string>("缓存", "16 MB Intel Smart Cache"));
            HardwareItems.Add(cpu);
        }
        
        private void AddMemoryInfo()
        {
            var memory = new HardwareInfo { Name = "系统内存" };
            memory.Properties.Add(new KeyValuePair<string, string>("总容量", "32 GB"));
            memory.Properties.Add(new KeyValuePair<string, string>("类型", "DDR4"));
            memory.Properties.Add(new KeyValuePair<string, string>("频率", "3200 MHz"));
            memory.Properties.Add(new KeyValuePair<string, string>("插槽数", "4"));
            memory.Properties.Add(new KeyValuePair<string, string>("已使用插槽", "2"));
            HardwareItems.Add(memory);
        }
        
        private void AddDiskInfo()
        {
            var disk1 = new HardwareInfo { Name = "Samsung SSD 970 EVO Plus 1TB" };
            disk1.Properties.Add(new KeyValuePair<string, string>("类型", "SSD (NVMe)"));
            disk1.Properties.Add(new KeyValuePair<string, string>("容量", "1000 GB"));
            disk1.Properties.Add(new KeyValuePair<string, string>("可用空间", "650 GB"));
            disk1.Properties.Add(new KeyValuePair<string, string>("接口", "PCIe Gen3 x4"));
            HardwareItems.Add(disk1);
            
            var disk2 = new HardwareInfo { Name = "Western Digital WD Blue 2TB" };
            disk2.Properties.Add(new KeyValuePair<string, string>("类型", "HDD"));
            disk2.Properties.Add(new KeyValuePair<string, string>("容量", "2000 GB"));
            disk2.Properties.Add(new KeyValuePair<string, string>("可用空间", "1200 GB"));
            disk2.Properties.Add(new KeyValuePair<string, string>("接口", "SATA 6Gb/s"));
            HardwareItems.Add(disk2);
        }
        
        private void AddGpuInfo()
        {
            var gpu = new HardwareInfo { Name = "NVIDIA GeForce RTX 3070" };
            gpu.Properties.Add(new KeyValuePair<string, string>("显存", "8 GB GDDR6"));
            gpu.Properties.Add(new KeyValuePair<string, string>("CUDA核心", "5888"));
            gpu.Properties.Add(new KeyValuePair<string, string>("显存频率", "14 Gbps"));
            gpu.Properties.Add(new KeyValuePair<string, string>("接口", "PCIe 4.0"));
            gpu.Properties.Add(new KeyValuePair<string, string>("驱动程序版本", "512.95"));
            HardwareItems.Add(gpu);
        }
        
        private void AddNetworkInfo()
        {
            var network = new HardwareInfo { Name = "Intel Wi-Fi 6 AX201" };
            network.Properties.Add(new KeyValuePair<string, string>("类型", "无线网络适配器"));
            network.Properties.Add(new KeyValuePair<string, string>("MAC地址", "AA:BB:CC:DD:EE:FF"));
            network.Properties.Add(new KeyValuePair<string, string>("IP地址", "192.168.1.100"));
            network.Properties.Add(new KeyValuePair<string, string>("子网掩码", "255.255.255.0"));
            network.Properties.Add(new KeyValuePair<string, string>("默认网关", "192.168.1.1"));
            HardwareItems.Add(network);
        }
        
        private void AddPowerInfo()
        {
            var power = new HardwareInfo { Name = "电源状态" };
            power.Properties.Add(new KeyValuePair<string, string>("电源状态", "接通电源"));
            power.Properties.Add(new KeyValuePair<string, string>("电池状态", "正在充电"));
            power.Properties.Add(new KeyValuePair<string, string>("剩余电量", "85%"));
            power.Properties.Add(new KeyValuePair<string, string>("剩余时间", "2小时30分钟"));
            power.Properties.Add(new KeyValuePair<string, string>("电源计划", "平衡"));
            HardwareItems.Add(power);
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
    }
    
    // 硬件信息数据模型
    public class HardwareInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<KeyValuePair<string, string>> Properties { get; } = new();
    }
} 