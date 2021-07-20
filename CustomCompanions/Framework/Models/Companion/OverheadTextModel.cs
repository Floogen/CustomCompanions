namespace CustomCompanions.Framework.Models.Companion
{
    public class OverheadTextModel
    {
        public string Text { get; set; }
        public float ChanceWeight { get; set; } = 1f;
        public int TextLifetime { get; set; } = 3000;

        public override string ToString()
        {
            return $"[Text: {Text} | ChanceWeight: {ChanceWeight}]";
        }
    }
}
