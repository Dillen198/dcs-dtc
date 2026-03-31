using DTC.New.WPF.ViewModels.Base;

namespace DTC.New.WPF.ViewModels.Pages;

public class MainPageViewModel : ViewModelBase, INavigationTarget
{
    private readonly MainViewModel _main;

    public string PageTitle => "Home";

    public MainPageViewModel(MainViewModel main)
    {
        _main = main;
    }

    public PresetsViewModel NavigateTo(string aircraftId)
    {
        var aircraft = New.Presets.V2.Base.AircraftRepository.GetAircraft(aircraftId);
        var vm = new PresetsViewModel(_main, aircraft);
        _main.Navigation.Push(vm);
        return vm;
    }
}
