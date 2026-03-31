using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTC.New.WPF.ViewModels.Base;
using DTC.New.WPF.ViewModels.Pages;
using DTC.Utilities;
using DTC.Utilities.Network;

namespace DTC.New.WPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public NavigationService Navigation { get; } = new NavigationService();

    public string Version => "Version " + Util.GetAppVersion();

    [ObservableProperty]
    private bool _isAlwaysOnTop;

    // Button bar visible when on an aircraft page
    public bool ShowBottomBar => Navigation.Current is AircraftViewModel;

    // Cockpit button state tracking (mirrors MainForm)
    private bool _showDTCPressed;
    private bool _hideDTCPressed;
    private bool _toggleDTCPressed;
    private bool _showHideDTCState;

    // Raised when DCS requests show/hide of the window
    public event Action<bool>? ShowHideRequested;

    // Raised when a preset is selected from the DCS in-game panel
    public event Action<string, string>? PresetSelectedFromDCS;

    public MainViewModel()
    {
        IsAlwaysOnTop = Settings.AlwaysOnTop;

        var mainPage = new MainPageViewModel(this);
        Navigation.Reset(mainPage);
        Navigation.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NavigationService.Current))
                OnPropertyChanged(nameof(ShowBottomBar));
        };

        CockpitInfoReceiver.DataReceived += OnCockpitData;
        CockpitInfoReceiver.Start();
    }

    private void OnCockpitData(CockpitInfoReceiver.Data d)
    {
        if (d.toggleDTC == "1" && !_toggleDTCPressed)
        {
            _toggleDTCPressed = true;
            App.Current.Dispatcher.Invoke(() => ApplyShowHide(!_showHideDTCState));
        }
        if (d.toggleDTC == "0") _toggleDTCPressed = false;

        if (d.showDTC == "1" && !_showDTCPressed)
        {
            _showDTCPressed = true;
            App.Current.Dispatcher.Invoke(() => ApplyShowHide(true));
        }
        if (d.showDTC == "0") _showDTCPressed = false;

        if (d.hideDTC == "1" && !_hideDTCPressed)
        {
            _hideDTCPressed = true;
            App.Current.Dispatcher.Invoke(() => ApplyShowHide(false));
        }
        if (d.hideDTC == "0") _hideDTCPressed = false;

        // Phase C: in-game preset selection
        if (!string.IsNullOrEmpty(d.request_presets))
        {
            HandleRequestPresets(d.request_presets);
        }
        if (!string.IsNullOrEmpty(d.select_preset))
        {
            App.Current.Dispatcher.Invoke(() =>
                PresetSelectedFromDCS?.Invoke(d.select_preset, d.aircraft ?? ""));
        }
    }

    private void ApplyShowHide(bool show)
    {
        _showHideDTCState = show;
        ShowHideRequested?.Invoke(show);
    }

    private void HandleRequestPresets(string aircraftType)
    {
        try
        {
            var aircraft = New.Presets.V2.Base.AircraftRepository.GetAircraft(aircraftType);
            aircraft.RefreshPresetList();
            PresetPanelSender.SendPresetList(aircraft.Presets);
        }
        catch { /* unknown aircraft type from DCS — ignore */ }
    }

    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        IsAlwaysOnTop = !IsAlwaysOnTop;
        Settings.AlwaysOnTop = IsAlwaysOnTop;
    }

    [RelayCommand]
    private void ExecuteUpload()
    {
        if (Navigation.Current is AircraftViewModel av)
            av.UploadCommand.Execute(null);
    }

    [RelayCommand]
    private void ShowKneeboard()
    {
        if (Navigation.Current is AircraftViewModel av)
            av.ShowKneeboardCommand.Execute(null);
    }

    /// <summary>
    /// Navigate to the presets page for the given aircraft (used by command-line and DCS in-game panel).
    /// </summary>
    internal PresetsViewModel NavigateTo(string aircraftId)
    {
        var mainPage = new MainPageViewModel(this);
        Navigation.Reset(mainPage);
        return mainPage.NavigateTo(aircraftId);
    }
}
