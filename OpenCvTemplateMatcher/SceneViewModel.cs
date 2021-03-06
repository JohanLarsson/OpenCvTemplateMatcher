namespace OpenCvTemplateMatcher
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;
    using Ookii.Dialogs.Wpf;
    using OpenCvSharp;

    public sealed class SceneViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable disposable;
        private readonly ViewModel viewModel;

        private Mat descriptors;
        private Mat image;
        private string fileName;
        private TimeSpan elapsed;
        private Exception exception;
        private bool disposed;

        public SceneViewModel(ViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.OpenSceneCommand = new RelayCommand(this.OpenScene);
            this.disposable = Observable.Merge(
                    viewModel.ObservePropertyChangedSlim(x => x.ImageMode, signalInitial: false),
                    viewModel.ObservePropertyChangedSlim(x => x.Surf.Surf, signalInitial: false))
                .Subscribe(_ => this.Update());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenSceneCommand { get; }

        public ObservableBatchCollection<KeyPointViewModel> KeyPoints { get; } = new ObservableBatchCollection<KeyPointViewModel>();

        public Mat Descriptors
        {
            get => this.descriptors;

            private set
            {
                if (ReferenceEquals(value, this.descriptors))
                {
                    return;
                }

                this.descriptors = value;
                this.OnPropertyChanged();
            }
        }

        public Mat Image
        {
            get => this.image;

            private set
            {
                if (ReferenceEquals(value, this.image))
                {
                    return;
                }

                this.image = value;
                this.OnPropertyChanged();
            }
        }

        public TimeSpan Elapsed
        {
            get => this.elapsed;

            private set
            {
                if (value == this.elapsed)
                {
                    return;
                }

                this.elapsed = value;
                this.OnPropertyChanged();
            }
        }

        public Exception Exception
        {
            get => this.exception;

            private set
            {
                if (ReferenceEquals(value, this.exception))
                {
                    return;
                }

                this.exception = value;
                this.OnPropertyChanged();
            }
        }

        public string FileName
        {
            get => this.fileName;

            set
            {
                if (value == this.fileName)
                {
                    return;
                }

                this.fileName = value;
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
            this.disposable?.Dispose();
            this.descriptors?.Dispose();
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

        private void OpenScene()
        {
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                var fn = dialog.FileName;
                this.FileName = fn;
                this.image?.Dispose();
                this.Image = string.IsNullOrEmpty(fn)
                    ? null
                    : new Mat(fn, this.viewModel.ImageMode);

                this.Update();
            }
        }

        private void Update()
        {
            this.Exception = null;
            this.descriptors?.Dispose();
            if (this.image == null)
            {
                this.KeyPoints.Clear();
                this.Descriptors = null;
                return;
            }

            try
            {
                var ds = new Mat();
                var sw = Stopwatch.StartNew();
                this.viewModel.Surf.Surf.DetectAndCompute(this.image, null, out KeyPoint[] kps, ds);
                this.Elapsed = sw.Elapsed;
                this.Descriptors = ds;
                this.KeyPoints.Clear();
                this.KeyPoints.AddRange(kps.Select(kp => new KeyPointViewModel(kp)));
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }
    }
}