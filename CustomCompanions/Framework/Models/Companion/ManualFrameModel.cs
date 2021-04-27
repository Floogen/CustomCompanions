
namespace CustomCompanions.Framework.Models.Companion
{
    public class ManualFrameModel
    {
        public int Frame { get; set; }
        public int Duration { get; set; }

        public override string ToString()
        {
            return $"[Frame: {Frame} | Duration: {Duration}]";
        }
    }
}
