
namespace CustomCompanions.Framework.Models.Companion
{
    public class ManualFrameModel
    {
        public int Frame { get; set; }
        public int Duration { get; set; }
        public bool Flip { get; set; }

        public override string ToString()
        {
            return $"[Frame: {Frame} | Duration: {Duration}: | Flip: {Flip}]";
        }
    }
}
