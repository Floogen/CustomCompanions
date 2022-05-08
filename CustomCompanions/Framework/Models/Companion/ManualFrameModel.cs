
namespace CustomCompanions.Framework.Models.Companion
{
    public class ManualFrameModel
    {
        public int Frame { get; set; }
        public int Duration { get; set; }
        public bool Flip { get; set; }
        public bool PlayOnlyOnce { get; set; }
        public bool HasBeenPlayed { get; set; }

        public bool ShouldBePlayed()
        {
            if (PlayOnlyOnce is true)
            {
                if (HasBeenPlayed is true)
                {
                    return false;
                }

                HasBeenPlayed = true;
            }

            return true;
        }

        public void Reset()
        {
            HasBeenPlayed = false;
        }

        public override string ToString()
        {
            return $"[Frame: {Frame} | Duration: {Duration}: | Flip: {Flip} | PlayOnlyOnce: {PlayOnlyOnce}]";
        }
    }
}
