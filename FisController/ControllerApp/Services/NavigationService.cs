using ControllerApp.ManeuverHelper;
using ControllerApp.Resources;
using Mapbox.Directions;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Notes
// This service is responsible for handling navigation data and sending it to the FIS device.
// Service should be aware of the current maneuver and constantly monitor the location of the user 
// and the relativity to the next maneuver.
// It should handle the switch over to the next maneuver when the current one is reached.
// It should monitor if the user does not stray from the route and notify the user if they do
// as well as reroute if necessary.


namespace ControllerApp.Services
{
    public class NavigationService
    {
        private FisNavigationService fisNavigationService;
        private LocationService locationService;
        private MapboxService mapboxService;
        private MapsuiService mapsuiService;

        private DirectionsResponse? currentDirectionsResponse;
        private Dictionary<Step, NavigationTemplate>? templateMap;
        private Maneuver? currentManeuver;
        private double remainingDistance;
        private double previousRemainingDistance;
        private double previousTotalDistance;
        private bool followingRoute = true;

        public bool NavigationSessionStarted { get; private set; } = false;

        public NavigationService(FisNavigationService fisNavSvc, LocationService locSvc,
            MapboxService mapBoxSvc, MapsuiService mapsuiSvc)
        {
            fisNavigationService = fisNavSvc;
            locationService = locSvc;
            mapboxService = mapBoxSvc;
            mapsuiService = mapsuiSvc;

            mapboxService.DirectionsResponseReceived += OnDirectionsReceived;

            locationService.LocationUpdated += OnLocationUpdated;
            locationService.LocationUpdatedMapsui += OnLocationUpdated;
        }
        
        // Public methods
        public bool StartNavigation()
        {
            if (currentDirectionsResponse != null && templateMap != null && templateMap.Count > 0)
            {
                fisNavigationService.SetNavigationTemplates(templateMap.Values.ToList());
                currentManeuver = templateMap.Keys.FirstOrDefault()?.Maneuver;
                fisNavigationService.SetCurrentNavigation(templateMap.First().Value);
                NavigationSessionStarted = true;
                return true;
            }
            return false;
        }

        public void StopNavigation()
        {
            NavigationSessionStarted = false;
        }

        public void IncrementManeuver()
        {
            if (templateMap != null && templateMap.Count > 0)
            {
                if (currentManeuver != null)
                {
                    var findStep = templateMap.Keys.FirstOrDefault(x => x.Maneuver == currentManeuver);
                    if (findStep != null)
                    {
                        var currentIndex = templateMap.Keys.ToList().IndexOf(findStep);
                        var nextStep = templateMap.Keys.ElementAt(currentIndex + 1);
                        if (nextStep != templateMap.Last().Key)
                        {
                            currentManeuver = nextStep.Maneuver;
                            fisNavigationService.SetCurrentNavigation(templateMap[nextStep]);
                        }

                        if (nextStep == templateMap.Last().Key)
                        {
                            var lastTemplate = templateMap.Values.Last();
                            fisNavigationService.SetCurrentNavigation(lastTemplate);
                        }
                    }
                }
            }
        }

        // Private methods

        // Event handlers
        private void OnDirectionsReceived(object? sender, DirectionsResponse response)
        {
            if (response.Code != null && mapsuiService != null)
            {
                currentDirectionsResponse = response;
                templateMap = PrepareLegsTemplatesFromDirectionsResponse(response);
            }
        }

        
        private void OnLocationUpdated(object? sender, Location location)
        {
            if (NavigationSessionStarted)
            {
                // Update navigation templates
            }
        }

