using ControllerApp.Resources;
using ControllerApp.Services;
using Mapbox.Directions;
using Plugin.BLE.Abstractions.EventArgs;

namespace ControllerApp;

public partial class TestWriteBLE : ContentPage
{
    private readonly BleService bleService;
    private readonly FisNavigationService fisNavigationService;
    private NavigationTemplate naviTemplate = new NavigationTemplate();
    private MapService mapService;
    public TestWriteBLE(BleService bleService, MapService map)
	{
		InitializeComponent();
        this.bleService = bleService;
        fisNavigationService = new FisNavigationService(this.bleService);

        BindingContext = naviTemplate;

        bleService.SetupConnectedEvent(OnDeviceConnected);

        mapService = map;
        mapService.DirectionsResponseReceived += DirectionsResponseReceived;
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

    private async void SendNavigation_Clicked(object sender, EventArgs e)
    {
        await fisNavigationService.SendNavigationData();
    }

    private void CycleDirection_Clicked(object sender, EventArgs e)
    {
        naviTemplate = fisNavigationService.SetUpNextDirection();
        BindingContext = naviTemplate;
    }

    private void DirectionsResponseReceived(object? sender, DirectionsResponse e)
    {
        var template = new List<NavigationTemplate>();
        var legs = e.Routes.FirstOrDefault()?.Legs.FirstOrDefault();
        if (legs != null)
        {
            template = mapLegToTemplates(legs);
        }
        fisNavigationService.SetNavigationTemplates(template);
    }

    private List<NavigationTemplate> mapLegToTemplates(Leg leg)
    {
        var templates = new List<NavigationTemplate>();
        var totalDistance = leg.Distance;

        foreach (var step in leg.Steps)
        {
            templates.Add(new NavigationTemplate
            {
                CurrentAddress = step.Name,
                TotalDistance = CalculateRemainingDistance(step, leg.Steps),
                DistanceToNextTurn = (decimal)step.Distance/1000,
                ArrivalTime = TimeOnly.FromDateTime(DateTime.Now.AddSeconds(step.Duration)),
            });
        }

        return templates;
    }

    private decimal CalculateRemainingDistance(Step currentStep, List<Step> allSteps)
    {
        decimal distance = 0;
        var remainingSteps = allSteps;
        remainingSteps.RemoveRange(allSteps.IndexOf(currentStep), allSteps.Count - allSteps.IndexOf(currentStep));
        distance = (decimal)remainingSteps.Sum(x => x.Distance);
        return distance;
    }
}