using ControllerApp.Services;

namespace ControllerApp;

public partial class TestWriteBLE : ContentPage
{
    private readonly BleService bleService;
    public TestWriteBLE(BleService bleService)
	{
		InitializeComponent();
        this.bleService = bleService;
        if (bleService != null)
        {
            BleStatusLabel.Text = bleService.GetDeviceConnectedStatus();
        }
    }

    private async void WriteNaviEntry_Completed(object sender, EventArgs e)
    {
        if (bleService != null)
        {
            await bleService.SendNaviBytes(WriteNaviEntry.Text);
        }
    }

    private async void WriteRadioEntry_Completed(object sender, EventArgs e)
    {
        if (bleService != null)
        {
            await bleService.SendRadioBytes(WriteRadioEntry.Text);
        }
    }
}