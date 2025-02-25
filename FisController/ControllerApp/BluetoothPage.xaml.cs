using ControllerApp.Services;
using ControllerApp.ViewModels;
using Plugin.BLE.Abstractions.EventArgs;

namespace ControllerApp
{
    public partial class BluetoothPage : ContentPage
    {
        private readonly BleService bleService;

        public BluetoothPage(BleService bleService, DevicesViewModel devicesViewModel)
        {
            InitializeComponent();
            this.bleService = bleService;
            Initialize();
            BindingContext = devicesViewModel;

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
