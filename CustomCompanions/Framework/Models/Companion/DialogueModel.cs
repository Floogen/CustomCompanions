namespace CustomCompanions.Framework.Models.Companion
{
    public class DialogueModel
    {
        public string Text { get; set; }
        public bool DisplayOnce { get; set; }
        internal bool HasBeenDisplayed { get; set; }
        public int PortraitIndex { get; set; } = -1;

        public override string ToString()
        {
            return $"[Text: {Text} | DisplayOnce: {DisplayOnce}]";
        }
    }
}
