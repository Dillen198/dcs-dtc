using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DTC.New.WPF.ViewModels.Base;

public class BreadcrumbItem
{
    public string Title { get; }
    public Action NavigateAction { get; }

    public BreadcrumbItem(string title, Action navigateAction)
    {
        Title = title;
        NavigateAction = navigateAction;
    }
}

public class NavigationService : ObservableObject
{
    private readonly Stack<INavigationTarget> _stack = new();

    public ObservableCollection<BreadcrumbItem> Breadcrumbs { get; } = new();

    private INavigationTarget? _current;
    public INavigationTarget? Current
    {
        get => _current;
        private set => SetProperty(ref _current, value);
    }

    public void Reset(INavigationTarget root)
    {
        // Dispose any disposable pages
        foreach (var item in _stack)
        {
            if (item is IDisposable d) d.Dispose();
        }

        _stack.Clear();
        Breadcrumbs.Clear();

        _stack.Push(root);
        Breadcrumbs.Add(new BreadcrumbItem(root.PageTitle, () => Reset(root)));
        Current = root;
    }

    public void Push(INavigationTarget target)
    {
        _stack.Push(target);
        Breadcrumbs.Add(new BreadcrumbItem(target.PageTitle, () => PopTo(target)));
        Current = target;
    }

    public void PopTo(INavigationTarget target)
    {
        while (_stack.Count > 0 && _stack.Peek() != target)
        {
            var popped = _stack.Pop();
            if (Breadcrumbs.Count > 0)
                Breadcrumbs.RemoveAt(Breadcrumbs.Count - 1);
            if (popped is IDisposable d) d.Dispose();
        }
        Current = _stack.Count > 0 ? _stack.Peek() : null;
    }
}
