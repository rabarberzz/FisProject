using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

public class Constants
{
    public static readonly GattLocalCharacteristicParameters OutputCharacteristicParameters = new GattLocalCharacteristicParameters
    {
        CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify,
        WriteProtectionLevel = GattProtectionLevel.Plain,
        UserDescription = "Service output"
    };

    public static readonly GattLocalCharacteristicParameters InputCharacteristicParameters = new GattLocalCharacteristicParameters
    {
        CharacteristicProperties = GattCharacteristicProperties.Write | GattCharacteristicProperties.WriteWithoutResponse,
        WriteProtectionLevel = GattProtectionLevel.Plain,
        UserDescription = "Service input"
    };

    public static readonly Guid BleServerUuid = Guid.Parse("f15aaf00-fc20-47c7-a574-9411948aed62");

    public static readonly Guid BleOutputCharacteristicUuid = Guid.Parse("f15aaf01-fc20-47c7-a574-9411948aed62");
    public static readonly Guid BleInputCharacteristicUuid = Guid.Parse("f15aaf02-fc20-47c7-a574-9411948aed62");
}
