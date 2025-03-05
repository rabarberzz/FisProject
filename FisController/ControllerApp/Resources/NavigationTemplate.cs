using System.Globalization;
using System.Text;

namespace ControllerApp.Resources
{
    public class NavigationTemplate
    {
        private string currentAddress = "null";
        private string nextTurnDescriptor = "null";
        public string CurrentAddress
        {
            get => RemoveDiacritics(currentAddress).ToUpper();
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

        public string GetArrivalTimeString => $"{ArrivalTime.ToString("HH:mm", CultureInfo.InvariantCulture)}";
        public string GetTotalDistanceString => $"{TotalDistance.ToString("F0", CultureInfo.InvariantCulture)}\nKM";
        public string GetDistanceToNextTurnString => $"{DistanceToNextTurn.ToString("F1", CultureInfo.InvariantCulture)}\nKM";

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