        // On every location update check if the user is close to the next maneuver
        // also check if the user is still on the route.
        // Also update the remaining distances on every location update.
        // Need to handle next maneuver switch over.
        // Dont forget to handle the sending of data to FIS device.
        private void OnLocationUpdated(object? sender, MPoint location)
        {
            if (NavigationSessionStarted && currentManeuver != null && templateMap != null)
            {
                var currentStep = templateMap.Keys.First(x => x.Maneuver == currentManeuver);
                if (!CheckIfUserIsFollowingRoute(location))
                {
                    followingRoute = CheckIfUserIsFollowingRoute(location);
                    return;
                }
                else if (followingRoute == false 
                    && fisNavigationService.GetCurrentTemplate() != templateMap[currentStep])
                {
                    currentStep = templateMap.Keys.First(x => x.Maneuver == currentManeuver);
                    fisNavigationService.SetCurrentNavigation(templateMap[currentStep]);
                    followingRoute = CheckIfUserIsFollowingRoute(location);
                    return;
                }

                if (currentManeuver != templateMap.First().Key.Maneuver)
                {
                    remainingDistance = CalculateDistaceToNextManeuver(location);
                }

                if (remainingDistance < 200 && remainingDistance > 0 || currentManeuver == templateMap.First().Key.Maneuver)
                {
                    remainingDistance = CalculateStraightLineDistanceToNextManeuver(location);
                }

                var totalDistance = CalculateDistanceToFinish(location);

                //HandleRemainingDistancesNumberCountChange(remainingDistance / 1000, totalDistance / 1000);

                fisNavigationService.SetRemainingDistances(remainingDistance / 1000, totalDistance / 1000);

                if (remainingDistance < 20 && remainingDistance >=0)
                {
                    IncrementManeuver();
                }

                previousRemainingDistance = remainingDistance / 1000;
                previousTotalDistance = totalDistance / 1000;
            }
        }

        private void HandleRemainingDistancesNumberCountChange(double remainingDistance, double remainingTotalDistance)
        {
            if ((remainingDistance < 100 && previousRemainingDistance >= 100 && previousRemainingDistance != remainingDistance)
                    || (remainingTotalDistance < 100 && previousTotalDistance >= 100 && remainingTotalDistance != previousTotalDistance))
            {
                fisNavigationService.ClearNaviScreen();
            }

            if ((remainingDistance < 10 && previousRemainingDistance >= 10 && previousRemainingDistance != remainingDistance)
                && (remainingTotalDistance < 10 && previousTotalDistance >= 10 && remainingTotalDistance != previousTotalDistance))
            {
                fisNavigationService.ClearNaviScreen();
            }
        }

        private bool CheckIfUserIsFollowingRoute(MPoint currentLocation)
        {
            var closestPoint = mapsuiService.GetClosestGeometryPointFromCoordinates(currentLocation);
            if (closestPoint != null)
            {
                var distance = currentLocation.Distance(closestPoint);
                // I think this should be more than 200, try 300 maybe
                if (distance > 300)
                {
                    var offRouteTemplate = new NavigationTemplate()
                    {
                        CurrentAddress = "Off route",
                        DirectionsIcon = DirectionsCodes.LeftTurnaround,
                    };
                    fisNavigationService.SetCurrentNavigation(offRouteTemplate);
                    return false;
                }
            }
            return true;
        }

        // Distance to next point calculation
        private double CalculateDistaceToNextManeuver(MPoint currentLocation)
        {
            // Calculate distance to next maneuver
            var distances = currentDirectionsResponse?.Routes.FirstOrDefault()?.Legs.FirstOrDefault()?.Annotation?.Distance;
            // Get the closest point in polyline to the current location
            var geometryPointIndexCurrentLocation = mapsuiService.GetClosestGeometryPointIndexFromCoordinates(currentLocation);

            if (currentManeuver != null && distances != null && geometryPointIndexCurrentLocation != -1)
            {
                var targetPoint = SphericalMercator.FromLonLat(currentManeuver.Location.y, currentManeuver.Location.x).ToMPoint();
                var geometryPointIndexManeuver = mapsuiService.GetClosestGeometryPointIndexFromCoordinates(targetPoint);

                if (geometryPointIndexManeuver != -1)
                {
                    var resultDistance = distances.Skip(geometryPointIndexCurrentLocation).Take(geometryPointIndexManeuver - geometryPointIndexCurrentLocation).Sum();

                    return resultDistance;
                }

            }

            return -1;
        }

        private double CalculateStraightLineDistanceToNextManeuver(MPoint currentLocation)
        {
            if (currentManeuver != null)
            {
                var targetLocationMPoint = SphericalMercator.FromLonLat(currentManeuver.Location.y, currentManeuver.Location.x).ToMPoint();

                return currentLocation.Distance(targetLocationMPoint);
            }

            return -1;
        }

