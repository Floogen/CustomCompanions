using System.Collections.Generic;

namespace CustomCompanions.Framework.Models.Companion
{
    public class IdleAnimationModel
    {
        public int StartingFrame { get; set; } = -1;
        public int NumberOfFrames { get; set; } = -1;
        public int Duration { get; set; } = -1;
        public List<ManualFrameModel> ManualFrames { get; set; }

        public override string ToString()
        {
            return $"[StartingFrame: {StartingFrame} | NumberOfFrames: {NumberOfFrames} | Duration: {Duration} | ManualFrames: {(ManualFrames is null ? null : string.Join(",", ManualFrames))}]";
        }
    }
}
