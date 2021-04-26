using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Companions
{
    internal enum Behavior
    {
        NOTHING,
        HOVER,
        WANDER
    }

    internal class IdleBehavior
    {
        internal Behavior behavior;

        private float behaviorTimer;
        private float motionMultiplier = 1f;

        internal IdleBehavior(string behaviorType)
        {
            if (String.IsNullOrEmpty(behaviorType))
            {
                this.behavior = Behavior.NOTHING;
                return;
            }

            switch (behaviorType.ToUpper())
            {
                case "HOVER":
                    this.behavior = Behavior.HOVER;
                    break;
                case "WANDER":
                    this.behavior = Behavior.WANDER;
                    break;
                default:
                    this.behavior = Behavior.NOTHING;
                    break;
            }
        }

        internal Vector2 ApplyMotionModifications(Vector2 motion, GameTime time)
        {
            // Determine the behavior logic to apply
            if (this.behavior == Behavior.WANDER)
            {
                this.behaviorTimer -= time.ElapsedGameTime.Milliseconds;
                if (this.behaviorTimer <= 0)
                {
                    this.motionMultiplier = 2f;
                    this.behaviorTimer = Game1.random.Next(1000, 8000);
                }

                // Get the current motion multiplier
                this.motionMultiplier -= 0.0005f * time.ElapsedGameTime.Milliseconds;
                if (this.motionMultiplier <= 1f)
                {
                    this.motionMultiplier = 1f;
                }

                motion.X += Game1.random.Next(-1, 2) * 0.1f;
                motion.Y += Game1.random.Next(-1, 2) * 0.1f;

                if (motion.X < -1f)
                {
                    motion.X = -1f;
                }
                if (motion.X > 1f)
                {
                    motion.X = 1f;
                }
                if (motion.Y < -1f)
                {
                    motion.Y = -1f;
                }
                if (motion.Y > 1f)
                {
                    motion.Y = 1f;
                }

                return motion * motionMultiplier;
            }
            else if (this.behavior == Behavior.HOVER)
            {
                // TODO: Implement parameter so user can pick frequency of HOVER and other idle behaviors
                behaviorTimer = (behaviorTimer + (float)time.ElapsedGameTime.TotalMilliseconds / 1000) % 1;
                return new Vector2(0f, 2f * ((float)Math.Sin(2 * Math.PI * behaviorTimer)));
            }

            return Vector2.Zero;
        }
    }
}
