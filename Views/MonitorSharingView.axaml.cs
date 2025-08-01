using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Layout;
using Avalonia;
using screenshareav.Services;

namespace ProgrammersToolKit.Views
{
    public partial class MonitorSharingView : UserControl
    {
        private readonly List<MonitorInfo> _monitors;
        private readonly List<CheckBox> _monitorCheckBoxes = new();

        public MonitorSharingView()
        {
            AvaloniaXamlLoader.Load(this);
            _monitors = MonitorService.GetMonitors();
            var monitorList = this.FindControl<ItemsControl>("MonitorList");
            if (monitorList != null)
                monitorList.ItemsSource = _monitors.Select(m => CreateMonitorCheckBox(m)).ToList();

            var openTabsBtn = this.FindControl<Button>("OpenTabsBtn");
            if (openTabsBtn != null)
                openTabsBtn.Click += OpenTabsBtn_Click;
            var sideBySideBtn = this.FindControl<Button>("SideBySideBtn");
            if (sideBySideBtn != null)
                sideBySideBtn.Click += SideBySideBtn_Click;
            var mergeBtn = this.FindControl<Button>("MergeBtn");
            if (mergeBtn != null)
                mergeBtn.Click += MergeBtn_Click;
        }

        private CheckBox CreateMonitorCheckBox(MonitorInfo monitor)
        {
            var cb = new CheckBox { Content = $"{monitor.Name} ({monitor.Width}x{monitor.Height})", Tag = monitor };
            _monitorCheckBoxes.Add(cb);
            return cb;
        }

        private List<MonitorInfo> GetSelectedMonitors()
        {
            return _monitorCheckBoxes.Where(cb => cb.IsChecked == true && cb.Tag is MonitorInfo).Select(cb => (MonitorInfo)cb.Tag!).ToList();
        }

        private void OpenTabsBtn_Click(object? sender, RoutedEventArgs e)
        {
            var selected = GetSelectedMonitors();
            if (selected.Count == 0) return;
            var tabHost = new TabbedMonitorHost();
            var tabControl = tabHost.FindControl<TabControl>("MonitorTabs");
            if (tabControl == null) return;
            var tabs = new List<TabItem>();
            foreach (var monitor in selected)
            {
                var tab = new TabItem { Header = monitor.Name };
                tab.Content = new MonitorWindow { Title = monitor.Name };
                tabs.Add(tab);
            }
            tabControl.ItemsSource = tabs;
            tabHost.Show();
        }

        private void SideBySideBtn_Click(object? sender, RoutedEventArgs e)
        {
            var selected = GetSelectedMonitors();
            if (selected.Count == 0) return;
            foreach (var monitor in selected)
            {
                var win = new MonitorWindow { Title = monitor.Name };
                win.Show();
            }
        }

        private void MergeBtn_Click(object? sender, RoutedEventArgs e)
        {
            var selected = GetSelectedMonitors();
            if (selected.Count == 0) return;
            var win = new Window { Title = "Merged Monitors", Width = 1200, Height = 800 };
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (var monitor in selected)
            {
                panel.Children.Add(new Border
                {
                    Child = new TextBlock { Text = monitor.Name, Margin = new Thickness(16) },
                    BorderBrush = Avalonia.Media.Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(8)
                });
            }
            win.Content = panel;
            win.Show();
        }
    }
}
