using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace TFLitePoseTrainer.Behaviors;

public class ListBoxSelectionBehavior<T> : Behavior<ListBox>
{
    #region == SelectedItems ==

    public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                nameof(SelectedItems),
                typeof(IList<T>),
                typeof(ListBoxSelectionBehavior<T>),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = null,
                    PropertyChangedCallback = OnSelectedItemsChanged,
                });

    public IList<T> SelectedItems
    {
        get => (IList<T>)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    private static void OnSelectedItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is not ListBoxSelectionBehavior<T> behavior)
        {
            return;
        }

        if (args.OldValue is INotifyCollectionChanged oldObservableSelectedItems)
        {
            oldObservableSelectedItems.CollectionChanged -= behavior.OnSourceSelectedItemsChanged;
        }

        if (args.NewValue is INotifyCollectionChanged newObservableSelectedItems)
        {
            newObservableSelectedItems.CollectionChanged += behavior.OnSourceSelectedItemsChanged;
        }

        if (args.NewValue is not IList<T> newSelectedItems)
        {
            return;
        }

        behavior.SelectItems(newSelectedItems);
    }

    private void OnSourceSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ApplyChange(_listBox?.SelectedItems, selectedItems =>
        {
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    selectedItems.Remove(item);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    selectedItems.Add(item);
                }
            }
        });
    }

    #endregion

    private ListBox? _listBox;
    private bool _isChanging = false;

    protected override void OnAttached()
    {
        base.OnAttached();

        _listBox = AssociatedObject;
        _listBox.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (_listBox is null)
        {
            return;
        }

        _listBox.SelectionChanged -= OnSelectionChanged;
        _listBox = null;
    }

    private void SelectItems(IList<T> items)
    {
        ApplyChange(_listBox?.SelectedItems, selectedItems =>
        {
            selectedItems.Clear();

            foreach (var item in items)
            {
                selectedItems.Add(item);
            }
        });
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyChange(SelectedItems, selectedItems =>
        {
            foreach (var item in e.RemovedItems)
            {
                selectedItems.Remove((T)item);
            }

            foreach (var item in e.AddedItems)
            {
                selectedItems.Add((T)item);
            }
        });
    }

    private void ApplyChange(System.Collections.IList? items, Action<System.Collections.IList> action)
    {
        if (_isChanging || items is null)
        {
            return;
        }

        _isChanging = true;
        action(items);
        _isChanging = false;
    }

    private void ApplyChange(IList<T>? items, Action<IList<T>> action)
    {
        if (_isChanging || items is null)
        {
            return;
        }

        _isChanging = true;
        action(items);
        _isChanging = false;
    }
}
