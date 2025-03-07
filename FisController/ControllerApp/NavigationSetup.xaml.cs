using ControllerApp.SearchBox.Retrieve;
using ControllerApp.SearchBox.Suggestion;
using ControllerApp.Services;
using Mapbox.Directions;
using Mapbox.Utils;
using Mapsui;

namespace ControllerApp;

public partial class NavigationSetup : ContentPage
{
	private MapService mapService;
    private NavigationService navigationService;

    private ListView? currentTargetListView;
    private SuggestionObject? selectedOriginSuggestion;
    private SuggestionObject? selectedDestinationSuggestion;
    private RetrieveResponse? retrieveResponseOrigin;
    private RetrieveResponse? retrieveResponseDestination;
    private MPoint? locationPoint;

    public NavigationSetup(MapService mapService, NavigationService navService)
	{
		InitializeComponent();
		this.mapService = mapService;
        this.mapService.SuggestionResponseReceived += MapService_SuggestionResponseReceived;
        this.mapService.RetrieveResponseReceived += MapService_RetrieveResponseReceived;

        navigationService = navService;
        navigationService.LocationUpdated += NavigationService_LocationUpdated;
    }

    private void NavigationService_LocationUpdated(object? sender, MPoint e)
    {
        locationPoint = e;
    }

    private void MapService_RetrieveResponseReceived(object? sender, RetrieveResponse e)
    {
        if (e != null && e.Features.Count > 0)
        {
            if (selectedOriginSuggestion != null
                && selectedOriginSuggestion.MapboxId == e.Features.FirstOrDefault()?.Properties.MapboxId)
            {
                retrieveResponseOrigin = e;
            }

            if (selectedDestinationSuggestion != null
                && selectedDestinationSuggestion.MapboxId == e.Features.FirstOrDefault()?.Properties.MapboxId)
            {
                retrieveResponseDestination = e;
            }

            if (retrieveResponseOrigin != null && retrieveResponseDestination != null)
            {
                var originFeature = retrieveResponseOrigin.Features.First();
                Vector2d origin = new(originFeature.Geometry.Coordinates[1], originFeature.Geometry.Coordinates[0]);

                var destinationFeature = retrieveResponseDestination.Features.First();
                Vector2d destination = new(destinationFeature.Geometry.Coordinates[1], destinationFeature.Geometry.Coordinates[0]);

                mapService.GetDirections(origin, destination);
            }
        }
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

        if (!string.IsNullOrEmpty(searchBox.Text))
        {
            if (searchBox == OriginSearchBar)
		    {
                mapService.GetLocationSuggestion(searchBox.Text, locationPoint);
                currentTargetListView = OriginResultView;
            }

		    if (searchBox == DestinationSearchBar)
		    {
                mapService.GetLocationSuggestion(searchBox.Text);
                currentTargetListView = DestinationResultView;
            }
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
            mapService.GetSuggestionRetrieve(suggestionObject);
        }
    }
}