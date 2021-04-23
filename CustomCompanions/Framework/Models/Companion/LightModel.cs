namespace CustomCompanions.Framework.Models.Companion
{
    public class LightModel
    {
        public int[] Color { get; set; }
        public float Radius { get; set; }
        public float PulseInterval { get; set; }
        public int PulseSpeed { get; set; }

        public override string ToString()
        {
            return $"[Color: {string.Join(",", Color)} | Radius: {Radius} | PulseInterval: {PulseInterval} | PulseSpeed: {PulseSpeed}]";
        }
    }
}
