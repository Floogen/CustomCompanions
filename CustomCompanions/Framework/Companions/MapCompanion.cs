﻿using CustomCompanions.Framework.Models.Companion;
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
        private int pauseTimer;
        private bool canHalt;
        private float motionMultiplier;

        public MapCompanion(CompanionModel model, Vector2 targetTile, GameLocation location) : base(model, null, targetTile)
        {
            base.targetTile = targetTile * 64f;
            base.currentLocation = location;

            base.farmerPassesThrough = model.EnableFarmerCollision ? false : true;
            base.SetUpCompanion();

            this.canHalt = !IsFlying();
            this.motionMultiplier = 1f;
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

            // Timers
            if (this.pauseTimer > 0)
            {
                this.pauseTimer -= time.ElapsedGameTime.Milliseconds;
            }
            if (this.shakeTimer > 0)
            {
                this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
            }

            // Do Idle Behaviors
            this.PerformBehavior(base.idleBehavior.behavior, time, location);
            //CustomCompanions.monitor.Log($"{this.wasIdle} | {this.isMoving()} | {this.previousDirection} | {this.facingDirection}", StardewModdingAPI.LogLevel.Debug);
        }

        public override bool isMoving()
        {
            if (this.pauseTimer > 0 && canHalt)
            {
                return false;
            }

            if (!this.moveUp && !this.moveDown && !this.moveRight && !this.moveLeft)
            {
                return this.position.Field.IsInterpolating();
            }

            return true;
        }

        public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            // Unused
            return;
        }

        public override void Halt()
        {
            if (canHalt)
            {
                base.Halt();
                base.Sprite.UpdateSourceRect();
                base.speed = this.model.TravelSpeed;
                base.addedSpeed = 0;
            }
        }

        internal void FaceAndMoveInDirection(int direction)
        {
            this.SetFacingDirection(direction);
            this.SetMovingDirection(direction);
        }
        private bool HandleCollision(Microsoft.Xna.Framework.Rectangle next_position)
        {
            if (Game1.random.NextDouble() <= 0.25)
            {
                this.pauseTimer = Game1.random.Next(2000, 10000);
            }

            return false;
        }

        private void AttemptRandomDirection(float chanceWhileMoving, float chanceWhileIdle)
        {
            if (this.pauseTimer > 0 && canHalt)
            {
                return;
            }

            if (!Game1.IsClient && Game1.random.NextDouble() < (this.isMoving() ? chanceWhileMoving : chanceWhileIdle))
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
                            this.pauseTimer = Game1.random.Next(2000, 10000);
                            break;
                    }
                }
            }
        }

        private void MovePositionViaSpeed(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            if (this.pauseTimer > 0 && canHalt)
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
                    base.position.Y -= base.speed + base.addedSpeed;
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
                    base.position.X += base.speed + base.addedSpeed;
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
                    base.position.Y += base.speed + base.addedSpeed;
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
                    base.position.X -= base.speed + base.addedSpeed;
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

        private void MovePositionViaMotion(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            if (this.pauseTimer > 0 && canHalt)
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
                base.motion.Y -= Game1.random.Next(1, 2) * 0.1f;
                this.FaceAndMoveInDirection(0);
            }
            else if (base.moveRight)
            {
                base.motion.X += Game1.random.Next(1, 2) * 0.1f;
                this.FaceAndMoveInDirection(1);
            }
            else if (base.moveDown)
            {
                base.motion.Y += Game1.random.Next(1, 2) * 0.1f;
                this.FaceAndMoveInDirection(2);
            }
            else if (base.moveLeft)
            {
                base.motion.X -= Game1.random.Next(1, 2) * 0.1f;
                this.FaceAndMoveInDirection(3);
            }

            // Update position
            base.Position += this.motion.Value * this.motionMultiplier * base.model.TravelSpeed;
            this.motionMultiplier -= 0.0005f * time.ElapsedGameTime.Milliseconds;
            if (this.motionMultiplier < 1f)
            {
                this.motionMultiplier = 1f;
            }

            // Restrict motion
            this.KeepMotionWithinBounds(1f, 1f);

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

        private void KeepMotionWithinBounds(float xBounds, float yBounds)
        {
            if (base.motion.X < Math.Abs(xBounds) * -1)
            {
                base.motion.X = Math.Abs(xBounds) * -1;
            }
            if (base.motion.X > Math.Abs(xBounds))
            {
                base.motion.X = Math.Abs(xBounds);
            }
            if (base.motion.Y < Math.Abs(yBounds) * -1)
            {
                base.motion.Y = Math.Abs(yBounds) * -1;
            }
            if (base.motion.Y > Math.Abs(yBounds))
            {
                base.motion.Y = Math.Abs(yBounds);
            }
        }

        private bool PerformBehavior(Behavior behavior, GameTime time, GameLocation location)
        {
            switch (behavior)
            {
                case Behavior.WANDER:
                    if (base.IsFlying())
                    {
                        DoWanderFly(time, location);
                    }
                    else
                    {
                        DoWanderWalk(time, location);
                    }
                    return true;
            }

            return false;
        }

        private void DoWanderFly(GameTime time, GameLocation location)
        {
            // Handle random directional changes
            this.AttemptRandomDirection(0.007f, 0.1f);

            // Handle animating
            base.Animate(time, !this.isMoving());
            base.update(time, location, -1, move: false);
            base.wasIdle.Value = !this.isMoving();

            this.MovePositionViaMotion(time, Game1.viewport, location);
        }

        private void DoWanderWalk(GameTime time, GameLocation location)
        {
            // Handle random movement
            this.AttemptRandomDirection(0.007f, 0.1f);

            // Handle animating
            base.Animate(time, !this.isMoving());
            base.update(time, location, -1, move: false);
            base.wasIdle.Value = !this.isMoving();

            this.MovePositionViaSpeed(time, Game1.viewport, location);
        }
    }
}