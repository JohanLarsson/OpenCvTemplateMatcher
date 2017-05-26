namespace OpenCvTemplateMatcher
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using OpenCvSharp;

    public class BfMatcherViewModel : INotifyPropertyChanged
    {
        private NormTypes normType = NormTypes.L2;
        private bool crossCheck;

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
            }
        }

        public BFMatcher Create() => new BFMatcher(this.normType, this.crossCheck);

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}