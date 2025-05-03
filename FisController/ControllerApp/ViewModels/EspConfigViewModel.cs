using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ControllerApp.ViewModels
{
    public class EspConfigViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private decimal fineTuneRatio = 0.9250m;
        private int tyreWidth;
        private int tyreAspectRatio;
        private int tyreDiameter;
        private bool speedDisplayEnabled = true;

        public decimal FineTuneRatio
        {
            get => fineTuneRatio;
            set
            {
                if (fineTuneRatio != value)
                {
                    fineTuneRatio = value;
                    OnPropertyChanged(nameof(FineTuneRatio));
                }
            }
        }

        public int TyreWidth
        {
            get => tyreWidth;
            set
            {
                if (tyreWidth != value)
                {
                    tyreWidth = value;
                    OnPropertyChanged(nameof(TyreWidth));
                }
            }
        }

        public int TyreAspectRatio
        {
            get => tyreAspectRatio;
            set
            {
                if (tyreAspectRatio != value)
                {
                    tyreAspectRatio = value;
                    OnPropertyChanged(nameof(TyreAspectRatio));
                }
            }
        }

        public int TyreDiameter
        {
            get => tyreDiameter;
            set
            {
                if (tyreDiameter != value)
                {
                    tyreDiameter = value;
                    OnPropertyChanged(nameof(TyreDiameter));
                }
            }
        }

        public bool SpeedDisplayEnabled
        {
            get => speedDisplayEnabled;
            set
            {
                if (speedDisplayEnabled != value)
                {
                    speedDisplayEnabled = value;
                    OnPropertyChanged(nameof(SpeedDisplayEnabled));
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
