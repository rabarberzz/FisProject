using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerApp.Services
{
    public class EspConfigService
    {
        private BleService bleService;
        private String configTemplate = "speed_{0}/ratio_{1}";
        public bool SpeedDisplayEnabled;
        public decimal FineTuneRatio;

        public EspConfigService(BleService bleService)
        {
            this.bleService = bleService;
        }

        public async Task SendConfigToEsp()
        {
            if (FineTuneRatio > 0)
            {
                await bleService.SendConfigBytes(setUpConfigString());
            }
        }

        public async Task<bool> TryGetConfigFromEsp()
        {
            var currentConfig = await bleService.GetConfigBytes();
            if (currentConfig != null)
            {
                applyConfigFromString(currentConfig);
                return true;
            }
            return false;
        }

        public decimal CalculateFineTuneRatio(int width, int aspectRatio, int rimDiameter)
        {
            var pulsesPerRotation = 8;

            var rimDiameterMm = rimDiameter * 25.4m;
            var sidewallHeight = width * (aspectRatio / 100m);

            var totalDiameter = rimDiameterMm + (2 * sidewallHeight);

            var cirumference = totalDiameter * (decimal)Math.PI;

            var distancePerPulse = (cirumference / pulsesPerRotation) / 1000000;

            // return conversion ratio of km/h per pulse
            FineTuneRatio = decimal.Round(distancePerPulse * 3600, 3);
            return FineTuneRatio;
        }

        private string setUpConfigString()
        {
            var configString = string.Format(
                configTemplate, 
                SpeedDisplayEnabled ? "1" : "0", 
                FineTuneRatio.ToString(CultureInfo.InvariantCulture)
            );
            return configString;
        }

        private void applyConfigFromString(string configString)
        {
            if (configString.Length > 0)
            {
                try
                {
                    var enableDecode = configString.Substring(6, 1);
                    var ratioDecode = configString.Substring(14);

                    SpeedDisplayEnabled = enableDecode == "1" ? true : false;

                    FineTuneRatio = decimal.Parse(ratioDecode, CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    throw new FormatException("Invalid config string format", ex);
                }
            }
        }

    }
}
