using ControllerApp.SearchBox.Retrieve;
using ControllerApp.SearchBox.Suggestion;
using ControllerApp.Services;
using Mapbox.Directions;
using Mapbox.Utils;
using Mapsui;

namespace ControllerApp;

public partial class NavigationSetup : ContentPage
{
	private MapboxService mapboxService;
    private NavigationService navigationService;

    private ListView? currentTargetListView;
    private SuggestionObject? selectedOriginSuggestion;
    private SuggestionObject? selectedDestinationSuggestion;
    private RetrieveResponse? retrieveResponseOrigin;
    private RetrieveResponse? retrieveResponseDestination;
    private Location? locationPoint;

    public NavigationSetup(MapboxService mapbService, NavigationService navService)
	{
		InitializeComponent();
		this.mapboxService = mapbService;
        this.mapboxService.SuggestionResponseReceived += MapboxService_SuggestionResponseReceived;
        this.mapboxService.RetrieveResponseReceived += MapboxService_RetrieveResponseReceived;
        this.mapboxService.RequestFailed += MapboxService_RequestFailed;

        navigationService = navService;
        navigationService.LocationUpdated += NavigationService_LocationUpdated;
    }

    private void MapboxService_RequestFailed(object? sender, Exception e)
    {
        DisplayAlert("Error", e.Message, "OK");
    }

    private void NavigationService_LocationUpdated(object? sender, Location e)
    {
        locationPoint = e;
    }

    private void MapboxService_RetrieveResponseReceived(object? sender, RetrieveResponse e)
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

                mapboxService.GetDirections(origin, destination);
            }
        }
    }

    private void MapboxService_SuggestionResponseReceived(object? sender, SuggestionResponse e)
    {
        if (currentTargetListView != null && e.Suggestions != null && e.Suggestions.Count > 0)
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
                mapboxService.GetLocationSuggestion(searchBox.Text, locationPoint);
                currentTargetListView = OriginResultView;
            }

		    if (searchBox == DestinationSearchBar)
		    {
                mapboxService.GetLocationSuggestion(searchBox.Text, locationPoint);
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
            mapboxService.GetSuggestionRetrieve(suggestionObject);
        }
    }
}