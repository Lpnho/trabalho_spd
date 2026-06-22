using Eto.Forms;

namespace Freeway.Interfaces;

public interface INavigationContext
{
    public void NavigateTo(Control control);
    public void NavigatePop();

    public event EventHandler<KeyEventArgs>? KeyDown;
}
