using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

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
                if (companion.IsFlying())
                {
                    float dashMultiplier = 2f;
                    int minTimeBetweenDash = 5000;
                    if (arguments != null && arguments.Length >= 2)
                    {
                        dashMultiplier = arguments[0];
                        minTimeBetweenDash = (int)arguments[1];
                    }

                    this.behaviorTimer -= time.ElapsedGameTime.Milliseconds;
                    if (this.behaviorTimer <= 0)
                    {
                        this.motionMultiplier = dashMultiplier;
                        this.behaviorTimer = Game1.random.Next(minTimeBetweenDash, minTimeBetweenDash * 2);
                    }

                    // Get the current motion multiplier
                    companion.position.Value += companion.motion.Value * this.motionMultiplier;
                    this.motionMultiplier -= 0.0005f * time.ElapsedGameTime.Milliseconds;
                    if (this.motionMultiplier <= 0f)
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

                    return false;
                }
                else
                {
                    this.behaviorTimer -= time.ElapsedGameTime.Milliseconds;
                    if (this.behaviorTimer >= 0)
                    {
                        companion.Halt();
                        companion.motion.Value = Vector2.Zero;
                        return false;
                    }
                    companion.motion.Value = Vector2.One;

                    int targetDirection = companion.FacingDirection;
                    if (Game1.random.NextDouble() < 0.007)
                    {
                        int newDirection = Game1.random.Next(5);
                        if (newDirection != (companion.FacingDirection + 2) % 4)
                        {
                            if (newDirection < 4)
                            {
                                companion.Halt();
                                targetDirection = newDirection;

                                if (companion.currentLocation.isCollidingPosition(companion.nextPosition(newDirection), Game1.viewport, companion))
                                {
                                    companion.SetMovingDirection(companion.previousDirection);
                                    this.behaviorTimer = Game1.random.Next(2000, 10000);
                                    return false;
                                }

                                companion.previousDirection.Value = companion.FacingDirection;
                                companion.SetMovingDirection(targetDirection);
                            }
                        }
                    }

                    Location next_tile = companion.nextPositionTile();
                    if (!companion.currentLocation.isTileOnMap(new Vector2(next_tile.X, next_tile.Y)) || companion.currentLocation.isCollidingPosition(companion.nextPosition(targetDirection), Game1.viewport, isFarmer: true, 0, glider: false, companion, pathfinding: false))
                    {
                        companion.SetMovingDirection(companion.facingDirection);
                        this.behaviorTimer = Game1.random.Next(2000, 10000);

                        if (Game1.random.NextDouble() < 0.6)
                        {
                            companion.SetMovingDirection(Utility.GetOppositeFacingDirection(companion.facingDirection));
                        }
                    }
                    else
                    {
                        switch (targetDirection)
                        {
                            case 0:
                                companion.position.Y -= companion.speed;
                                break;
                            case 1:
                                companion.position.X += companion.speed;
                                break;
                            case 2:
                                companion.position.Y += companion.speed;
                                break;
                            case 3:
                                companion.position.X -= companion.speed;
                                break;
                        }
                    }

                    return false;
                }
            }
            else if (this.behavior == Behavior.HOVER)
            {
                float hoverCycleTime = 1000;
                if (arguments != null && arguments.Length >= 1)
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
                if (arguments != null && arguments.Length >= 2)
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
