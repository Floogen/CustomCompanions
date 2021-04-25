namespace CustomCompanions.Framework.Models.Companion
{
    public class SoundModel
    {
        public string SoundName { get; set; }
        public string WhenToPlay { get; set; }
        public int TimeBetweenSound { get; set; }
        public float ChanceOfPlaying { get; set; } = 1f;

        public override string ToString()
        {
            return $"[SoundName: {SoundName} | WhenToPlay: {WhenToPlay} | TimeBetweenSound: {TimeBetweenSound} | ChanceOfPlaying: {ChanceOfPlaying}]";
        }
    }
}
