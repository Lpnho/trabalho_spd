using Eto.Forms;
using Freeway.Implementation.GoogleRpc;
using Freeway.Implementation.Sockets;
using Freeway.Singleton;
using Freeway.Ui;

namespace Freeway;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        //new NetworkProvinderSingleton(new SocketNetworkProvinder());
        //new NetworkProvinderSingleton(new GrpcNetworkProvinder());
        //new NetworkProvinderSingleton(new MqttNetworkProvinder());
        new Application(new Eto.GtkSharp.Platform()).Run(new MainWindow());
    }
}