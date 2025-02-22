using ControllerApp.Services;
using Plugin.BLE.Abstractions.EventArgs;

namespace ControllerApp;

public partial class TestWriteBLE : ContentPage
{
    private readonly BleService bleService;
    public TestWriteBLE(BleService bleService)
	{
		InitializeComponent();
        this.bleService = bleService;
        
        bleService.SetupConnectedEvent(OnDeviceConnected);
    }

    private async void WriteNaviEntry_Completed(object sender, EventArgs e)
    {
        if (bleService != null)
        {
            await bleService.SendNaviDirectionsCodes(WriteNaviEntry.Text);
        }
    }

    private async void WriteRadioEntry_Completed(object sender, EventArgs e)
    {
        if (bleService != null)
        {
            await bleService.SendRadioBytes(WriteRadioEntry.Text);
        }
    }

    private void OnDeviceConnected(object sender, DeviceEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            BleStatusLabel.Text = e.Device.State.ToString();
        });
    }
}