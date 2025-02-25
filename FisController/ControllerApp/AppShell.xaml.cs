namespace ControllerApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(BluetoothPage), typeof(BluetoothPage));
            Routing.RegisterRoute(nameof(TestWriteBLE), typeof(TestWriteBLE));
        }
    }
}
