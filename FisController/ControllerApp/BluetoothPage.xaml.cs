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
            //adapter = bleService.StartScan().Result;

            //bleService.SetupDevicesDiscoveredEvent(Devices);
            //DeviceList.ItemsSource = Devices;
        }

        private async void OnScanForDevices_Clicked(object sender, EventArgs e)
        {
            await bleService.StartScan();
        }

        private async void OnTest_Clicked(object sender, EventArgs e)
        {
            await bleService.TestConnection();
        }

        private void OnDeviceConnectionChanged(object sender, DeviceEventArgs args)
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

        //private async Task ConnectToDevice(IDevice device)
        //{
        //    var result = await bleService.TryConnectToDevice(device);
        //    SetBleConnectStatusLabelDispatch(result);
        //    ConnectCommand = new Command<IDevice>(async (device) => await DisconnectFromDevice(device));
            
        //}

        //private async Task DisconnectFromDevice(IDevice device)
        //{
        //    await bleService.TryDisconnectFromDevice(device);
        //    SetBleConnectStatusLabelDispatch(false);
        //}

        private void SetBleConnectStatusLabelDispatch(bool connectResult)
        {
            if (connectResult)
            {
                BleConnectStatusLabel.Dispatcher.Dispatch(() =>
                {
                    BleConnectStatusLabel.Text = "Connected";
                });
            }
            else
            {
                BleConnectStatusLabel.Dispatcher.Dispatch(() =>
                {
                    BleConnectStatusLabel.Text = "Failed to connect";
                });
            }
        }

        private async void SetError(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
