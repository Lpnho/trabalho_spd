using Eto.Drawing;
using Eto.Forms;

namespace Freeway.Ui;

internal partial class StartWindow : Panel
{
    private Button _startServerButton;
    private Button _startClientButton;

    private void InitializeComponents()
    {
        _startServerButton = new Button
        {
            Text = "Iniciar Jogo",
            Width = 450,
            Height = 100
        };

        _startClientButton = new Button
        {
            Text = "Conectar",
            Width = 450,
            Height = 100
        };

        var buttonLayout = new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Spacing = 10,
            Items =
            {
                _startServerButton,
                _startClientButton
            }
        };

        var layout = new DynamicLayout
        {
            Padding = 20,
            DefaultSpacing = new Size(10, 10)
        };

        layout.Add(null, yscale: true);

        layout.BeginHorizontal();
        layout.Add(null, xscale: true);
        layout.Add(buttonLayout);
        layout.Add(null, xscale: true);
        layout.EndHorizontal();

        layout.Add(null, yscale: true);

        Content = layout;
    }
}