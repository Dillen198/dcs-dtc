using DTC.New.Presets.V2.Base;
using DTC.Utilities;
using DTC.New.WPF.ViewModels.Pages;
using System.Windows;
using System.Windows.Input;

namespace DTC.New.WPF.Views.Pages;

public partial class PresetsPageView : System.Windows.Controls.UserControl
{
    private IPreset? _pendingPreset; // null = add, non-null = rename/clone

    public PresetsPageView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is PresetsViewModel oldVm)
            oldVm.AddRenameRequested -= ShowDialog;
        if (e.NewValue is PresetsViewModel newVm)
            newVm.AddRenameRequested += ShowDialog;
    }

    private void ShowDialog(IPreset? preset)
    {
        _pendingPreset = preset;
        dialogTitle.Text = preset == null ? "New Preset Name" : "Rename Preset";
        dialogTextBox.Text = preset?.Name ?? "";
        dialogOverlay.Visibility = Visibility.Visible;
        dialogTextBox.Focus();
        dialogTextBox.SelectAll();
    }

    private void HideDialog()
    {
        dialogOverlay.Visibility = Visibility.Collapsed;
        _pendingPreset = null;
    }

    private void CommitDialog()
    {
        var name = dialogTextBox.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        if (DataContext is PresetsViewModel vm)
        {
            if (_pendingPreset == null)
                vm.CreatePreset(name);
            else if (_pendingPreset is Preset p)
                vm.RenamePreset(p, name);
        }
        HideDialog();
    }

    private void DialogOk_Click(object sender, RoutedEventArgs e) => CommitDialog();

    private void DialogCancel_Click(object sender, RoutedEventArgs e) => HideDialog();

    private void DialogTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter) CommitDialog();
        else if (e.Key == Key.Escape) HideDialog();
    }

    private void DataGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is PresetsViewModel vm)
            vm.EditCommand.Execute(null);
    }
}
