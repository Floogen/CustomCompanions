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
        WANDER,
        JUMPER
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
                case "JUMPER":
                    this.behavior = Behavior.JUMPER;
                    break;
                default:
                    this.behavior = Behavior.NOTHING;
                    break;
            }
        }

        internal bool PerformIdleBehavior(Companion companion, GameTime time, float[] arguments)
        {
            // Determine the behavior logic to apply
            if (this.behavior == Behavior.WANDER)
            {
                float dashMultiplier = 2f;
                int maxTimeBetweenDash = 5000;
                if (arguments is null && arguments.Length >= 2)
                {
                    dashMultiplier = arguments[0];
                    maxTimeBetweenDash = (int)arguments[1];
                }

                this.behaviorTimer -= time.ElapsedGameTime.Milliseconds;
                if (this.behaviorTimer <= 0)
                {
                    this.motionMultiplier = dashMultiplier;
                    this.behaviorTimer = Game1.random.Next(maxTimeBetweenDash);
                }

                //Vector2 targetPosition = companion.GetTargetPosition() + new Vector2(companion.model.SpawnOffsetX, companion.model.SpawnOffsetY);
                //Vector2 smoothedPositionSlow = Vector2.Lerp(companion.position, targetPosition, 0.02f);
                //companion.position.Value = smoothedPositionSlow;

                // Get the current motion multiplier
                companion.position.Value += companion.motion.Value * this.motionMultiplier;
                this.motionMultiplier -= 0.0005f * time.ElapsedGameTime.Milliseconds;
                if (this.motionMultiplier <= 1f)
                {
                    this.motionMultiplier = 1f;
                }

                companion.motion.X += Game1.random.Next(-1, 2) * 0.1f;
                companion.motion.Y += Game1.random.Next(-1, 2) * 0.1f;

                if (companion.motion.X < -1f)
                {
                    companion.motion.X = -1f;
                }
                if (companion.motion.X > 1f)
                {
                    companion.motion.X = 1f;
                }
                if (companion.motion.Y < -1f)
                {
                    companion.motion.Y = -1f;
                }
                if (companion.motion.Y > 1f)
                {
                    companion.motion.Y = 1f;
                }

                companion.motion.Value = companion.motion.Value * motionMultiplier;
                return false;
            }
            else if (this.behavior == Behavior.HOVER)
            {
                float hoverCycleTime = 1000;
                if (arguments is null && arguments.Length >= 1)
                {
                    hoverCycleTime = arguments[0];
                }

                behaviorTimer = (behaviorTimer + (float)time.ElapsedGameTime.TotalMilliseconds / hoverCycleTime) % 1;
                companion.motion.Value = new Vector2(0f, 2f * ((float)Math.Sin(2 * Math.PI * behaviorTimer)));

                return true;
            }
            else if (this.behavior == Behavior.JUMPER)
            {
                float jumpScale = 10f;
                float randomJumpBoostMultiplier = 2f;
                if (arguments is null && arguments.Length >= 2)
                {
                    jumpScale = arguments[0];
                    randomJumpBoostMultiplier = arguments[1];
                }

                companion.PerformJumpMovement(jumpScale, randomJumpBoostMultiplier);
                return true;
            }
            else
            {
                companion.motion.Value = Vector2.Zero;
                return true;
            }
        }
    }
}
