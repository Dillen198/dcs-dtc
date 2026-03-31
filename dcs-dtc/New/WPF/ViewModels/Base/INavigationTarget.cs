namespace DTC.New.WPF.ViewModels.Base;

/// <summary>
/// Marker interface for ViewModels that can appear as a page in the navigation stack.
/// </summary>
public interface INavigationTarget
{
    string PageTitle { get; }
}
