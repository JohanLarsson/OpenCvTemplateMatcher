﻿namespace OpenCvTemplateMatcher
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;
    using Ookii.Dialogs.Wpf;
    using OpenCvSharp;

    public sealed class ModelViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ViewModel viewModel;
        private readonly IDisposable disposable;

        private string imageFileName;
        private Mat descriptors;
        private Mat image;
        private Mat mask;
        private string maskFileName;
        private bool disposed;

        public ModelViewModel(ViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.OpenModelCommand = new RelayCommand(this.OpenModel);
            this.OpenModelMaskCommand = new RelayCommand(this.OpenModelMask);
            this.disposable = viewModel.ObservePropertyChangedSlim(x => x.ImageMode, signalInitial: false)
                                       .Subscribe(_ => this.Update());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenModelCommand { get; }

        public ICommand OpenModelMaskCommand { get; }

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

        public Mat Mask
        {
            get => this.mask;

            private set
            {
                if (ReferenceEquals(value, this.mask))
                {
                    return;
                }

                this.mask = value;
                this.OnPropertyChanged();
            }
        }

        public string ImageFileName
        {
            get => this.imageFileName;

            set
            {
                if (value == this.imageFileName)
                {
                    return;
                }

                this.imageFileName = value;
                this.OnPropertyChanged();
            }
        }

        public string MaskFileName
        {
            get => this.maskFileName;

            set
            {
                if (value == this.maskFileName)
                {
                    return;
                }

                this.maskFileName = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableBatchCollection<KeyPointViewModel> KeyPoints { get; } = new ObservableBatchCollection<KeyPointViewModel>();

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.descriptors?.Dispose();
            this.image?.Dispose();
            this.mask?.Dispose();
            this.disposable?.Dispose();
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

        private void OpenModel()
        {
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                this.ImageFileName = fileName;
                this.image?.Dispose();
                this.Image = string.IsNullOrEmpty(fileName)
                    ? null
                    : new Mat(this.imageFileName, this.viewModel.ImageMode);
                this.Update();
            }
        }

        private void OpenModelMask()
        {
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                this.MaskFileName = fileName;
                this.mask?.Dispose();
                this.Mask = string.IsNullOrEmpty(fileName)
                    ? null
                    : new Mat(fileName, ImreadModes.GrayScale);
                this.Update();
            }
        }

        private void Update()
        {
            this.descriptors?.Dispose();
            if (this.image == null)
            {
                this.KeyPoints.Clear();
                this.Descriptors = null;
                return;
            }

            var ds = new Mat();
            this.viewModel.Surf.Surf.DetectAndCompute(this.image, this.mask, out KeyPoint[] kps, ds);
            this.Descriptors = ds;
            this.KeyPoints.Clear();
            this.KeyPoints.AddRange(kps.Select(kp => new KeyPointViewModel(kp)));
        }
    }
}
