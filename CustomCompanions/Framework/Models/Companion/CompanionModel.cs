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
        public int SpawnDirection { get; set; } = 2;
        public int SpawnOffsetX { get; set; }
        public int SpawnOffsetY { get; set; }
        public float MaxIdleDistance { get; set; } = 128f;
        public string IdleBehavior { get; set; } = "NOTHING";
        public float[] IdleArguments { get; set; }
        public List<int[]> Colors { get; set; } = new List<int[]>();
        public bool IsPrismatic { get; set; } = false;
        public LightModel Light { get; set; }
        public List<SoundModel> Sounds { get; set; } = new List<SoundModel>();
        public int FrameSizeWidth { get; set; }
        public int FrameSizeHeight { get; set; }
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
                $"\tTravelSpeed: {TravelSpeed} | SpawnDirection: {SpawnDirection} | SpawnOffsetX: {SpawnOffsetX} | SpawnOffsetY: {SpawnOffsetY}\n" +
                $"\tMaxIdleDistance: {MaxIdleDistance} | IdleBehavior: {IdleBehavior} | IdleArguments: { (IdleArguments is null ? null : IdleArguments) }\n" +
                $"\tColors: {string.Join(",", Colors.Select(c => "[" + string.Join(",", c) + "]"))} | IsPrismatic: {IsPrismatic}\n" +
                $"\tLight: {(Light is null ? null : Light)}\n" +
                $"\tSounds: {string.Join(",", Sounds)}\n" +
                $"\tFrameSize: {FrameSizeWidth}x{FrameSizeHeight} | TileSheetPath: {TileSheetPath}\n" +
                $"\tUniformAnimation: {(UniformAnimation is null ? null : UniformAnimation)}\n" +
                $"\tUpAnimation: {(UpAnimation is null ? null : UpAnimation)}\n" +
                $"\tDownAnimation: {(DownAnimation is null ? null : DownAnimation)}\n" +
                $"\tLeftAnimation: {(LeftAnimation is null ? null : LeftAnimation)}\n" +
                $"\tRightAnimation: {(RightAnimation is null ? null : RightAnimation)}\n]";
        }
    }
}
