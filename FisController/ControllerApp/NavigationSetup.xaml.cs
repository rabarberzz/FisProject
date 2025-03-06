using ControllerApp.SearchBox.Suggestion;
using ControllerApp.Services;
using Mapbox.Directions;

namespace ControllerApp;

public partial class NavigationSetup : ContentPage
{
	private MapService mapService;
    private ListView? currentTargetListView;
    private SuggestionObject? selectedOriginSuggestion;
    private SuggestionObject? selectedDestinationSuggestion;

	public NavigationSetup(MapService mapService)
	{
		InitializeComponent();
		this.mapService = mapService;
        this.mapService.SuggestionResponseReceived += MapService_SuggestionResponseReceived;
    }

    private void MapService_SuggestionResponseReceived(object? sender, SuggestionResponse e)
    {
        if (currentTargetListView != null && e.Suggestions.Count > 0)
        {
            currentTargetListView.ItemsSource = e.Suggestions;
        }
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchBox = (SearchBar)sender;

        if (searchBox == OriginSearchBar)
		{
            mapService.GetLocationSuggestion(searchBox.Text);
            currentTargetListView = OriginResultView;
        }

		if (searchBox == DestinationSearchBar)
		{
            mapService.GetLocationSuggestion(searchBox.Text);
            currentTargetListView = DestinationResultView;
        }
    }

    private void ListViewAddress_Selected(object sender, SelectedItemChangedEventArgs e)
    {
        var listview = (ListView)sender;

        if (e.SelectedItem is SuggestionObject suggestionObject)
        {
            if (listview == OriginResultView)
            {
                selectedOriginSuggestion = suggestionObject;
            }

            if (listview == DestinationResultView)
            {
                selectedDestinationSuggestion = suggestionObject;
            }
        }
    }
}