namespace OpenCvTemplateMatcher
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using OpenCvSharp.XFeatures2D;

    public class SurfViewModel : INotifyPropertyChanged
    {
        private int hessianThreshold = 1000;
        private int octaves = 4;
        private int octaveLayers = 2;
        private bool extended = true;
        private bool upright;

        public event PropertyChangedEventHandler PropertyChanged;

        public int HessianThreshold
        {
            get => this.hessianThreshold;

            set
            {
                if (value == this.hessianThreshold)
                {
                    return;
                }

                this.hessianThreshold = value;
                this.OnPropertyChanged();
            }
        }

        public int Octaves
        {
            get => this.octaves;

            set
            {
                if (value == this.octaves)
                {
                    return;
                }

                this.octaves = value;
                this.OnPropertyChanged();
            }
        }

        public int OctaveLayers
        {
            get => this.octaveLayers;

            set
            {
                if (value == this.octaveLayers)
                {
                    return;
                }

                this.octaveLayers = value;
                this.OnPropertyChanged();
            }
        }

        public bool Extended
        {
            get => this.extended;

            set
            {
                if (value == this.extended)
                {
                    return;
                }

                this.extended = value;
                this.OnPropertyChanged();
            }
        }

        public bool Upright
        {
            get => this.upright;

            set
            {
                if (value == this.upright)
                {
                    return;
                }

                this.upright = value;
                this.OnPropertyChanged();
            }
        }

        public SURF Create() => SURF.Create(this.hessianThreshold, this.octaves, this.octaveLayers, this.extended, this.upright);

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}