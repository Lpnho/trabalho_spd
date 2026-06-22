using Eto.Forms;
using Freeway.Interfaces;

namespace Freeway.Ui;

public partial class MainWindow : Form, INavigationContext
{
    private Stack<Control> _stack = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    public MainWindow()
    {
        InitializeComponents();
        Closing += OnClosing;
        NavigateTo(new StartWindow(this, _cancellationTokenSource.Token));
    }


    public void NavigateTo(Control control)
    {
        _stack.Push(_container.Content);
        _container.Content = control;
        control.Focus();
    }
    public void NavigatePop()
    {
        _container.Content = _stack.Pop();
    }
    private void OnClosing(object? sender, EventArgs args)
    {
        _cancellationTokenSource.Cancel();
        _stack.Clear();
    }
}
