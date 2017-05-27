namespace OpenCvTemplateMatcher
{
    using OpenCvSharp;

    public class DMatchViewModel
    {
        public DMatchViewModel(DMatch match)
        {
            this.Match = match;
        }

        public DMatch Match { get; }

        public int ImgIdx => this.Match.ImgIdx;

        public int QueryIdx => this.Match.QueryIdx;

        public int TrainIdx => this.Match.TrainIdx;

        public float Distance => this.Match.Distance;
    }
}
