namespace OpenCvTemplateMatcher
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using OpenCvSharp;

    public sealed class BfMatcherViewModel : INotifyPropertyChanged, IDisposable
    {
        private NormTypes normType = NormTypes.L2SQR;
        private bool crossCheck = true;
        private BFMatcher matcher;
        private bool disposed;

        public BfMatcherViewModel()
        {
            this.UpdateMatcher();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NormTypes NormType
        {
            get => this.normType;

            set
            {
                if (value == this.normType)
                {
                    return;
                }

                this.normType = value;
                this.OnPropertyChanged();
                this.UpdateMatcher();
            }
        }

        public bool CrossCheck
        {
            get => this.crossCheck;

            set
            {
                if (value == this.crossCheck)
                {
                    return;
                }

                this.crossCheck = value;
                this.OnPropertyChanged();
                this.UpdateMatcher();
            }
        }

        public BFMatcher Matcher
        {
            get => this.matcher;

            private set
            {
                if (ReferenceEquals(value, this.matcher))
                {
                    return;
                }

                this.matcher = value;
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
            this.matcher?.Dispose();
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

        private void UpdateMatcher()
        {
            this.matcher?.Dispose();
            this.Matcher = new BFMatcher(this.normType, this.crossCheck);
        }
    }
}