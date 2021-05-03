namespace CustomCompanions.Framework.Models.Companion
{
    public class ShadowModel
    {
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float Scale { get; set; }
        public int Alpha { get; set; } = 255;

        public override string ToString()
        {
            return $"[Offset: {OffsetX}, {OffsetY} | Scale: {Scale} | Alpha: {Alpha}]";
        }
    }
}
