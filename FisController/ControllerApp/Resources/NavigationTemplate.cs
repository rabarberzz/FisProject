using System.Globalization;

namespace ControllerApp.Resources
{
    public class NavigationTemplate
    {
        private string currentAddress = "null";
        private string nextTurnDescriptor = "null";
        public string CurrentAddress
        {
            get => currentAddress.ToUpper();
            set => currentAddress = value;
        }
        public TimeOnly ArrivalTime { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal DistanceToNextTurn { get; set; }
        public string NextTurnDescriptor
        {
            get => nextTurnDescriptor.ToUpper();
            set => nextTurnDescriptor = value;
        }
        public string DirectionsIcon { get; set; } = "\x34\x74\x34";

        public string GetArrivalTimeString => $"{ArrivalTime.ToString("hh:mm")}";
        public string GetTotalDistanceString => $"{TotalDistance.ToString("F0", CultureInfo.InvariantCulture)}\nKM";
        public string GetDistanceToNextTurnString => $"{DistanceToNextTurn.ToString("F1", CultureInfo.InvariantCulture)}\nKM";
    }
}
