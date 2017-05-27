namespace OpenCvTemplateMatcher
{
    using OpenCvSharp;

    public class KeyPointViewModel
    {
        public KeyPointViewModel(KeyPoint keyPoint)
        {
            this.KeyPoint = keyPoint;
        }

        public KeyPoint KeyPoint { get; }

        public Point2f Point => this.KeyPoint.Pt;

        public float X => this.Point.X;

        public float Y => this.Point.Y;

        public float Angle => this.KeyPoint.Angle;

        public float Size => this.KeyPoint.Size;

        public float Response => this.KeyPoint.Response;

        public int Octave => this.KeyPoint.Octave;

        public int ClassId => this.KeyPoint.ClassId;
    }
}