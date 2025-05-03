using ControllerApp.Services;
using ControllerApp.ViewModels;
using System.ComponentModel;

namespace ControllerApp;

public partial class EspConfigPage : ContentPage
{
	private EspConfigService espConfigService;
    private EspConfigViewModel viewModel;
    private const decimal IncrementValue = 0.001m; // Small increment value
    private const decimal MinValue = 0m;       // Minimum allowed value
    private const decimal MaxValue = 1.5m;       // Maximum allowed value

    public EspConfigPage(EspConfigService esp)
	{
        espConfigService = esp;
        InitializeComponent();

        viewModel = new EspConfigViewModel();
        viewModel.FineTuneRatio = espConfigService.FineTuneRatio;
        viewModel.SpeedDisplayEnabled = espConfigService.SpeedDisplayEnabled;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (await espConfigService.TryGetConfigFromEsp())
        {
            viewModel.FineTuneRatio = espConfigService.FineTuneRatio;
            viewModel.SpeedDisplayEnabled = espConfigService.SpeedDisplayEnabled;
        }
    }

    private void OnIncrementButtonClicked(object sender, EventArgs e)
    {
        if (BindingContext is EspConfigViewModel viewModel)
        {
            if (viewModel.FineTuneRatio + IncrementValue <= MaxValue)
            {
                viewModel.FineTuneRatio += IncrementValue;
            }
        }
    }

    private void OnDecrementButtonClicked(object sender, EventArgs e)
    {
        if (BindingContext is EspConfigViewModel viewModel)
        {
            if (viewModel.FineTuneRatio - IncrementValue >= MinValue)
            {
                viewModel.FineTuneRatio -= IncrementValue;
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EspConfigViewModel.SpeedDisplayEnabled))
        {
            espConfigService.SpeedDisplayEnabled = viewModel.SpeedDisplayEnabled;
        }

        if (e.PropertyName != nameof(EspConfigViewModel.FineTuneRatio))
        {
            if (viewModel.TyreDiameter > 0 && viewModel.TyreAspectRatio > 0 && viewModel.TyreWidth > 0)
            {
                viewModel.FineTuneRatio = espConfigService.CalculateFineTuneRatio(viewModel.TyreWidth, viewModel.TyreAspectRatio, viewModel.TyreDiameter);
            }
        } 
        else
        {
            espConfigService.FineTuneRatio = viewModel.FineTuneRatio;
        }
    }

    private void SendButton_Clicked(object sender, EventArgs e)
    {
        _ = espConfigService.SendConfigToEsp();
    }
}
