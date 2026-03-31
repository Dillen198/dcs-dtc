using DTC.New.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfButton = System.Windows.Controls.Button;
using WpfCursors = System.Windows.Input.Cursors;
using WpfFontFamily = System.Windows.Media.FontFamily;

namespace DTC.New.WPF.Views;

public partial class BreadcrumbBar : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource),
            typeof(ObservableCollection<BreadcrumbItem>),
            typeof(BreadcrumbBar),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public ObservableCollection<BreadcrumbItem>? ItemsSource
    {
        get => (ObservableCollection<BreadcrumbItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public BreadcrumbBar()
    {
        InitializeComponent();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (BreadcrumbBar)d;
        if (e.OldValue is ObservableCollection<BreadcrumbItem> old)
            old.CollectionChanged -= bar.OnCollectionChanged;
        if (e.NewValue is ObservableCollection<BreadcrumbItem> newColl)
        {
            newColl.CollectionChanged += bar.OnCollectionChanged;
            bar.Rebuild(newColl);
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Rebuild(ItemsSource);
    }

    private void Rebuild(IEnumerable<BreadcrumbItem>? items)
    {
        PART_Panel.Children.Clear();
        if (items == null) return;

        var itemList = items.ToList();
        for (int i = 0; i < itemList.Count; i++)
        {
            var crumb = itemList[i];
            bool isLast = i == itemList.Count - 1;

            if (i > 0)
            {
                PART_Panel.Children.Add(new TextBlock
                {
                    Text = " › ",
                    Foreground = (WpfBrush)FindResource("TextDisabledBrush"),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            var btn = new WpfButton
            {
                Content = crumb.Title,
                Background = WpfBrushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = (WpfBrush)FindResource(isLast ? "TextPrimaryBrush" : "TextSecondaryBrush"),
                FontFamily = (WpfFontFamily)FindResource("UIFont"),
                FontSize = 12,
                Cursor = WpfCursors.Hand,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = crumb
            };

            var capturedIsLast = isLast;
            btn.Click += (s, _) =>
            {
                if (s is WpfButton b && b.Tag is BreadcrumbItem c)
                    c.NavigateAction?.Invoke();
            };
            btn.MouseEnter += (s, _) =>
            {
                if (s is WpfButton b)
                    b.Foreground = (WpfBrush)FindResource("AccentGreenBrightBrush");
            };
            btn.MouseLeave += (s, _) =>
            {
                if (s is WpfButton b)
                    b.Foreground = (WpfBrush)FindResource(capturedIsLast ? "TextPrimaryBrush" : "TextSecondaryBrush");
            };

            PART_Panel.Children.Add(btn);
        }
    }
}
