using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System.Collections.ObjectModel;
using System.Text;

namespace ControllerApp.Services
{
    public class BleService
    {
        static Guid serviceUUID = Guid.Parse("f15aaf00-fc20-47c7-a574-9411948aed62"); // device/service UUID
        static Guid charUUID = Guid.Parse("f15aaf01-fc20-47c7-a574-9411948aed62"); // text characteristic UUID
        static Guid naviUUID = Guid.Parse("f15aaf02-fc20-47c7-a574-9411948aed62"); // navigation characteristic UUID

        private IBluetoothLE ble;
        private IAdapter adapter;
        private CancellationTokenSource bleCancellationSource;
        private IDevice? device;

        private ICharacteristic? radioCharacteristic;
        private ICharacteristic? naviCharacteristic;

        public BleService()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            bleCancellationSource = new CancellationTokenSource();
        }

        private async Task<bool> SetUpCharacteristics()
        {
            if (device == null) return false;

            var service = await device.GetServiceAsync(serviceUUID);

            if (service != null)
            {
                radioCharacteristic = await service.GetCharacteristicAsync(charUUID);
                naviCharacteristic = await service.GetCharacteristicAsync(naviUUID);
                return radioCharacteristic != null && naviCharacteristic != null;
            }

            return false;
        }

        private char[] ParseCharacterCodes(string charCodes)
        {
            var codes = charCodes.Split(new[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries);
            var characters = new char[codes.Length];

            for (int i = 0; i < codes.Length; i++)
            {
                characters[i] = (char)Convert.ToInt32(codes[i], 16);
            }

            return characters;
        }

        private void SyncDeviceList(ObservableCollection<IDevice> devices)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                devices.Clear();
                foreach (var device in adapter.ConnectedDevices)
                {
                    if (!devices.Contains(device))
                    {
                        devices.Add(device);
                    }
                }
                foreach (var device in adapter.DiscoveredDevices)
                {
                    if (!devices.Contains(device))
                    {
                        devices.Add(device);
                    }
                }
                foreach (var device in adapter.BondedDevices)
                {
                    if (!devices.Contains(device))
                    {
                        devices.Add(device);
                    }
                }
            });
        }

        public async Task<bool> TryConnectToDevice(IDevice connectDevice)
        {
            if (adapter != null)
            {
                try
                {
                    await adapter.ConnectToDeviceAsync(connectDevice, cancellationToken: bleCancellationSource.Token);
                    device = connectDevice;

                    return await SetUpCharacteristics();
                }
                catch (DeviceConnectionException ex)
                {
                    bleCancellationSource.Cancel();
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> TryDisconnectFromDevice(IDevice connectedDevice)
        {
            if (adapter != null)
            {
                if (device != null && device == connectedDevice)
                {
                    await adapter.DisconnectDeviceAsync(connectedDevice);
                    device = null;
                    return true;
                }
            }
            return false;
        }


        public async Task<bool> TestConnection()
        {
            if (radioCharacteristic != null)
            {
                var result = await radioCharacteristic.WriteAsync(Encoding.UTF8.GetBytes("TEST\nFISCONTROL"));
                if (result != 0) return false;
                Thread.Sleep(2000);
                result = await radioCharacteristic.WriteAsync(Encoding.UTF8.GetBytes(""));
                return result == 0;
            }

            return false;
        }

        public async Task<IAdapter> StartScan(bool scanWithFilter = true)
        {
            adapter.ScanMode = ScanMode.Balanced;
            if (scanWithFilter)
            {
                await adapter.StartScanningForDevicesAsync([serviceUUID]);
            }
            else
            {
                await adapter.StartScanningForDevicesAsync();
            }

            return adapter;
        }

        public string GetBleStatus()
        {
            return ble.State.ToString();
        }

        public string GetDeviceConnectedStatus()
        {
            return device != null && device.BondState == DeviceBondState.Bonded ? "Connected" : "Not connected";
        }

        public void SetupDevicesDiscoveredEvent(ObservableCollection<IDevice> devices)
        {
            if (adapter == null)
            {
                return;
            }

            adapter.DeviceDiscovered += (s, a) => SyncDeviceList(devices);
            adapter.DeviceConnected += (s, a) => SyncDeviceList(devices);
            adapter.DeviceDisconnected += (s, a) => SyncDeviceList(devices);
        }

        public async Task SendRadioBytes(string text)
        {
            if (radioCharacteristic != null)
            {
                var encodedText = Encoding.UTF8.GetBytes(text);
                await radioCharacteristic.WriteAsync(encodedText);
            }
        }

        public async Task SendNaviDirectionsCodes(string text)
        {
            if (naviCharacteristic != null)
            {
                var charCodes = ParseCharacterCodes(text);
                var charactersString = new string(charCodes);
                var encodedText = Encoding.UTF8.GetBytes(charactersString);
                await naviCharacteristic.WriteAsync(encodedText);
            }
        }

        public async Task SendNaviBytes(string text)
        {
            if (naviCharacteristic != null)
            {
                var encodedText = Encoding.UTF8.GetBytes(text);
                await naviCharacteristic.WriteAsync(encodedText);
            }
        }

        public void SetupConnectedEvent(EventHandler<DeviceEventArgs> eventHandler)
        {
            adapter.DeviceConnected += eventHandler;
        }
    }
}
