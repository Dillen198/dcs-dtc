using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTC.New.Presets.V2.Base;
using DTC.New.UI.Base.Pages;
using DTC.New.UI.Base.Systems;
using DTC.New.WPF.ViewModels.Base;
using DTC.Utilities.Network;

namespace DTC.New.WPF.ViewModels.Pages;

/// <summary>
/// Represents a single tab in the aircraft sidebar.
/// Wraps an existing WinForms AircraftSystemPage for compatibility during migration.
/// </summary>
public class SystemTabItem : ObservableObject
{
    public string Title { get; }
    public bool IsDivider { get; }

    /// <summary>The underlying WinForms system page (null for dividers).</summary>
    public AircraftSystemPage? SystemPage { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public SystemTabItem(AircraftSystemPage page)
    {
        Title = page.GetPageTitle();
        SystemPage = page;
        IsDivider = false;
    }

    public SystemTabItem()
    {
        Title = "---";
        IsDivider = true;
    }
}

/// <summary>
/// ViewModel for the aircraft configuration page shell.
/// Owns the list of system tabs and delegates content to WinForms AircraftSystemPages via WindowsFormsHost.
/// </summary>
public partial class AircraftViewModel : ViewModelBase, INavigationTarget, IDisposable
{
    private readonly MainViewModel _main;
    private readonly Aircraft _aircraft;
    private readonly Preset _preset;

    // The underlying WinForms AircraftPage — kept alive for state, upload logic, and waypoint capture
    public AircraftPage WinFormsPage { get; }

    public string PageTitle => _preset.Name;
    public string AircraftName => _aircraft.Name;

    public List<SystemTabItem> Tabs { get; }

    [ObservableProperty]
    private SystemTabItem? _selectedTab;

    public AircraftViewModel(MainViewModel main, Aircraft aircraft, Preset preset, AircraftPage winFormsPage)
    {
        _main = main;
        _aircraft = aircraft;
        _preset = preset;
        WinFormsPage = winFormsPage;

        // Build tab list from the WinForms page's system pages
        Tabs = BuildTabs(winFormsPage);

        // Select first non-divider tab
        SelectedTab = Tabs.FirstOrDefault(t => !t.IsDivider);
    }

    private static List<SystemTabItem> BuildTabs(AircraftPage page)
    {
        var tabs = new List<SystemTabItem>();
        foreach (var sysPage in page.GetSystemPagesArray())
        {
            tabs.Add(sysPage.IsDivider() ? new SystemTabItem() : new SystemTabItem(sysPage));
        }
        return tabs;
    }

    partial void OnSelectedTabChanged(SystemTabItem? value)
    {
        if (value?.SystemPage != null)
        {
            WinFormsPage.ShowSystemPage(value.SystemPage);
        }
    }

    [RelayCommand]
    public void Upload()
    {
        WinFormsPage.UploadToJet(false, false);
    }

    [RelayCommand]
    public void ShowKneeboard()
    {
        WinFormsPage.ShowKneeboard();
    }

    public void Dispose()
    {
        WinFormsPage.Dispose();
    }
}
