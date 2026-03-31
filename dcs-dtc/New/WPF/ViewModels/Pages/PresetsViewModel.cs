using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTC.New.Presets.V2.Base;
using DTC.New.WPF.ViewModels.Base;
using DTC.Utilities;
using IPreset = DTC.Utilities.IPreset;

namespace DTC.New.WPF.ViewModels.Pages;

public partial class PresetsViewModel : ViewModelBase, INavigationTarget
{
    private readonly MainViewModel _main;
    private readonly Aircraft _aircraft;

    public string PageTitle => _aircraft.Name + " Presets";

    public IReadOnlyList<IPreset> Presets => _aircraft.Presets;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloneCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private IPreset? _selectedPreset;

    // Raised to request an inline rename/add dialog from the View
    public event Action<IPreset?>? AddRenameRequested;

    public PresetsViewModel(MainViewModel main, Aircraft aircraft)
    {
        _main = main;
        _aircraft = aircraft;
    }

    public void Refresh()
    {
        _aircraft.RefreshPresetList();
        OnPropertyChanged(nameof(Presets));
    }

    [RelayCommand]
    private void Add() => AddRenameRequested?.Invoke(null);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Rename() => AddRenameRequested?.Invoke(SelectedPreset);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit()
    {
        if (SelectedPreset is Preset p)
            OpenPreset(p);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Clone()
    {
        if (SelectedPreset is Preset p)
        {
            var cloned = _aircraft.ClonePreset(p);
            if (cloned != null)
                AddRenameRequested?.Invoke(cloned);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Delete()
    {
        if (SelectedPreset is Preset p)
        {
            if (DTCMessageBox.ShowQuestion("Do you really want to delete " + p.Name + "?"))
            {
                _aircraft.DeletePreset(p);
                Refresh();
            }
        }
    }

    public void OpenPreset(Preset preset)
    {
        var vm = AircraftViewModelFactory.Make(_aircraft, preset, _main);
        _main.Navigation.Push(vm);
    }

    public bool ShowPreset(string name)
    {
        foreach (var p in _aircraft.Presets)
        {
            if (p.Name == name && p is Preset preset)
            {
                OpenPreset(preset);
                return true;
            }
        }
        return false;
    }

    public void CreatePreset(string name)
    {
        if (FileStorage.PresetExists(_aircraft, name))
        {
            if (!DTCMessageBox.ShowQuestion($"Preset with name {name} already exists. Do you want to overwrite it?"))
                return;
        }
        var newPreset = _aircraft.CreatePreset(name);
        _aircraft.PersistPreset(newPreset);
        Refresh();
        OpenPreset(newPreset);
    }

    public void RenamePreset(Preset preset, string newName)
    {
        if (newName == preset.Name) return;
        if (FileStorage.PresetExists(_aircraft, newName))
        {
            if (!DTCMessageBox.ShowQuestion($"Preset with name {newName} already exists. Do you want to overwrite it?"))
                return;
        }
        var oldName = preset.Name;
        preset.Name = newName;
        FileStorage.RenamePresetFile(_aircraft, preset, oldName);
        Refresh();
    }

    private bool HasSelection() => SelectedPreset != null;
}
