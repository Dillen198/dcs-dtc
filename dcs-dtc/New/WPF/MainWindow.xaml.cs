using DTC.New.WPF.ViewModels;
using DTC.Utilities;
using System.Windows;
using System.Windows.Input;
using WpfApplication = System.Windows.Application;

namespace DTC.New.WPF;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        // Restore position
        var x = Settings.MainWindowX;
        var y = Settings.MainWindowY;
        if (x > 0 || y > 0)
        {
            Left = x;
            Top = y;
        }

        LocationChanged += (_, _) =>
        {
            Settings.MainWindowX = (int)Left;
            Settings.MainWindowY = (int)Top;
        };

        vm.ShowHideRequested += OnShowHideRequested;
        vm.PresetSelectedFromDCS += OnPresetSelectedFromDCS;

        Loaded += async (_, _) =>
        {
            // Run DCS install check (mirrors WinForms MainForm_Load)
            await System.Threading.Tasks.Task.Run(() =>
            {
                if (!DCSInstallCheck.Check())
                    Dispatcher.Invoke(() => WpfApplication.Current.Shutdown());
            });
        };

        UpdatePinIcon();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsAlwaysOnTop))
                UpdatePinIcon();
        };
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) return; // ignore double-click
        DragMove();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnPin_Click(object sender, RoutedEventArgs e)
    {
        _vm.ToggleAlwaysOnTopCommand.Execute(null);
        Topmost = _vm.IsAlwaysOnTop;
        UpdatePinIcon();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void UpdatePinIcon()
    {
        Topmost = _vm.IsAlwaysOnTop;
        pinIcon.Text = _vm.IsAlwaysOnTop ? "📌" : "📍";
        pinIcon.Opacity = _vm.IsAlwaysOnTop ? 1.0 : 0.5;
    }

    private void OnShowHideRequested(bool show)
    {
        if (show)
        {
            Show();
            Activate();
            BringIntoView();
        }
        else
        {
            WindowState = WindowState.Minimized;
        }
    }

    private void OnPresetSelectedFromDCS(string presetName, string aircraftType)
    {
        var presetsVm = _vm.NavigateTo(aircraftType);
        presetsVm.ShowPreset(presetName);
    }
}
