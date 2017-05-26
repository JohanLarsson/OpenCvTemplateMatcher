namespace OpenCvTemplateMatcher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Media.Imaging;
    using Gu.Reactive;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    public sealed class ViewModel : INotifyPropertyChanged, IDisposable
    {
        private ImreadModes imageMode = ImreadModes.Color;
        private BitmapSource overlay;
        private IReadOnlyList<DMatch> matches = new DMatch[0];
        private HomographyMethods homographyMethod = HomographyMethods.None;
        private Exception exception;
        private bool disposed;

        public ViewModel()
        {
            this.Model = new ModelViewModel(this);
            this.Scene = new SceneViewModel(this);
            Observable.Merge(
                          this.Surf.ObservePropertyChangedSlim(x => x.Surf),
                          this.BfMatcher.ObservePropertyChangedSlim(x => x.Matcher),
                          this.Model.ObservePropertyChangedSlim(x => x.Descriptors),
                          this.Scene.ObservePropertyChangedSlim(x => x.Descriptors),
                          this.ObservePropertyChangedSlim(nameof(this.HomographyMethod)))
                      .Throttle(TimeSpan.FromMilliseconds(10), DispatcherScheduler.Current)
                      .Subscribe(_ => this.Update());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SurfViewModel Surf { get; } = new SurfViewModel();

        public ModelViewModel Model { get; }

        public SceneViewModel Scene { get; }

        public BfMatcherViewModel BfMatcher { get; } = new BfMatcherViewModel();

        public ImreadModes ImageMode
        {
            get => this.imageMode;

            set
            {
                if (value == this.imageMode)
                {
                    return;
                }

                this.imageMode = value;
                this.OnPropertyChanged();
            }
        }

        public BitmapSource Overlay
        {
            get => this.overlay;

            private set
            {
                if (ReferenceEquals(value, this.overlay))
                {
                    return;
                }

                this.overlay = value;
                this.OnPropertyChanged();
            }
        }

        public IReadOnlyList<DMatch> Matches
        {
            get => this.matches;

            private set
            {
                if (ReferenceEquals(value, this.matches))
                {
                    return;
                }

                this.matches = value;
                this.OnPropertyChanged();
            }
        }

        public HomographyMethods HomographyMethod
        {
            get => this.homographyMethod;

            set
            {
                if (value == this.homographyMethod)
                {
                    return;
                }

                this.homographyMethod = value;
                this.OnPropertyChanged();
            }
        }

        public Exception Exception
        {
            get => this.exception;

            set
            {
                if (ReferenceEquals(value, this.exception))
                {
                    return;
                }

                this.exception = value;
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
            this.Surf.Dispose();
            this.BfMatcher.Dispose();
            this.Model.Dispose();
            this.Scene.Dispose();
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

        private void Update()
        {
            if (this.Model.Descriptors == null ||
                this.Scene.Descriptors == null)
            {
                this.Overlay = null;
                this.Matches = new DMatch[0];
                return;
            }

            try
            {
                this.Exception = null;
                this.Matches = this.BfMatcher.Matcher.Match(this.Scene.Descriptors, this.Model.Descriptors);
                var goodMatches = this.matches.Where(m => m.Distance < 0.2).ToArray();

                this.FindAndApplyHomography(
                    goodMatches.Select(m => this.Model.KeyPoints[m.TrainIdx].Pt),
                    goodMatches.Select(m => this.Scene.KeyPoints[m.QueryIdx].Pt),
                    this.Model.Image,
                    this.Model.Mask,
                    this.Scene.Image);
            }
            catch (Exception e)
            {
                this.Exception = e;
            }
        }

        private void FindAndApplyAffine(IEnumerable<Point2f> srcPoints, IEnumerable<Point2f> dstPoints, Mat model, Mat mask, Mat scene)
        {
            using (var src = InputArray.Create(srcPoints))
            {
                using (var dst = InputArray.Create(dstPoints))
                {
                    //// http://docs.opencv.org/3.0-beta/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html#decomposehomographymat
                    using (var transform = Cv2.GetAffineTransform(src, dst))
                    {
                        using (var tmp = scene.Overlay())
                        {
                            if (mask != null)
                            {
                                model.CopyTo(model, mask);
                                Cv2.WarpAffine(model, tmp, transform, tmp.Size());
                            }
                            else
                            {
                                Cv2.WarpAffine(model, tmp, transform, tmp.Size());
                            }

                            this.Overlay = tmp.ToBitmapSource();
                        }
                    }
                }
            }
        }

        private void FindAndApplyHomography(IEnumerable<Point2f> srcPoints, IEnumerable<Point2f> dstPoints, Mat model, Mat mask, Mat scene)
        {
            using (var src = InputArray.Create(srcPoints))
            {
                using (var dst = InputArray.Create(dstPoints))
                {
                    //// http://docs.opencv.org/3.0-beta/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html#decomposehomographymat
                    using (var homo = Cv2.FindHomography(
                        src,
                        dst,
                        this.homographyMethod))
                    {
                        using (var tmp = scene.Overlay())
                        {
                            if (mask != null)
                            {
                                model.CopyTo(model, mask);
                                Cv2.WarpPerspective(model, tmp, homo, tmp.Size());
                            }
                            else
                            {
                                Cv2.WarpPerspective(model, tmp, homo, tmp.Size());
                            }

                            this.Overlay = tmp.ToBitmapSource();
                        }
                    }
                }
            }
        }
    }
}
