using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ControllerDevTool
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerControlPage : Page
    {
        private BLEServerService bleServerService;

        public ServerControlPage()
        {
            this.InitializeComponent();

            bleServerService = App.SharedBleServerService;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (bleServerService != null)
            {
                bleServerService.WriteRequested += OnWriteRequested; // Subscribe to the event
                if (bleServerService.outputCharacteristic != null)
                {
                    bleServerService.outputCharacteristic.SubscribedClientsChanged += OutputCharacteristic_SubscribedClientsChanged;
                }
            }
        }

        private void OnWriteRequested(object sender, string message)
        {
            _ = UpdateServerStatusMessageAsync("Data received: " + message);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            bleServerService.StopServer();
            _ = UpdateServerStatusMessageAsync("Service stopped");
        }

        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            var serviceStarted = await bleServerService.StartServer();
            if (serviceStarted)
            {
                ServerStatusMessage.Text = "Service started";
                bleServerService.WriteRequested += OnWriteRequested; // Subscribe to the event
                bleServerService.outputCharacteristic.SubscribedClientsChanged += OutputCharacteristic_SubscribedClientsChanged;
            }
            else
            {
                ServerStatusMessage.Text = bleServerService.ErrorMessage;
            }
        }

        private async void OutputCharacteristic_SubscribedClientsChanged(GattLocalCharacteristic sender, object args)
        {
            await UpdateServerSubscriberCountAsync(sender.SubscribedClients.Count);
        }

        private async Task UpdateServerStatusMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ServerStatusMessage.Text = message;
            });
        }

        private async Task UpdateServerSubscriberCountAsync(int count)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ServerSubscribersCount.Text = "Subscribers: " + count;
            });
        }
    }
}
