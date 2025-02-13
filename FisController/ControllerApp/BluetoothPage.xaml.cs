using ControllerApp.Services;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ControllerApp
{
	public partial class BluetoothPage : ContentPage
	{
        public ObservableCollection<IDevice> Devices { private set; get; }
        public ICommand ConnectCommand { get; private set; }
        private BleService bleService;

        public BluetoothPage()
		{
			InitializeComponent();
            bleService = new BleService();
            Initialize();
            BindingContext = this;

            ConnectCommand = new Command<IDevice>(async (device) => await ConnectToDevice(device));
        }

        public void Initialize()
        {
            BleStatusLabel.Dispatcher.Dispatch(() =>
            {
                BleStatusLabel.Text = bleService.GetBleStatus();
            });
            //adapter = bleService.StartScan().Result;
            Devices = new ObservableCollection<IDevice>();

            bleService.SetupDevicesDiscoveredEvent(Devices);
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

        private async Task ConnectToDevice(IDevice device)
        {
            var result = await bleService.TryConnectToDevice(device);
            SetBleConnectStatusLabelDispatch(result);
        }

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
