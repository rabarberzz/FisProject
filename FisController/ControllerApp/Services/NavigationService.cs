using ControllerApp.Resources;
using Mapbox.Directions;
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
        private BleService bleService;
        private FisNavigationService fisNavigationService;
        private LocationService locationService;
        private MapboxService mapboxService;
        private MapsuiService mapsuiService;

        private DirectionsResponse? currentDirectionsResponse;

        public bool NavigationSessionStarted { get; private set; } = false;

        public NavigationService(BleService bleSvc, 
            FisNavigationService fisNavSvc, LocationService locSvc,
            MapboxService mapBoxSvc, MapsuiService mapsuiSvc)
        {
            bleService = bleSvc;
            fisNavigationService = fisNavSvc;
            locationService = locSvc;
            mapboxService = mapBoxSvc;
            mapsuiService = mapsuiSvc;

            mapboxService.DirectionsResponseReceived += OnDirectionsReceived;

            locationService.LocationUpdated += OnLocationUpdated;
        }
        
        // Public methods
        public void StartNavigation()
        {
            if (currentDirectionsResponse != null)
            {
                fisNavigationService.SetNavigationTemplates(PrepareTemplatesFromDirectionsResponse(currentDirectionsResponse));
                NavigationSessionStarted = true;
            }
        }

        public void StopNavigation()
        {
            NavigationSessionStarted = false;
        }

        // Private methods

        // Event handlers
        private void OnDirectionsReceived(object? sender, DirectionsResponse response)
        {
            if (response.Code != null && mapsuiService != null)
            {
                currentDirectionsResponse = response;
            }
        }

        // On every location update check if the user is close to the next maneuver
        // also check if the user is still on the route.
        // Also update the remaining distances on every location update.
        // Need to handle next maneuver switch over.
        // Dont forget to handle the sending of data to FIS device.
        private void OnLocationUpdated(object? sender, Location location)
        {
            if (NavigationSessionStarted)
            {
                // Update navigation templates
            }
        }

        // Helper methods
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
    }
}
