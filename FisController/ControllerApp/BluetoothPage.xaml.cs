using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;

namespace ControllerApp
{
	public partial class BluetoothPage : ContentPage
	{
		private IBluetoothLE ble;
		private IAdapter adapter;
        public ObservableCollection<IDevice> Devices { get; private set; }

        public BluetoothPage()
		{
			InitializeComponent();
			Initialize();
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
            DeviceList.ItemTemplate = DeviceListTemplate();
        }

        private static DataTemplate DeviceListTemplate()
        {
            var textCell = new TextCell();
            textCell.SetBinding(TextCell.TextProperty, "Name");
            textCell.SetBinding(TextCell.DetailProperty, "Id");
            return new DataTemplate(() => textCell);
        }

        private async void OnScanForDevices_Clicked(object sender, EventArgs e)
        {
            await adapter.StartScanningForDevicesAsync();
        }
    }
}
