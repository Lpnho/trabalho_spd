using Eto.Drawing;
using Eto.Forms;

namespace Freeway.Ui;

internal partial class ConnectWindow : Panel
{
    private TextBox _ipTextBox;
    private TextBox _portTextBox;

    private Label _ipLogLabel;
    private Label _portLogLabel;
    private Label _generalLogLabel;

    private Button _startButton;
    private Button _cancelButton;

    private void InitializeComponents()
    {
        _ipTextBox = new TextBox
        {
            Width = 300
        };

        _portTextBox = new TextBox
        {
            Width = 300
        };

        _ipLogLabel = new Label
        {
            Text = string.Empty,
            TextColor = Colors.Red
        };

        _portLogLabel = new Label
        {
            Text = string.Empty,
            TextColor = Colors.Red
        };

        _generalLogLabel = new Label
        {
            Text = string.Empty,
            TextColor = Colors.Red,
            Font = SystemFonts.Bold(),
        };

        _startButton = new Button
        {
            Text = "Conectar",
            Width = 140
        };

        _cancelButton = new Button
        {
            Text = "Cancelar",
            Width = 140
        };

        var buttonLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Items =
            {
                _startButton,
                _cancelButton
            }
        };

        var formLayout = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Items =
            {
                new Label
                {
                    Text = "IP do servidor"
                },

                _ipTextBox,

                _ipLogLabel,

                new Label
                {
                    Text = "Porta"
                },

                _portTextBox,

                _portLogLabel,

                _generalLogLabel,

                buttonLayout
            }
        };

        var groupBox = new GroupBox
        {
            Text = "Conectar ao Servidor",
            Padding = 20,
            Content = formLayout
        };

        var layout = new DynamicLayout
        {
            Padding = 20
        };

        layout.Add(null, yscale: true);

        layout.BeginHorizontal();
        layout.Add(null, xscale: true);
        layout.Add(groupBox);
        layout.Add(null, xscale: true);
        layout.EndHorizontal();

        layout.Add(null, yscale: true);

        Content = layout;
    }
}