using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace CustomCompanions.Framework.Models.Companion
{
    public class CompanionModel
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int TravelSpeed { get; set; }
        public int SpawnOffsetX { get; set; }
        public int SpawnOffsetY { get; set; }
        public string IdleBehavior { get; set; } = "NOTHING";
        public List<int[]> Colors { get; set; } = new List<int[]>();
        public bool IsPrismatic { get; set; } = false;
        public LightModel Light { get; set; }
        public List<SoundModel> Sounds { get; set; }
        public int FrameSize { get; set; }
        public string TileSheetPath { get; set; }
        public AnimationModel UniformAnimation { get; set; }
        public AnimationModel UpAnimation { get; set; }
        public AnimationModel DownAnimation { get; set; }
        public AnimationModel LeftAnimation { get; set; }
        public AnimationModel RightAnimation { get; set; }

        public override string ToString()
        {
            return $"\n[\n" +
                $"\tOwner: {Owner} | Name: {Name} | Type: {Type}\n" +
                $"\tTravelSpeed: {TravelSpeed} | SpawnOffsetX: {SpawnOffsetX} | SpawnOffsetY: {SpawnOffsetY}\n" +
                $"\tIdleBehavior: {IdleBehavior}\n" +
                $"\tColors: {string.Join(",", Colors.Select(c => "[" + string.Join(",", c) + "]"))} | IsPrismatic: {IsPrismatic}\n" +
                $"\tLight: {Light}\n" +
                $"\tSounds: {string.Join(",", Sounds)}\n" +
                $"\tFrameSize: {FrameSize} | TileSheetPath: {TileSheetPath}\n" +
                $"\tUniformAnimation: {UniformAnimation}\n" +
                $"\tUpAnimation: {UpAnimation}\n" +
                $"\tDownAnimation: {DownAnimation}\n" +
                $"\tLeftAnimation: {LeftAnimation}\n" +
                $"\tRightAnimation: {RightAnimation}\n]";
        }
    }
}
