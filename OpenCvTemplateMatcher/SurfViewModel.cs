namespace OpenCvTemplateMatcher
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using OpenCvSharp.XFeatures2D;

    public sealed class SurfViewModel : INotifyPropertyChanged, IDisposable
    {
        private int hessianThreshold = 1000;
        private int octaves = 4;
        private int octaveLayers = 2;
        private bool extended = true;
        private bool upright;
        private SURF surf;
        private bool disposed;

        public SurfViewModel()
        {
            this.UpdateSurf();
        }

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
                this.UpdateSurf();
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
                this.UpdateSurf();
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
                this.UpdateSurf();
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
                this.UpdateSurf();
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
                this.UpdateSurf();
            }
        }

        public SURF Surf
        {
            get => this.surf;

            private set
            {
                if (ReferenceEquals(value, this.surf))
                {
                    return;
                }

                this.surf = value;
                this.OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.surf?.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        private void UpdateSurf()
        {
            this.surf?.Dispose();
            this.Surf = SURF.Create(
                this.hessianThreshold,
                this.octaves,
                this.octaveLayers,
                this.extended,
                this.upright);
        }
    }
}