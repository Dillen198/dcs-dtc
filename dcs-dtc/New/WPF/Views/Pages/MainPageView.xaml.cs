using DTC.New.WPF.ViewModels.Pages;
using System.Windows;

namespace DTC.New.WPF.Views.Pages;

public partial class MainPageView : System.Windows.Controls.UserControl
{
    public MainPageView()
    {
        InitializeComponent();
    }

    private void Aircraft_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is string aircraftId && DataContext is MainPageViewModel vm)
            vm.NavigateTo(aircraftId);
    }
}
