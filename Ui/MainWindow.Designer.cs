using Eto.Forms;
using Freeway.Interfaces;
using Freeway.Singleton;

namespace Freeway.Ui;
public partial class MainWindow : Form, INavigationContext
{
    Panel _container;
    private void InitializeComponents()
    {
        Title = "Freeway";
        _container = new Panel();
        Content = _container;

        Size = ConfigurationSingleton.DefaultSize;
        MinimumSize = ConfigurationSingleton.MinimumSize;
        Resizable = false;
    }


}
