using ControllerApp.Services;
using Plugin.BLE.Abstractions.Contracts;
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
        }

        private async Task DisconnectFromDevice(IDevice device)
        {
            if (device != null && device.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                await bleService.TryDisconnectFromDevice(device);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
