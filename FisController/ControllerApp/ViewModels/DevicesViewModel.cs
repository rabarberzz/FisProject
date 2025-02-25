using ControllerApp.Services;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ControllerApp.ViewModels
{
    public class DevicesViewModel : INotifyPropertyChanged
    {
        private readonly BleService bleService;
        public string ConnectionButtonText { get; private set; } = "Connect/Disconnect";

        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ConnectCommand { get; private set; }
        public ObservableCollection<IDevice> Devices { private set; get; } = new ObservableCollection<IDevice>();
        public event EventHandler<DeviceEventArgs>? DeviceConnectionChanged;

        // Default constructor for XAML instantiation
        public DevicesViewModel()
        {
        }

        public DevicesViewModel(BleService service)
        {
            bleService = service;
            ConnectCommand = new Command<IDevice>(async (device) =>
            {
                if (device.State == Plugin.BLE.Abstractions.DeviceState.Connected)
                {
                    await DisconnectFromDevice(device);
                }
                else
                {
                    await ConnectToDevice(device);
                }
            });

            bleService.SetupDevicesDiscoveredEvent(Devices);
        }

        private async Task ConnectToDevice(IDevice device)
        {
            var result = await bleService.TryConnectToDevice(device);
            if (result)
            {
                DeviceConnectionChanged?.Invoke(this, new DeviceEventArgs() { Device = device});
            }
        }

        private async Task DisconnectFromDevice(IDevice device)
        {
            if (device != null && device.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var result = await bleService.TryDisconnectFromDevice(device);
                if (result)
                {
                    DeviceConnectionChanged?.Invoke(this, new DeviceEventArgs());
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnDeviceConnectionChanged(DeviceEventArgs e)
        {
            DeviceConnectionChanged?.Invoke(this, e);
        }
    }
}
