namespace CustomCompanions.Framework.Models.Companion
{
    public class LightModel
    {
        public int[] Color { get; set; }
        public float Radius { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public int PulseSpeed { get; set; }
        public float PulseMinRadius { get; set; }

        public override string ToString()
        {
            return $"[Color: {string.Join(",", Color)} | Radius: {Radius} | Offset: {OffsetX}, {OffsetY} | PulseSpeed: {PulseSpeed} | PulseMinRadius: {PulseMinRadius}]";
        }
    }
}
