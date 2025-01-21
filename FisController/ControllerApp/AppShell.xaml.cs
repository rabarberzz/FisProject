namespace ControllerApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("BluetoothPage", typeof(BluetoothPage));
        }
    }
}
