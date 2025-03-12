using Mapbox.Directions;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using NetTopologySuite.Geometries;
using Color = Mapsui.Styles.Color;

namespace ControllerApp.Services
{
    public class MapsuiService
    {
        public MapControl MapControl { get; private set; }
        public MyLocationLayer LocationLayer { get; private set; }
        private Coordinate[]? loadedLinestringCoordinates;
        private DirectionsResponse? directionsResponseLocal;

        public MapsuiService()
        {
            MapControl = new MapControl();
            MapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            LocationLayer = new MyLocationLayer(MapControl.Map);
            MapControl.Map.Layers.Add(LocationLayer);
        }

        public void SetupPointsOnMap(DirectionsResponse directions)
        {
            directionsResponseLocal = directions;
            if (MapControl != null && MapControl.Map != null)
            {
                var features = ManeuverPointsFromDirectionsResponse(directions);
                var layer = CreatePointLayer(features);
                MapControl.Map.Layers.Remove(x => x.Name == "Points");
                MapControl.Map.Info += MapOnInfo;
                MapControl.Map.Widgets.Add(new MapInfoWidget(MapControl.Map));
                MapControl.Map.Layers.Add(layer);
            }
        }

        public void SetupLineOnMap(DirectionsResponse directions)
        {
            if (MapControl != null && MapControl.Map != null)
            {
                MapControl.Map.Layers.Remove(x => x.Name == "Line");
                var feature = GeometryFeatureFromDirectionsResponse(directions);
                var layer = CreateLineLayer(feature, CreateLineStringStyle());
                MapControl.Map.Layers.Add(layer);
            }
        }

        // gets the closest point coordinates relative to the input point from the loaded geometry linestring path
        public Coordinate? GetClosestGeometryPointFromCoordinates(MPoint locationPoint)
        {
            if (loadedLinestringCoordinates != null && loadedLinestringCoordinates.Length > 0)
            {
                var locationCoordinate = locationPoint.ToCoordinate();
                var closestPointDistance = loadedLinestringCoordinates.Min(c => c.Distance(locationCoordinate));
                var minIndex = loadedLinestringCoordinates
                    .Select((c, index) => new { Coordinate = c, Index = index })
                    .First(x => x.Coordinate.Distance(locationCoordinate) == closestPointDistance)
                    .Index;

                return loadedLinestringCoordinates[minIndex];
            }
            return null;
        }

        // get the index of the closest point in the geometry linestring path
        public int GetClosestGeometryPointIndexFromCoordinates(MPoint locationPoint)
        {
            if (loadedLinestringCoordinates != null && loadedLinestringCoordinates.Length > 0)
            {
                var locationCoordinate = locationPoint.ToCoordinate();
                var closestPointDistance = loadedLinestringCoordinates.Min(c => c.Distance(locationCoordinate));
                var minIndex = loadedLinestringCoordinates
                    .Select((c, index) => new { Coordinate = c, Index = index })
                    .First(x => x.Coordinate.Distance(locationCoordinate) == closestPointDistance)
                    .Index;

                return minIndex;
            }
            return -1;
        }

        // this successfully calculates the distance and corresponds to the distance annotation in the directions response
        public void CalculateDistanceBetweenPointsUsing()
        {
            if (loadedLinestringCoordinates != null && loadedLinestringCoordinates.Length > 0)
            {
                var targetIndex = 5;
                var routes = directionsResponseLocal?.Routes.FirstOrDefault();
                var distances = routes?.Legs.FirstOrDefault()?.Annotation?.Distance;
                var start = routes?.Geometry.FirstOrDefault();
                var targetStep = routes?.Legs.FirstOrDefault()?.Steps[targetIndex];
                var target = targetStep?.Maneuver.Location;

                if (start != null && target != null)
                {
                    MPoint startCoordinate = SphericalMercator.FromLonLat(start.Value.y, start.Value.x).ToMPoint();

                    MPoint targetCoordinate = SphericalMercator.FromLonLat(target.Value.y, target.Value.x).ToMPoint();

                    if (loadedLinestringCoordinates != null && loadedLinestringCoordinates.Length > 0)
                    {
                        var indexOfStart = GetClosestGeometryPointIndexFromCoordinates(startCoordinate);

                        var indexOfTarget = GetClosestGeometryPointIndexFromCoordinates(targetCoordinate);

                        if (distances != null)
                        {
                            var distanceDistinct = distances.Skip(indexOfStart).Take(indexOfTarget - indexOfStart).Sum();
                            var compareDistance = routes?.Legs.FirstOrDefault()?.Steps.GetRange(0, targetIndex).Sum(x => x.Distance);
                            if (compareDistance != null)
                            {
                                var comparison = double.Round(distanceDistinct) == double.Round((double)compareDistance);
                            }
                        }
                    }
                }

            }
        }

        // this calculates the distance between the current location and the next maneuver point straight without following the path
        public void CalculateDistanceStraightToPoint(MPoint locationPoint)
        {
            if (loadedLinestringCoordinates != null && loadedLinestringCoordinates.Length > 0)
            {
                var targetIndex = 0;
                var routes = directionsResponseLocal?.Routes.FirstOrDefault();
                var targetStep = routes?.Legs.FirstOrDefault()?.Steps[targetIndex];
                var target = targetStep?.Maneuver.Location;
                if (target != null)
                {
                    MPoint targetCoordinate = SphericalMercator.FromLonLat(target.Value.y, target.Value.x).ToMPoint();

                    if (loadedLinestringCoordinates != null && loadedLinestringCoordinates.Length > 0)
                    {
                        var locationCoordinate = locationPoint.ToCoordinate();
                        var distance = locationCoordinate.Distance(targetCoordinate.ToCoordinate());
                    }
                }
            }
        }

        private GeometryFeature[] GeometryFeatureFromDirectionsResponse(DirectionsResponse directions)
        {
            var coordinates = directions.Routes.FirstOrDefault()?.Geometry;
            var lineString = new LineString(coordinates?.Select(coord => SphericalMercator.FromLonLat(coord.y, coord.x).ToCoordinate()).ToArray());
            loadedLinestringCoordinates = lineString.Coordinates;
            return [new GeometryFeature { Geometry = lineString }];
        }

        private static IEnumerable<Mapsui.IFeature> ManeuverPointsFromDirectionsResponse(DirectionsResponse directions)
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
                    feature["Exit"] = maneuver.Exit;
                    feature.Styles.Add(CreateCalloutStyle(feature.ToStringOfKeyValuePairs()));
                    features.Add(feature);
                }
            }

            return features;
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

        private static ILayer CreateLineLayer(GeometryFeature[] geometry, IStyle? style = null)
        {
            var layer = new MemoryLayer
            {
                Name = "Line",
                Features = geometry,
                Style = style,
            };

            return layer;
        }

        private static MemoryLayer CreatePointLayer(IEnumerable<Mapsui.IFeature> features)
        {
            return new MemoryLayer
            {
                Name = "Points",
                IsMapInfoLayer = true,
                Features = features,
                Style = Mapsui.Styles.SymbolStyles.CreatePinStyle()
            };
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
            var line = new Pen(Color.BlueViolet, 3);
            var style = new VectorStyle
            {
                Fill = null,
                Outline = null,
                Line = line
            };

            return style;
        }
    }
}
