using CustomCompanions.Framework.Models.Companion;
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
    public class MapCompanion : Companion
    {
        public int pauseTimer;

        public MapCompanion(CompanionModel model, Vector2 targetTile, GameLocation location) : base(model, null, targetTile)
        {
            base.targetTile = targetTile * 64f;
            base.currentLocation = location;

            base.farmerPassesThrough = model.EnableFarmerCollision ? false : true;
            base.SetUpCompanion();
        }

        public override void update(GameTime time, GameLocation location)
        {
            if (!Game1.shouldTimePass())
            {
                return;
            }

            base.currentLocation = location;

            // Update light location, if applicable
            base.UpdateLight(time);

            // Play any sound(s) that are required
            if (Utility.isThereAFarmerWithinDistance(base.getTileLocation(), 10, base.currentLocation) != null)
            {
                base.PlayRequiredSounds(time);
            }

            if (Game1.IsMasterGame && this.behaviors(time, location))
            {
                return;
            }

            if (this.pauseTimer > 0)
            {
                this.pauseTimer -= time.ElapsedGameTime.Milliseconds;
            }
            if (this.shakeTimer > 0)
            {
                this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
            }

            // Do Idle Behaviors here
            this.UpdateRandomMovements();
            CustomCompanions.monitor.Log($"{this.wasIdle} | {this.isMoving()} | {this.previousDirection} | {this.facingDirection}", StardewModdingAPI.LogLevel.Debug);
            base.Animate(time, !this.isMoving());
            base.update(time, location, -1, move: false);

            // Handle any movement
            //this.SetAnimation(time);
            this.wasIdle.Value = !this.isMoving();

            if (!Game1.IsClient)
            {
                this.MovePosition(time, Game1.viewport, location);
            }
        }

        private bool behaviors(GameTime time, GameLocation location)
        {
            if (!Game1.IsClient)
            {
                if (base.controller != null)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool isMoving()
        {
            if (this.pauseTimer > 0)
            {
                return false;
            }

            if (!this.moveUp && !this.moveDown && !this.moveRight && !this.moveLeft)
            {
                return this.position.Field.IsInterpolating();
            }

            return true;
        }

        public void UpdateRandomMovements()
        {
            if (this.pauseTimer > 0)
            {
                return;
            }

            if (!Game1.IsClient && Game1.random.NextDouble() < (this.isMoving() ? 0.007 : 0.1))
            {
                int newDirection = Game1.random.Next(5);
                if (newDirection != (this.FacingDirection + 2) % 4)
                {
                    if (newDirection < 4)
                    {
                        int oldDirection = this.FacingDirection;
                        if (base.currentLocation.isCollidingPosition(this.nextPosition(newDirection), Game1.viewport, this))
                        {
                            return;
                        }
                    }
                    switch (newDirection)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            this.FaceAndMoveInDirection(newDirection);
                            break;
                        default:
                            this.Halt();
                            this.Sprite.StopAnimation();
                            this.Sprite.UpdateSourceRect();
                            this.pauseTimer = Game1.random.Next(2000, 10000);
                            break;
                    }
                }
            }
        }

        public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            if (this.pauseTimer > 0 || Game1.IsClient)
            {
                if (this.previousDirection.Value != this.FacingDirection)
                {
                    this.previousDirection.Value = this.facingDirection;
                }
                return;
            }

            Location next_tile = base.nextPositionTile();
            if (!currentLocation.isTileOnMap(new Vector2(next_tile.X, next_tile.Y)))
            {
                this.FaceAndMoveInDirection(Utility.GetOppositeFacingDirection(base.FacingDirection));
                return;
            }
            if (base.moveUp)
            {
                if (!currentLocation.isCollidingPosition(this.nextPosition(0), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
                {
                    base.position.Y -= base.speed;
                    this.FaceAndMoveInDirection(0);
                }
                else if (!this.HandleCollision(this.nextPosition(0)))
                {
                    if (Game1.random.NextDouble() < 0.6)
                    {
                        this.FaceAndMoveInDirection(2);
                    }
                }
            }
            else if (base.moveRight)
            {
                if (!currentLocation.isCollidingPosition(this.nextPosition(1), Game1.viewport, isFarmer: false, 0, glider: false, this))
                {
                    base.position.X += base.speed;
                    this.FaceAndMoveInDirection(1);
                }
                else if (!this.HandleCollision(this.nextPosition(1)))
                {
                    if (Game1.random.NextDouble() < 0.6)
                    {
                        this.FaceAndMoveInDirection(3);
                    }
                }
            }
            else if (base.moveDown)
            {
                if (!currentLocation.isCollidingPosition(this.nextPosition(2), Game1.viewport, isFarmer: false, 0, glider: false, this))
                {
                    base.position.Y += base.speed;
                    this.FaceAndMoveInDirection(2);
                }
                else if (!this.HandleCollision(this.nextPosition(2)))
                {
                    if (Game1.random.NextDouble() < 0.6)
                    {
                        this.FaceAndMoveInDirection(0);
                    }
                }
            }
            else if (base.moveLeft)
            {
                if (!currentLocation.isCollidingPosition(this.nextPosition(3), Game1.viewport, isFarmer: false, 0, glider: false, this))
                {
                    base.position.X -= base.speed;
                    this.FaceAndMoveInDirection(3);
                }
                else if (!this.HandleCollision(this.nextPosition(3)))
                {
                    if (Game1.random.NextDouble() < 0.6)
                    {
                        this.FaceAndMoveInDirection(1);
                    }
                }
            }

            var targetDistance = Vector2.Distance(base.Position, this.GetTargetPosition());
            if (targetDistance > this.model.MaxDistanceBeforeTeleport && this.model.MaxDistanceBeforeTeleport != -1)
            {
                base.position.Value = this.GetTargetPosition();
            }
            else if (targetDistance > this.model.MaxIdleDistance && this.model.MaxIdleDistance != -1)
            {
                this.FaceAndMoveInDirection(this.getGeneralDirectionTowards(this.GetTargetPosition(), 0, opposite: false, useTileCalculations: false));

                if (Game1.random.NextDouble() <= 0.25)
                {
                    this.pauseTimer = Game1.random.Next(2000, 10000);
                }
            }
        }

        public override void Halt()
        {
            base.Halt();
            base.speed = this.model.TravelSpeed;
            base.addedSpeed = 0;
        }

        public bool HandleCollision(Microsoft.Xna.Framework.Rectangle next_position)
        {
            if (Game1.random.NextDouble() <= 0.25)
            {
                this.pauseTimer = Game1.random.Next(2000, 10000);
            }

            return false;
        }

        internal void FaceAndMoveInDirection(int direction)
        {
            this.SetFacingDirection(direction);
            this.SetMovingDirection(direction);
        }

        internal void SetAnimation(GameTime time)
        {
            bool hasIdleFrames = base.HasIdleFrames(this.idleUniformFrames != null ? -1 : this.FacingDirection);

            if (this.Sprite.CurrentAnimation != null && (this.previousDirection == this.FacingDirection || this.activeUniformFrames != null))
            {
                if (!hasIdleFrames || ((base.IsPlayingIdleFrames(this.FacingDirection) && !this.isMoving()) || (!base.IsPlayingIdleFrames(this.FacingDirection) && this.isMoving())))
                {
                    if (!this.Sprite.animateOnce(time))
                    {
                        return;
                    }
                }
            }

            if (this.Sprite.CurrentAnimation == this.idleUniformFrames)

                if (!this.isMoving() && hasIdleFrames)
                {
                    if (this.idleUniformFrames != null)
                    {
                        this.Sprite.setCurrentAnimation(this.idleUniformFrames);
                    }
                    else
                    {
                        switch (this.FacingDirection)
                        {
                            case 0:
                                this.Sprite.setCurrentAnimation(this.idleUpFrames);
                                break;
                            case 1:
                                this.Sprite.setCurrentAnimation(this.idleRightFrames);
                                break;
                            case 2:
                                this.Sprite.setCurrentAnimation(this.idleDownFrames);
                                break;
                            case 3:
                                this.Sprite.setCurrentAnimation(this.idleLeftFrames);
                                break;
                        }
                    }
                }
                else
                {
                    if (this.activeUniformFrames != null)
                    {
                        this.Sprite.setCurrentAnimation(this.activeUniformFrames);
                    }
                    else
                    {
                        switch (this.FacingDirection)
                        {
                            case 0:
                                this.Sprite.setCurrentAnimation(this.activeUpFrames);
                                break;
                            case 1:
                                this.Sprite.setCurrentAnimation(this.activeRightFrames);
                                break;
                            case 2:
                                this.Sprite.setCurrentAnimation(this.activeDownFrames);
                                break;
                            case 3:
                                this.Sprite.setCurrentAnimation(this.activeLeftFrames);
                                break;
                        }
                    }
                }

            this.Sprite.animateOnce(time);
        }

    }
}