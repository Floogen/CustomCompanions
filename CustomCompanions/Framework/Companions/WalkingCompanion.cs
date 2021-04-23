using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Companions
{
    public class WalkingCompanion : NPC
    {
        public WalkingCompanion(string companionName, string texturePath, Vector2 spawnPosition) : base(new AnimatedSprite(texturePath, 0, 16, 16), spawnPosition * 64f, 2, companionName)
        {

        }
    }
}
