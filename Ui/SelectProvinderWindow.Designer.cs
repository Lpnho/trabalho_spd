using Eto.Drawing;
using Eto.Forms;

namespace Freeway.Ui;

public partial class SelectProvinderWindow : Panel
{
    private Button _grpcServerButton;
    private Button _socketServerButton;
    private Button _mqttClientButton;

    private void InitializeComponents()
    {
        _grpcServerButton = new Button
        {
            Text = "Grpc ",
            Width = 450,
            Height = 100
        };

        _socketServerButton = new Button
        {
            Text = "Socket",
            Width = 450,
            Height = 100
        };
        _mqttClientButton = new Button
        {
            Text = "Mqtt",
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
                _grpcServerButton,
                _socketServerButton,
                _mqttClientButton
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