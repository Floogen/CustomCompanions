namespace CustomCompanions.Framework.Models.Companion
{
    public class IdleAnimationModel
    {
        public int StartingFrame { get; set; }
        public int NumberOfFrames { get; set; }
        public int Duration { get; set; }

        public override string ToString()
        {
            return $"[StartingFrame: {StartingFrame} | NumberOfFrames: {NumberOfFrames} | Duration: {Duration}]";
        }
    }
}
