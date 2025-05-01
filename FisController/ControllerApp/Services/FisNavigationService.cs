using ControllerApp.Resources;

namespace ControllerApp.Services
{
    public class FisNavigationService
    {
        private BleService bleService;
        private NavigationTemplate currentNavigation;
        private List<NavigationTemplate> navigationTemplateList = new List<NavigationTemplate>();
        private static string DataTemplate = "icon_{0}/address_{1}/time_{2}/total_{3}/turn_{4}/exit_{5}/clear_{6}";

        public FisNavigationService(BleService service)
        {
            bleService = service;
            SetMockTemplates();
            currentNavigation = navigationTemplateList[0];
        }

        private void SetMockTemplates()
        {
            var template = new NavigationTemplate
            {
                CurrentAddress = "Barona 12",
                ArrivalTime = new TimeOnly(13, 31),
                TotalDistance = 100,
                DistanceToNextTurn = 15,
                NextTurnDescriptor = "Turn Left",
                DirectionsIcon = DirectionsCodes.LeftTurn
            };

            navigationTemplateList = new List<NavigationTemplate>
            {
                template
            };

            template = new NavigationTemplate
            {
                CurrentAddress = "Ieriku 9",
                ArrivalTime = new TimeOnly(13, 55),
                TotalDistance = 85,
                DistanceToNextTurn = 12.4m,
                NextTurnDescriptor = "Turn right",
                DirectionsIcon = DirectionsCodes.RightTurn
            };

            navigationTemplateList.Add(template);

            template = new NavigationTemplate
            {
                CurrentAddress = "Krasta iela",
                ArrivalTime = new TimeOnly(14, 19),
                TotalDistance = 30,
                DistanceToNextTurn = 0.3m,
                NextTurnDescriptor = "Turn around",
                DirectionsIcon = DirectionsCodes.LeftTurnaround
            };

            navigationTemplateList.Add(template);
        }

        private string SetUpData()
        {
            var data = string.Format
                (
                DataTemplate,
                currentNavigation.DirectionsIcon,
                currentNavigation.CurrentAddress,
                currentNavigation.GetArrivalTimeString,
                currentNavigation.GetTotalDistanceString,
                currentNavigation.GetDistanceToNextTurnString,
                currentNavigation.RoundaboutExit,
                currentNavigation.ClearScreen
                );

            return data;
        }

        private void HandleDistanceChange(decimal remainingDistance, decimal newRemainingDistance)
        {
            //if ((remainingDistance < 100 && previousRemainingDistance >= 100 && previousRemainingDistance != remainingDistance)
            //        || (remainingTotalDistance < 100 && previousTotalDistance >= 100 && remainingTotalDistance != previousTotalDistance))
            //{
            //    fisNavigationService.ClearNaviScreen();
            //}

            //if ((remainingDistance < 10 && previousRemainingDistance >= 10 && previousRemainingDistance != remainingDistance)
            //    && (remainingTotalDistance < 10 && previousTotalDistance >= 10 && remainingTotalDistance != previousTotalDistance))
            //{
            //    fisNavigationService.ClearNaviScreen();
            //}
            currentNavigation.ClearScreen = "false";

            if (remainingDistance >= 100 && newRemainingDistance < 100 && remainingDistance != newRemainingDistance)
            {
                currentNavigation.ClearScreen = "true";
            }

            if (remainingDistance > 10 && newRemainingDistance <= 10 && remainingDistance != newRemainingDistance)
            {
                currentNavigation.ClearScreen = "true";
            }

        }

        public void SetNavigationTemplates(List<NavigationTemplate> templates)
        {
            navigationTemplateList = templates;
        }

        public async Task SendNavigationData()
        {
            await bleService.SendNaviBytes(SetUpData());
        }

        public NavigationTemplate SetUpNextDirection()
        {
            var index = navigationTemplateList.IndexOf(currentNavigation);
            if (index == navigationTemplateList.Count - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
            currentNavigation = navigationTemplateList[index];
            return currentNavigation;
        }

        public void SetCurrentNavigation(NavigationTemplate template)
        {
            ClearNaviScreen();
            currentNavigation = template;
            _ = SendNavigationData();
        }

        public void SetDistanceToManeuver(double distance)
        {
            currentNavigation.DistanceToNextTurn = (decimal)distance;
            _ = SendNavigationData();
        }

        public void SetTotalDistanceLeft(double distance)
        {
            currentNavigation.TotalDistance = (decimal)distance;
            _ = SendNavigationData();
        }

        public void SetRemainingDistances(double maneuver, double total)
        {
            HandleDistanceChange(currentNavigation.DistanceToNextTurn, (decimal)maneuver);
            currentNavigation.DistanceToNextTurn = (decimal)maneuver;

            HandleDistanceChange(currentNavigation.TotalDistance, (decimal)total);
            currentNavigation.TotalDistance = (decimal)total;

            _ = SendNavigationData();
        }

        public async void ClearNaviScreen()
        {
            await bleService.SendNaviClearCommand();
        }

        // debug method
        public NavigationTemplate GetCurrentTemplate()
        {
            return currentNavigation;
        }
    }
}
