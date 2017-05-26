namespace OpenCvTemplateMatcher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;
    using Ookii.Dialogs.Wpf;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    public class ViewModel : INotifyPropertyChanged
    {
        private string modelFile;
        private string modelMaskFile;
        private string sceneFile;
        private ImreadModes imageMode = ImreadModes.Color;
        private BitmapSource overlay;
        private IReadOnlyList<KeyPoint> modelKeyPoints = new KeyPoint[0];
        private IReadOnlyList<KeyPoint> sceneKeyPoints = new KeyPoint[0];
        private IReadOnlyList<DMatch> matches = new DMatch[0];
        private HomographyMethods homographyMethod = HomographyMethods.None;
        private Exception exception;

        public ViewModel()
        {
            this.OpenModelCommand = new RelayCommand(this.OpenModel);
            this.OpenModelMaskCommand = new RelayCommand(this.OpenModelMask);
            this.OpenSceneCommand = new RelayCommand(this.OpenScene);

            Observable.Merge(
                          this.Surf.ObservePropertyChangedSlim(),
                          this.BfMatcher.ObservePropertyChangedSlim(),
                          this.ObservePropertyChangedSlim(nameof(this.ModelFile)),
                          this.ObservePropertyChangedSlim(nameof(this.ModelMaskFile)),
                          this.ObservePropertyChangedSlim(nameof(this.SceneFile)),
                          this.ObservePropertyChangedSlim(nameof(this.ImageMode)),
                          this.ObservePropertyChangedSlim(nameof(this.HomographyMethod)))
                      .Subscribe(_ => this.Update());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenModelCommand { get; }

        public ICommand OpenModelMaskCommand { get; }

        public ICommand OpenSceneCommand { get; }

        public SurfViewModel Surf { get; } = new SurfViewModel();

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

        public string ModelFile
        {
            get => this.modelFile;

            set
            {
                if (value == this.modelFile)
                {
                    return;
                }

                this.modelFile = value;
                this.OnPropertyChanged();
            }
        }

        public string ModelMaskFile
        {
            get => this.modelMaskFile;

            set
            {
                if (value == this.modelMaskFile)
                {
                    return;
                }

                this.modelMaskFile = value;
                this.OnPropertyChanged();
            }
        }

        public string SceneFile
        {
            get => this.sceneFile;

            set
            {
                if (value == this.sceneFile)
                {
                    return;
                }

                this.sceneFile = value;
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

        public IReadOnlyList<KeyPoint> ModelKeyPoints
        {
            get => this.modelKeyPoints;

            private set
            {
                if (ReferenceEquals(value, this.modelKeyPoints))
                {
                    return;
                }

                this.modelKeyPoints = value;
                this.OnPropertyChanged();
            }
        }

        public IReadOnlyList<KeyPoint> SceneKeyPoints
        {
            get => this.sceneKeyPoints;

            private set
            {
                if (ReferenceEquals(value, this.sceneKeyPoints))
                {
                    return;
                }

                this.sceneKeyPoints = value;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenModel()
        {
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                this.ModelFile = dialog.FileName;
            }
        }

        private void OpenModelMask()
        {
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                this.ModelMaskFile = dialog.FileName;
            }
        }

        private void OpenScene()
        {
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                this.SceneFile = dialog.FileName;
            }
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(this.modelFile) ||
                string.IsNullOrEmpty(this.sceneFile))
            {
                this.Overlay = null;
                this.ModelKeyPoints = new KeyPoint[0];
                this.SceneKeyPoints = new KeyPoint[0];
                this.Matches = new DMatch[0];
                return;
            }

            try
            {
                this.Exception = null;
                using (var surf = this.Surf.Create())
                {
                    using (var model = new Mat(this.modelFile, this.imageMode))
                    {
                        using (var md = new Mat())
                        {
                            using (var mask = File.Exists(this.modelMaskFile) ? new Mat(this.modelMaskFile, ImreadModes.GrayScale) : null)
                            {
                                surf.DetectAndCompute(model, mask, out KeyPoint[] mkp, md);
                                this.ModelKeyPoints = mkp;
                                using (var scene = new Mat(this.sceneFile, this.imageMode))
                                {
                                    using (var sd = new Mat())
                                    {
                                        surf.DetectAndCompute(scene, null, out KeyPoint[] skp, sd);
                                        this.SceneKeyPoints = skp;
                                        using (var matcher = this.BfMatcher.Create())
                                        {
                                            this.Matches = matcher.Match(sd, md);
                                            var goodMatches = this.matches.Where(m => m.Distance < 0.2).ToArray();
                                            this.FindAndApplyHomography(
                                                goodMatches.Select(m => mkp[m.TrainIdx].Pt),
                                                goodMatches.Select(m => skp[m.QueryIdx].Pt),
                                                model,
                                                mask,
                                                scene);

                                            ////this.FindAndApplyAffine(
                                            ////    goodMatches.Select(m => mkp[m.TrainIdx].Pt),
                                            ////    goodMatches.Select(m => skp[m.QueryIdx].Pt),
                                            ////    model,
                                            ////    mask,
                                            ////    scene);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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