        private double CalculateDistanceToFinish(MPoint currentLocation)
        {
            var distances = currentDirectionsResponse?.Routes.FirstOrDefault()?.Legs.FirstOrDefault()?.Annotation?.Distance;
            // Get the closest point in polyline to the current location
            var geometryPointIndexCurrentLocation = mapsuiService.GetClosestGeometryPointIndexFromCoordinates(currentLocation);

            if (currentManeuver != null && distances != null && geometryPointIndexCurrentLocation != -1)
            {
                var targetLocation = templateMap?.Last().Key.Maneuver.Location;
                if (targetLocation != null)
                {
                    var targetPoint = SphericalMercator.FromLonLat(targetLocation.Value.y, targetLocation.Value.x).ToMPoint();
                    var geometryPointIndexManeuver = mapsuiService.GetClosestGeometryPointIndexFromCoordinates(targetPoint);

                    if (geometryPointIndexManeuver != -1)
                    {
                        var resultDistance = distances.Skip(geometryPointIndexCurrentLocation).Sum();

                        return resultDistance;
                    }
                }

            }

            return -1;
        }

        // Helper methods
        private Dictionary<Step, NavigationTemplate> PrepareLegsTemplatesFromDirectionsResponse(DirectionsResponse response)
        {
            var templateMap = new Dictionary<Step, NavigationTemplate>();
            var leg = response.Routes.FirstOrDefault()?.Legs.FirstOrDefault();
            var steps = leg?.Steps;
            if (leg != null)
            {
                if (steps != null)
                {
                    foreach (var step in steps)
                    {
                        var maneuverType = step.Maneuver?.Type ?? "";
                        var maneuverModifier = step.Maneuver?.Modifier ?? "";
                        var roundaboutExit = step.Maneuver?.Exit.ToString() ?? "";
                        var template = new NavigationTemplate
                        {
                            CurrentAddress = step.Name,
                            TotalDistance = CalculateRemainingDistance(step, steps) / 1000,
                            DistanceToNextTurn = (decimal)step.Distance / 1000,
                            ArrivalTime = TimeOnly.FromDateTime(DateTime.Now.AddSeconds(leg.Duration)),
                            DirectionsIcon = ManeuverDirectionCodeMap.GetDirectionCode(maneuverType, maneuverModifier) ?? "\x34\x74\x34",
                            RoundaboutExit = roundaboutExit
                        };

                        templateMap.Add(step, template);
                    }
                }
            }
            return templateMap;
        }

        private List<NavigationTemplate> PrepareTemplatesFromDirectionsResponse(DirectionsResponse response)
        {
            var template = new List<NavigationTemplate>();
            var legs = response.Routes.FirstOrDefault()?.Legs.FirstOrDefault();
            if (legs != null)
            {
                template = mapLegToTemplates(legs);
            }
            return template;
        }

        // Set up initial templates
        private List<NavigationTemplate> mapLegToTemplates(Leg leg)
        {
            var templates = new List<NavigationTemplate>();
            var totalDistance = leg.Distance;

            foreach (var step in leg.Steps)
            {
                templates.Add(new NavigationTemplate
                {
                    CurrentAddress = step.Name,
                    TotalDistance = CalculateRemainingDistance(step, leg.Steps) / 1000,
                    DistanceToNextTurn = (decimal)step.Distance / 1000,
                    ArrivalTime = TimeOnly.FromDateTime(DateTime.Now.AddSeconds(leg.Duration)),
                });
            }

            return templates;
        }

        // Used for setting up the initial templates
        // might be not needed when using real location to calculate remaining distance
        private decimal CalculateRemainingDistance(Step currentStep, List<Step> allSteps)
        {
            decimal distance = 0;
            var remainingSteps = allSteps.GetRange(allSteps.IndexOf(currentStep), allSteps.Count - allSteps.IndexOf(currentStep));
            distance = (decimal)remainingSteps.Sum(x => x.Distance);

            return distance;
        }

        private Dictionary<DirectionsResponse, NavigationTemplate> MapDirectionsToTemplates(List<DirectionsResponse> directions)
        {
            var templateMap = new Dictionary<DirectionsResponse, NavigationTemplate>();
            foreach (var direction in directions)
            {
                templateMap.Add(direction, new NavigationTemplate());
            }
            return templateMap;
        }
    }
}
