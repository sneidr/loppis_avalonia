using Avalonia.Controls;
using loppis.ViewModels;

namespace loppis;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Initialized(object sender, System.EventArgs e)
    {
        DataContext = new SalesViewModel();
        (DataContext as SalesViewModel)?.LoadCommand.Execute(null);
    }
}