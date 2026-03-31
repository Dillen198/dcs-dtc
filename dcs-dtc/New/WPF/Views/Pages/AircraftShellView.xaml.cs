using DTC.New.WPF.ViewModels.Pages;
using System.Windows;
using System.Windows.Forms.Integration;

namespace DTC.New.WPF.Views.Pages;

public partial class AircraftShellView : System.Windows.Controls.UserControl
{
    private WindowsFormsHost? _host;
    private AircraftViewModel? _vm;

    public AircraftShellView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_host != null)
        {
            PART_ContentHost.Content = null;
            _host.Dispose();
            _host = null;
        }

        _vm = e.NewValue as AircraftViewModel;
        if (_vm == null) return;

        // Embed the WinForms content panel in WPF via WindowsFormsHost
        _host = new WindowsFormsHost
        {
            Child = _vm.WinFormsPage.PnlMain
        };
        PART_ContentHost.Content = _host;

        // Show first tab's system page
        if (_vm.SelectedTab?.SystemPage != null)
            _vm.WinFormsPage.ShowSystemPage(_vm.SelectedTab.SystemPage);

        _vm.PropertyChanged += (_, pce) =>
        {
            if (pce.PropertyName == nameof(AircraftViewModel.SelectedTab) && _vm.SelectedTab?.SystemPage != null)
                Dispatcher.Invoke(() => _vm.WinFormsPage.ShowSystemPage(_vm.SelectedTab.SystemPage));
        };
    }

    private void SystemTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Primitives.ToggleButton tb && tb.Tag is SystemTabItem tab)
        {
            if (_vm == null) return;
            foreach (var t in _vm.Tabs)
                t.IsSelected = t == tab;
            _vm.SelectedTab = tab;
        }
    }
}
