using System.Windows;

namespace DTC.New.WPF;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var vm = new ViewModels.MainViewModel();
        var window = new MainWindow(vm);
        MainWindow = window;
        window.Show();
    }
}
