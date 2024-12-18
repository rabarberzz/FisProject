using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace ControllerDevTool
{
    public class BLEServerService
    {
        public GattServiceProvider serviceProvider { get; private set; }
        private GattLocalCharacteristic inputCharacteristic;
        public GattLocalCharacteristic outputCharacteristic { get; private set; }
        private bool peripheralSupported;
        public string ErrorMessage { get; private set; }
        public event EventHandler<string> WriteRequested;

        public async Task<bool> StartServer()
        {
            if (serviceProvider == null)
            {
                var localAdapter = await BluetoothAdapter.GetDefaultAsync();
                if (localAdapter != null)
                {
                    peripheralSupported = localAdapter.IsPeripheralRoleSupported;
                }
                else
                {
                    peripheralSupported = false;
                }

                return await ServiceProviderInitAsync();
            }
            ErrorMessage += "\nFailed to start server";
            return false;
        }

        public void StopServer()
        {
            if (serviceProvider != null)
            {
                serviceProvider.StopAdvertising();
                serviceProvider = null;
            }
        }

        private async Task<bool> ServiceProviderInitAsync()
        {
            GattServiceProviderResult serviceResult = await GattServiceProvider.CreateAsync(Constants.BleServerUuid);

            if (serviceResult.Error == BluetoothError.Success)
            {
                serviceProvider = serviceResult.ServiceProvider;
            }
            else
            {
                ErrorMessage = "Failed to create service provider: " + serviceResult.Error.ToString();
                return false;
            }

            GattLocalCharacteristicResult result = await serviceProvider.Service.CreateCharacteristicAsync(Constants.BleInputCharacteristicUuid, Constants.InputCharacteristicParameters);
            if (result.Error == BluetoothError.Success)
            {
                inputCharacteristic = result.Characteristic;
            }
            else
            {
                ErrorMessage = "Failed to create input characteristic: " + result.Error.ToString();
                return false;
            }
            inputCharacteristic.WriteRequested += InputCharacteristic_WriteRequestedAsync;

            // presentation format - 32-bit unsigned integer, with exponent 0, unitless, no company description
            GattPresentationFormat intFormat = GattPresentationFormat.FromParts(
                GattPresentationFormatTypes.UInt32,
                0,
                Convert.ToUInt16(0x2700),
                Convert.ToByte(1),
                0x0000);

            Constants.OutputCharacteristicParameters.PresentationFormats.Add(intFormat);

            result = await serviceProvider.Service.CreateCharacteristicAsync(Constants.BleOutputCharacteristicUuid, Constants.OutputCharacteristicParameters);
            if (result.Error == BluetoothError.Success)
            {
                outputCharacteristic = result.Characteristic;
            }
            else
            {
                ErrorMessage = "Failed to create output characteristic: " + result.Error.ToString();
                return false;
            }
            outputCharacteristic.ReadRequested += OutputCharacteristic_ReadRequested;
            //outputCharacteristic.SubscribedClientsChanged += OutputCharacteristic_SubscribedClientsChanged;

            // Set the advertising parameters: connectable, discoverable
            GattServiceProviderAdvertisingParameters advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = peripheralSupported
            };
            serviceProvider.StartAdvertising(advParameters);

            return true;
        }

        private async void InputCharacteristic_WriteRequestedAsync(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            using (args.GetDeferral())
            {
                GattWriteRequest request = await args.GetRequestAsync();
                if (request == null)
                {
                    return;
                }
                ProcessWriteCharacteristic(request);
            }
        }

        private async void OutputCharacteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs args)
        {
            using (args.GetDeferral())
            {
                GattReadRequest request = await args.GetRequestAsync();
                if (request == null)
                {
                    return;
                }

                var writer = new DataWriter();
                writer.ByteOrder = ByteOrder.LittleEndian;
                writer.WriteString("Hello, client!");

                request.RespondWithValue(writer.DetachBuffer());
            }
        }

        private void ProcessWriteCharacteristic(GattWriteRequest request)
        {
            var reader = DataReader.FromBuffer(request.Value);
            reader.ByteOrder = ByteOrder.LittleEndian;
            var value = reader.ReadString(request.Value.Length);
            WriteRequested?.Invoke(this, value);
            if (request.Option == GattWriteOption.WriteWithResponse)
            {
                request.Respond();
            }
        }

        public async void NotifyClientDevices(string message)
        {
            if (outputCharacteristic != null)
            {
                var writer = new DataWriter();
                writer.WriteString(message);
                await outputCharacteristic.NotifyValueAsync(writer.DetachBuffer());
            }
        }
    }
}