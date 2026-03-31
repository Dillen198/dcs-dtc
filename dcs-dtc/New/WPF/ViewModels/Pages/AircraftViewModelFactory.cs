using DTC.New.Presets.V2.Base;
using DTC.New.UI.Base.Pages;

namespace DTC.New.WPF.ViewModels.Pages;

internal static class AircraftViewModelFactory
{
    public static AircraftViewModel Make(Aircraft aircraft, Preset preset, MainViewModel main)
    {
        // Reuse the existing WinForms AircraftPageFactory to create the underlying page.
        // The WPF shell wraps it via WindowsFormsHost; the WinForms page manages upload/capture logic.
        var winFormsPage = AircraftPageFactory.Make(aircraft, preset);
        return new AircraftViewModel(main, aircraft, preset, winFormsPage);
    }
}
