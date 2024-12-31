using ControllerApp.Services;
using Mapbox.Directions;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Color = Mapsui.Styles.Color;
using Timer = System.Timers.Timer;

namespace ControllerApp
{
    public partial class MainPage : ContentPage
    {
        private MyLocationLayer locationLayer;
        private Timer locationUpdateTimer;
        private MapControl mapControl;
        private MapService mapService;

        public MainPage()
        {
            InitializeComponent();
            InitializeMap();
            mapService = new MapService();
            mapService.DirectionsResponseReceived += OnDirectionsReceived;
            mapService.RequestFailed += OnHttpRequestFailed;

#if WINDOWS
            var access_token = Environment.GetEnvironmentVariable("MAPBOX_ACCESS_TOKEN");
            if (!string.IsNullOrEmpty(access_token))
            {
                var configService = IPlatformApplication.Current?.Services.GetService<IConfigurationService>();
                configService?.SetAccessToken(access_token);
            }
#endif
        }

        private async void InitializeMap()
        {
            mapControl = new MapControl();
            mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            var currentLocation = await GetCurrentLocationAsync();
            if (currentLocation != null && mapControl.Map != null)
            {
                locationLayer = new MyLocationLayer(mapControl.Map, currentLocation);
                mapControl.Map.Layers.Add(locationLayer);

                StartLocationUpdates();
            }

            mapControlElement.Content = mapControl; // Assign mapControl to mapControlElement
        }

        private async Task<MPoint> GetCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    var locationMPoint = new MPoint(location.Longitude, location.Latitude);
                    return Mapsui.Projections.SphericalMercator.FromLonLat(locationMPoint);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }

            return null;
        }

        private async Task UpdateLocation()
        {
            var currentLocation = await GetCurrentLocationAsync();
            if (currentLocation != null && mapControl.Map != null)
            {
                locationLayer.UpdateMyLocation(currentLocation);
            }
        }

        private void StartLocationUpdates()
        {
            locationUpdateTimer = new Timer(2000);
            locationUpdateTimer.Elapsed += async (sender, e) => await UpdateLocation();
            locationUpdateTimer.Start();
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            mapService.GetDirections();
        }

        private void OnDirectionsReceived(object? sender, DirectionsResponse response)
        {
            var directions = response;
            if (directions.Code != null)
            {
                responseEntry.Text = directions.Code;
                var features = PointsFromDirectionsResponse(directions);
                var layer = CreatePointLayer(features);
                mapControl.Map.Info += MapOnInfo;
                mapControl.Map?.Widgets.Add(new MapInfoWidget(mapControl.Map));
                mapControl.Map?.Layers.Add(layer);

            }
        }

        private void OnHttpRequestFailed(object? sender, Exception e)
        {
            responseEntry.Text = e.Message;
        }

        private static MemoryLayer CreatePointLayer(IEnumerable<Mapsui.IFeature> features)
        {
            return new MemoryLayer
            {
                Name = "Points",
                IsMapInfoLayer = true,
                Features = features,
                Style = Mapsui.Styles.SymbolStyles.CreatePinStyle(),
            };
        }

        private static void MapOnInfo(object? sender, MapInfoEventArgs e)
        {
            var calloutStyle = e.MapInfo?.Feature?.Styles.Where(s => s is CalloutStyle).Cast<CalloutStyle>().FirstOrDefault();
            if (calloutStyle != null)
            {
                calloutStyle.Enabled = !calloutStyle.Enabled;
                e.MapInfo?.Layer?.DataHasChanged();
            }
        }

        private static IEnumerable<Mapsui.IFeature> PointsFromDirectionsResponse(DirectionsResponse directions)
        {
            var steps = directions.Routes.FirstOrDefault()?.Legs.FirstOrDefault()?.Steps;
            var features = new List<Mapsui.IFeature>();

            if (steps != null)
            {
                foreach (var step in steps)
                {
                    var maneuver = step.Maneuver;
                    var tempMPoint = new MPoint(maneuver.Location.y, maneuver.Location.x);
                    var feature = new PointFeature(Mapsui.Projections.SphericalMercator.FromLonLat(tempMPoint));
                    feature["Instruction"] = maneuver.Instruction;
                    feature["Modifier"] = maneuver.Modifier ?? "none";
                    feature["Type"] = maneuver.Type;
                    feature.Styles.Add(CreateCalloutStyle(feature.ToStringOfKeyValuePairs()));
                    features.Add(feature);
                }
            }

            return features;
        }

        private static CalloutStyle CreateCalloutStyle(string content)
        {
            return new CalloutStyle
            {
                Title = content,
                TitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
                TitleFontColor = Color.Gray,
                MaxWidth = 120,
                RectRadius = 10,
                ShadowWidth = 4,
                Enabled = false,
                SymbolOffset = new Offset(0, SymbolStyle.DefaultHeight * 1f)
            };
        }


        private static IStyle CreateLineStringStyle()
        {
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
            return new VectorStyle
            {
                Fill = null,
                Outline = null,
                Line = { Color = Color.BlueViolet, Width = 3 }
            };
#pragma warning restore CS8670 // Object or collection initializer implicitly dereferences possibly null member.
        }
    }
}
