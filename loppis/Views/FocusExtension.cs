using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactions;
using System.Windows;

namespace loppis.Views;

public class FocusExtension
{
    public static readonly AttachedProperty<bool> IsFocusedProperty = AvaloniaProperty.RegisterAttached<FocusExtension, Control, bool>("IsFocused"/*, typeof(FocusExtension)/*, new UIPropertyMetadata(false, OnIsFocusedPropertyChanged)*/);

    public static bool GetIsFocused(Control element)
    {
        {
            return element.GetValue(IsFocusedProperty);
        }
    }

    public static void SetIsFocused(Control element, bool value)
    {
        element.SetValue(IsFocusedProperty, value);
        OnIsFocusedPropertyChanged(element, (AvaloniaPropertyChangedEventArgs)(object)value);
    }

    static FocusExtension()
    {
        IsFocusedProperty.Changed.AddClassHandler<Control>(OnIsFocusedPropertyChanged);
    }

    private static void OnIsFocusedPropertyChanged(Control element, AvaloniaPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            element.Focus();
        }
    }
}
