using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string GetTotalDistanceString => $"{TotalDistance}\nKM";
        // TODO: output point (.) instead of comma from this next turn property
        public string GetDistanceToNextTurnString => $"{DistanceToNextTurn}\nKM";
    }
}
