namespace CustomCompanions.Framework.Models.Companion
{
    public class PortraitModel
    {
        public string PortraitSheetPath { get; set; }
        public int FrameSizeWidth { get; set; } = 64;
        public int FrameSizeHeight { get; set; } = 64;
        public int FrameIndex { get; set; }
        public string PortraitDisplayName { get; set; }

        public override string ToString()
        {
            return $"[FrameIndex: {FrameIndex} | FrameSize: {FrameSizeWidth}x{FrameSizeHeight} | PortraitDisplayName: {PortraitDisplayName} | PortraitSheetPath: {PortraitSheetPath}]";
        }
    }
}
