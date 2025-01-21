using Android.Bluetooth;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;

namespace ControllerApp
{
	public partial class BluetoothPage : ContentPage
	{
        private IBluetoothLE ble;
        private IAdapter adapter;
        public ObservableCollection<IDevice> Devices { private set; get; }

        public BluetoothPage()
		{
			InitializeComponent();
            Initialize();
            BindingContext = this;
        }

        public void Initialize()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            Devices = new ObservableCollection<IDevice>();

            BleStatusLabel.Text = ble.State.ToString();

            ble.StateChanged += (s, e) =>
            {
                BleStatusLabel.Dispatcher.Dispatch(() =>
                {
                    BleStatusLabel.Text = ble.State.ToString();
                });
            };

            adapter.DeviceDiscovered += (s, a) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Devices.Add(a.Device);
                });
            };


            DeviceList.ItemsSource = Devices;
        }

        private async void OnScanForDevices_Clicked(object sender, EventArgs e)
        {
            await adapter.StartScanningForDevicesAsync();
        }

        private async void SetError(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
