using ControllerApp.Resources;
using ControllerApp.Services;
using ControllerApp.ViewModels;
using Plugin.BLE.Abstractions.EventArgs;

namespace ControllerApp
{
    public partial class BluetoothPage : ContentPage
    {
        private readonly BleService bleService;
        private readonly FisNavigationService fisSvc;

        public BluetoothPage(BleService bleService, DevicesViewModel devicesViewModel, FisNavigationService fisSvc)
        {
            InitializeComponent();
            this.bleService = bleService;
            Initialize();
            BindingContext = devicesViewModel;
            this.fisSvc = fisSvc;

            devicesViewModel.DeviceConnectionChanged += OnDeviceConnectionChanged;
        }

        public void Initialize()
        {
            BleStatusLabel.Dispatcher.Dispatch(() =>
            {
                BleStatusLabel.Text = bleService.GetBleStatus();
            });
        }

        private async void OnScanForDevices_Clicked(object sender, EventArgs e)
        {
            await bleService.StartScan();
        }

        private async void OnTest_Clicked(object sender, EventArgs e)
        {
            await bleService.TestConnection();
            TestNavigationIcons();
        }

        private void TestNavigationIcons()
        {
            var iconsList = DirectionsCodes.GetDirectionsCodes();
            foreach (var icon in iconsList)
            {
                var template = new NavigationTemplate()
                {
                    ArrivalTime = new TimeOnly(00, 00),
                    DistanceToNextTurn = 0,
                    TotalDistance = 0,
                    DirectionsIcon = icon.Value
                };
                
                fisSvc.SetCurrentNavigation(template);

                Thread.Sleep(2000);
            }
        }

        private void OnDeviceConnectionChanged(object? sender, DeviceEventArgs args)
        {
            if (args.Device != null)
            {
                BleConnectStatusLabel.Dispatcher.Dispatch(() =>
                {
                    BleConnectStatusLabel.Text = $"Connected: {args.Device.Name}";
                });
            }
            else
            {
                BleConnectStatusLabel.Dispatcher.Dispatch(() =>
                {
                    BleConnectStatusLabel.Text = "Disconnected";
                });
            }
        }

        private async void SetError(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
